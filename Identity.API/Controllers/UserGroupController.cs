using CommonUtil;
using CommonUtil.Helpers;
using Identity.API.Data;
using Identity.API.Data.Repositories;
using Identity.API.Entities;
using Identity.API.Logic.Enums;
using Identity.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Identity.API.Controllers
{
    [Route("api/usergroup")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Produces("application/json")]
    public class UserGroupController : Controller
    {
        private readonly ILogger _logger;
        private readonly IAppGroupRepository _groupRepo;
        private readonly IAppPermissionRepository _permissionRepo;
        private readonly ICurrentUser _currentUser;

        public UserGroupController(
           ILogger<UserGroupController> logger,
           UnitOfWork unitOfWork,
           ICurrentUser currentUser)
        : base(unitOfWork, logger)
        {
            _logger = logger;
            _groupRepo = new AppGroupRepository(unitOfWork);
            _permissionRepo = new AppPermissionRepository(unitOfWork);
            _currentUser = currentUser;
        }

        /// <summary>
        /// Get full user group details along with assigned permissions and users (Authenticated: User account admin permission is required)
        /// </summary>
        /// <param name="id">user group id</param>
        [HttpGet("")]
        [ProducesResponseType(typeof(Envelope<UserGroupDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult Get([FromQuery] int id)
        {
            if (!_currentUser.HasRole(Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Retrieving user group details: {0}", id);

            Maybe<AppGroup> userGroup = _groupRepo.GetById(id);
            if (userGroup.HasNoValue)
                return Error($"No matching user group found: {id}");

            var result = userGroup.Unwrap(d => new UserGroupDTO
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                UsersCount = d.UserGroups.Count(),
                PermissionsCount = d.GroupPermissions.Count(),
                Permissions = d.GroupPermissions.Select(p => new GroupPermissionDTO
                {
                    Id = p.Id,
                    PermissionId = p.PermissionId,
                    Name = p.Permission.Name,
                    Description = p.Permission.Description
                }),
                AppUsers = d.UserGroups.Select(u => new GroupUserDTO
                {
                    Id = u.Id,
                    UserId = u.AppUserId,
                    Email = u.AppUser.Email ?? u.AppUser.Username,
                    Fullname = u.AppUser.Fullname
                })
            });

            return Ok(result, contextReadonly: true);
        }

        /// <summary>
        /// List all available user groups (Authenticated: User account admin permission is required)
        /// </summary>
        [HttpGet("list")]
        [ProducesResponseType(typeof(Envelope<IEnumerable<UserGroupDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult GetList()
        {
            if (!_currentUser.HasRole(Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Retrieving user group list");

            List<AppGroup> appGroups = _groupRepo.GetList().ToList();
            var result = appGroups.Select(d => new UserGroupDTO
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                UsersCount = d.UserGroups.Count(),
                PermissionsCount = d.GroupPermissions.Count()
            });

            return Ok(result, contextReadonly: true);
        }

        /// <summary>
        /// Register a new user account (Authenticated: User account admin permission is required)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST {baseurl}/{api_endpoint}
        ///     {        
        ///       "name": "Admin",
        ///       "description": "Administrator permissions",
        ///       "selectedPermissions": ["UserAccountAdmin","SampleModuleViewer"]
        ///     }
        /// </remarks>
        [HttpPost("")]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult Create([FromBody] CreateUserGroupModel model)
        {
            if (!_currentUser.HasRole(Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Creating a new user group: {0}", model.Name);
            _logger.LogDebug("Request body: {0}", JsonSerializer.Serialize(model));

            Result<string> name = ((Maybe<string>)model.Name)
                .ToResult("User group name is not specified")
                .OnSuccess(d => d.Trim())
                .Ensure(d => d.Length >= 5, "User group name is too short")
                .Ensure(d => d.Length <= 50, "User group name is too long")
                .Ensure(d => new Regex(@"^([a-zA-Z0-9 ])*$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2)).IsMatch(d)
                    , "User group contains invalid characters");

            Result<string> description = ((Maybe<string>)model.Description)
                .ToResult("User group description is not specified")
                .OnSuccess(d => d.Trim())
                .Ensure(d => d.Length <= 250, "User group name is too long");

            Result result = Result.Combine(name, description);
            if (result.IsFailure)
                return Error(result.Error);

            if (_groupRepo.HasGroup(name.Value))
                return Error("Specified user group already exist");

            AppGroup group = new AppGroup(name.Value, description.Value);

            model.SelectedPermissions ??= new string[] { };

            if (model.SelectedPermissions.Any()) // has selected permissions
            {
                Result<List<AppPermission>> selectedPermissions = GetMatchingPermissions(_permissionRepo, model.SelectedPermissions);
                if (selectedPermissions.IsFailure)
                    return Error(selectedPermissions.Error);

                group.AddPermissions(selectedPermissions.Value);
            }

            _groupRepo.Insert(group);

            return Ok("User group added");
        }

        private Result<List<AppPermission>> GetMatchingPermissions(
            IAppPermissionRepository permissionRepo,
            string[] permissionNames
        )
        {
            if (permissionRepo == null)
                return Result.Fail<List<AppPermission>>("Permission repo is not specified");

            if (permissionNames == null)
                return Result.Fail<List<AppPermission>>("No permissions are specified");

            permissionNames = permissionNames.Select(d => d.Trim().ToUpper()).Distinct().ToArray();

            List<AppPermission> permissions = permissionRepo.GetList().ToList();
            List<AppPermission> result = new List<AppPermission>();

            foreach (var name in permissionNames)
            {
                Maybe<AppPermission> permission = permissions.FirstOrDefault(d => d.Name.ToUpper() == name);
                if (permission.HasNoValue)
                    return Result.Fail<List<AppPermission>>($"No matching permission found: {name}");

                result.Add(permission.Value);
            }

            return Result.Ok(result);
        }

        /// <summary>
        /// Delete an existin user group (Authenticated: User account admin permission is required)
        /// </summary>
        /// <param name="name">User group name</param>
        [HttpDelete("")]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult Delete([FromQuery] string name)
        {
            if (!_currentUser.HasRole(Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Removing user group: {0}", name);

            // determine whether specified user group is related to a system group or not. Deleting one of system group is not allowed
            if (EnumInfo.GetList<SystemUserGroup>().Any(systemUserGroup => systemUserGroup.Name.ToUpper() == name.ToUpper()))
                return Error("Removing a system user group is not permitted");

            Maybe<AppGroup> userGroup = _groupRepo.FindByName(name);
            if (userGroup.HasNoValue)
                return Error($"No matching user group found: {name}");

            // remove user groups and their aggregates
            _groupRepo.Delete(userGroup.Value.Id);

            return Ok("User group removed");
        }

        /// <summary>
        /// Edit an existing user group (Authenticated: User account admin permission is required)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT {baseurl}/{api_endpoint}
        ///     {        
        ///       "id": "1",
        ///       "name": "Admin",
        ///       "description": "Administrator permissions" ,
        ///       "selectedPermissions": ["UserAccountAdmin","SampleModuleViewer"]
        ///     }
        /// </remarks>
        [HttpPut("")]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult Edit([FromBody] EditUserGroupModel model)
        {
            if (!_currentUser.HasRole(Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Updating user group: {0}", model.Name);

            Result<string> name = ((Maybe<string>)model.Name)
               .ToResult("User group name is not specified")
               .OnSuccess(d => d.Trim())
               .Ensure(d => d.Length >= 5, "User group name is too short")
               .Ensure(d => d.Length <= 50, "User group name is too long")
               .Ensure(d => new Regex(@"^([a-zA-Z0-9 ])*$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2)).IsMatch(d)
                   , "User group contains invalid characters");

            Result<string> description = ((Maybe<string>)model.Description)
                .ToResult("User group description is not specified")
                .OnSuccess(d => d.Trim())
                .Ensure(d => d.Length <= 250, "User group name is too long");

            Result result = Result.Combine(name, description);
            if (result.IsFailure)
                return Error(result.Error);

            Maybe<AppGroup> userGroup = _groupRepo.GetById(model.Id);
            if (userGroup.HasNoValue)
                return Error($"No matching user group found: {model.Id}");

            userGroup.Value.Update(name.Value, description.Value);

            if (!userGroup.Value.IsNameSimilarTo(name.Value)) // if there is a group name change
            {
                if (_groupRepo.HasGroup(name.Value))
                    return Error("Specified user group already exist");
            }

            model.SelectedPermissions ??= new string[] { };

            Result<List<AppPermission>> selectedPermissions = GetMatchingPermissions(_permissionRepo, model.SelectedPermissions);
            if (selectedPermissions.IsFailure)
                return Error(selectedPermissions.Error);

            userGroup.Value.UpdatePermissions(selectedPermissions.Value);
            _groupRepo.Update(userGroup.Value);

            return Ok("User group updated");
        }

        /// <summary>
        /// List not assigned permissions list for a user group (Authenticated: User account admin permission is required)
        /// </summary>
        /// <param name="id">user group id</param>
        [HttpGet("notassigned/permissions")]
        [ProducesResponseType(typeof(Envelope<IEnumerable<PermissionDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult GetNotAssignedPermissions([FromQuery] int id)
        {
            if (!_currentUser.HasRole(Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Retrieving not assigned permissions list for user group: {0}", id);

            Maybe<AppGroup> userGroup = _groupRepo.GetById(id);
            if (userGroup.HasNoValue)
                return Error($"No matching user group found: {id}");

            List<AppPermission> permissions = _permissionRepo.GetList().ToList();
            List<int> assignedPermissions = userGroup.Value.GroupPermissions.Select(d => d.PermissionId).ToList();

            var result = permissions
                .Where(p => !assignedPermissions.Any(d => d == p.Id)) // exclude already assigned permissions to specified group
                .Join(EnumInfo.GetList<Permission>().ToList(), p => p.Name, all => all.Name, (p, all) => new { p, all })
                .Select(d => new PermissionDTO
                {
                    Id = d.p.Id,
                    Name = d.p.Name,
                    Description = d.p.Description,
                    Group = d.all.Group,
                    Active = true
                });

            return Ok(result, contextReadonly: true);
        }
    }
}
