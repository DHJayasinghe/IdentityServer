﻿using CommonUtil;
using CommonUtil.Helpers;
using Identity.API.Data;
using Identity.API.Data.Repositories;
using Identity.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Identity.API.Controllers
{
    [Route("api/permission")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PermissionController : Controller
    {
        private readonly ILogger _logger;
        private readonly IAppPermissionRepository _permissionRepo;
        private readonly ICurrentUser _currentUser;

        public PermissionController(
           ILogger<PermissionController> logger,
           UnitOfWork unitOfWork,
           ICurrentUser currentUser)
        : base(unitOfWork, logger)
        {
            _logger = logger;
            _permissionRepo = new AppPermissionRepository(unitOfWork);
            _currentUser = currentUser;
        }

        [HttpGet("list")]
        public IActionResult GetList()
        {
            if (!_currentUser.HasRole( Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Retrieving system modules permission list");

            List<EnumDescription> permissions = EnumInfo.GetList<Permission>().ToList();
            List<AppPermission> activePermissions = _permissionRepo.GetList().ToList();

            var result = permissions.GroupJoin(
                activePermissions,
                all => all.Name,
                active => active.Name,
                (all, active) => new
                {
                    all,
                    active = (Maybe<AppPermission>)active.FirstOrDefault()
                })
                .Select(d => new
                {
                    Id = d.active.HasValue ? d.active.Unwrap(p => p.Id) : default(int?),
                    d.all.Name,
                    d.all.Description,
                    d.all.Group,
                    Active = d.active.HasValue
                });


            return Ok(result, contextReadonly: true);
        }

        [HttpPost("")]
        public IActionResult Create(string name)
        {
            if (!_currentUser.HasRole(Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Creating a new permission: {0}", name);

            if (!Enum.TryParse(name, ignoreCase: true, out Permission perm))
                return Error($"Specified permission is not valid: {name}");

            if (_permissionRepo.HasPermission(name))
                return Error("Specified permission already exist");

            AppPermission permission = new AppPermission(
                name: perm.GetName(),
                description: perm.GetDescription());
            _permissionRepo.Insert(permission);

            return Ok("Permission added");
        }

        [HttpDelete("")]
        public IActionResult Delete(string name)
        {
            if (!_currentUser.HasRole(Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Removing a permission: {0}", name);

            if (!Enum.TryParse(name, ignoreCase: true, out Permission perm))
                return Error($"Specified permission is not valid: {name}");

            Maybe<AppPermission> permission = _permissionRepo.FindByName(name);
            if (permission.HasNoValue)
                return Error("Specified permission is not found");

            _permissionRepo.Delete(permission.Value.Id);

            return Ok("Permission removed");
        }
    }
}