using CommonUtil;
using CommonUtil.ValueTypes;
using Identity.API.Entities;
using Identity.API.Logic.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Identity.API.Data
{
    public static class PrepareDatabase
    {
        public static void PrepPopulation(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<IdentityDBContext>());
            }
        }

        public static void SeedData(IdentityDBContext context)
        {
            Console.WriteLine("Applying database migrations...");

            context.Database.Migrate();

            Console.WriteLine("Seeding initial data...");

            bool newEntry = false;
            Maybe<AppUser> adminUser = context.AppUser.FirstOrDefault(e => e.Username == "identity.admin@gmail.com");
            if (adminUser.HasNoValue)
            {
                adminUser = new AppUser(
                    firstName: "Dhanuka",
                    lastName: "Jayasinghe",
                    username: Email.Create("identity.admin@gmail.com").Value,
                    password: Password.Create("Admin@#456").Value
                );
                context.AppUser.Add(adminUser.Value);
                newEntry = true;
            }

            Maybe<AppPermission> iamAdminPermission = context.AppPermission.FirstOrDefault(e => e.Name == Permission.UserAccountAdmin.ToString());
            if (iamAdminPermission.HasNoValue)
            {
                iamAdminPermission = new AppPermission(
                    name: Permission.UserAccountAdmin.ToString(),
                    description: EnumInfo.GetDescription(Permission.UserAccountAdmin)
                );
                context.AppPermission.Add(iamAdminPermission.Value);
                newEntry = true;
            }

            Maybe<AppGroup> adminGroup = context.AppGroup.FirstOrDefault(e => e.Name == SystemUserGroup.ADMIN.ToString());
            if (adminGroup.HasNoValue)
            {
                adminGroup = new AppGroup(SystemUserGroup.ADMIN.ToString(), "System administrator permissions");
                context.AppGroup.Add(adminGroup.Value);
                newEntry = true;
            }

            if (newEntry)
            {
                context.SaveChanges();
                newEntry = false;
            }

            if (!context.AppGroupPermission.Any(e => e.PermissionId == iamAdminPermission.Value.Id && e.AppGroupId == adminGroup.Value.Id))
            {
                AppGroupPermission groupPermission = new AppGroupPermission(iamAdminPermission, adminGroup);
                context.AppGroupPermission.Add(groupPermission);
                newEntry = true;
            }
            if (!context.AppUserGroup.Any(e => e.AppUserId == adminUser.Value.Id && e.AppGroupId == adminGroup.Value.Id))
            {
                AppUserGroup adminUserGroup = new AppUserGroup(adminUser, adminGroup);
                context.AppUserGroup.Add(adminUserGroup);
                newEntry = true;
            }

            if (newEntry)
                context.SaveChanges();

            Console.WriteLine("Seeding complete");
        }
    }
}
