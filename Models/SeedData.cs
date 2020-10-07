using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OctoCodes.Controllers;
using OctoCodes.Data;

namespace OctoCodes.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var ctx = new OctoCodesContext(serviceProvider.GetRequiredService<DbContextOptions<OctoCodesContext>>());

            if(ctx.Users.Any())
                return;

            ctx.Users.Add(
                new User
                {
                    Username = "admin",
                    Password = AdminController.EncryptPassword("Password1")
                }
            );
            ctx.SaveChanges();
        }
    }
}
