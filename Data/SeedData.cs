using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoleBasedApplication.Authorization;
using RoleBasedApplication.Models;

namespace RoleBasedApplication.Data;

public static class SeedData
{
    #region snippet_Initialize

    public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw = "")
    {
        using (var context = new ApplicationDbContext(
                   serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
        {
            //Admin and Manager Credentials

            var adminID = await EnsureUser(serviceProvider, "Admin@123", "admin@bigdata.com");
            await EnsureRole(serviceProvider, adminID, Constants.ContactAdministratorsRole);

            var managerID = await EnsureUser(serviceProvider, "Tester@123", "manager@bigdata.com");
            await EnsureRole(serviceProvider, managerID, Constants.ContactManagersRole);

            SeedDB(context, adminID);
        }
    }

    private static async Task<string> EnsureUser(IServiceProvider serviceProvider,
        string testUserPw, string UserName)
    {
        var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

        var user = await userManager.FindByNameAsync(UserName);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = UserName,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, testUserPw);
        }

        if (user == null)
        {
            throw new Exception("Try to make password more stronger!");
        }

        return user.Id;
    }

    private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider,
        string uid, string role)
    {
        var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

        if (roleManager == null)
        {
            throw new Exception("roleManager null");
        }

        IdentityResult IR;
        if (!await roleManager.RoleExistsAsync(role))
        {
            IR = await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

        var user = await userManager.FindByIdAsync(uid);

        if (user == null)
        {
            throw new Exception("Try to make password strong enough!");
        }

        IR = await userManager.AddToRoleAsync(user, role);

        return IR;
    }

    #endregion

    #region snippet1

    public static void SeedDB(ApplicationDbContext context, string adminID)
    {
        if (context.Contact.Any())
        {
            return; // DB has been seeded
        }

        context.Contact.AddRange(

            #region snippet_Contact

            new Contact
            {
                Name = "Meru Sangroula",
                Address = "123 Rose Street",
                City = "Barrie",
                State = "ON",
                Zip = "1235",
                Email = "meru@example.com",
                Status = ContactStatus.Approved,
                OwnerID = adminID
            },

            #endregion

            #endregion

            new Contact
            {
                Name = "Shrasth Kumar",
                Address = "789 Grove Street",
                City = "Brampton",
                State = "ON",
                Zip = "1489",
                Email = "shrasth@example.com",
                Status = ContactStatus.Submitted,
                OwnerID = adminID
            },
            new Contact
            {
                Name = "Irem Kaymakcilar",
                Address = "334 Street",
                City = "Toronto",
                State = "ON",
                Zip = "1355",
                Email = "irem@example.com",
                Status = ContactStatus.Submitted,
                OwnerID = adminID
            }
        );
        context.SaveChanges();
    }
}