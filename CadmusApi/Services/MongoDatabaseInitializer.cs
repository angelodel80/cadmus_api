﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CadmusApi.Models;

namespace CadmusApi.Services
{
    /// <summary>
    /// MongoDB database initializer.
    /// </summary>
    /// <seealso cref="CadmusApi.Services.IDatabaseInitializer" />
    public sealed class MongoDatabaseInitializer : IDatabaseInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDatabaseInitializer"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="roleManager">The role manager.</param>
        public MongoDatabaseInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Seeds the database.
        /// </summary>
        /// <returns></returns>
        public async Task Seed()
        {
            Serilog.Log.Information("Seeding users");

            const string email = "fake@nowhere.com";
            ApplicationUser user;
            if (await _userManager.FindByEmailAsync(email) == null)
            {
                // use the create rather than addorupdate so can set password
                user = new ApplicationUser
                {
                    UserName = "zeus",
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = "Daniele",
                    LastName = "Fusi"
                };
                await _userManager.CreateAsync(user, "P4ssw0rd!");
            }

            user = await _userManager.FindByEmailAsync(email);
            string roleName = "admin";
            if (await _roleManager.FindByNameAsync(roleName) == null)
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });

            if (!await _userManager.IsInRoleAsync(user, roleName))
                await _userManager.AddToRoleAsync(user, roleName);
        }
    }
}