using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommonUtil;
using CommonUtil.Helpers;
using CommonUtil.Models;
using CommonUtil.Specification;
using CommonUtil.ValueTypes;
using Identity.API.Common;
using Identity.API.Data;
using Identity.API.Data.Repositories;
using Identity.API.Entities;
using Identity.API.Logic.Specifications;
using Identity.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.API.Controllers
{
    [Route("api/account")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Produces("application/json")]
    public sealed class AccountController : Controller
    {
        public static ConcurrentDictionary<string, string> RefereshTokenStore = new ConcurrentDictionary<string, string>();

        private readonly ILogger _logger;
        private readonly IJwtFactory _jwtFactory;
        private readonly ITokenFactory _tokenFactory;
        private readonly IJwtTokenValidator _jwtTokenValidator;

        private readonly ICurrentUser _currentUser;
        private readonly IAppUserRepository _appUserRepo;
        private readonly IAppGroupRepository _groupRepo;
        private readonly AuthSettings _authSettings;

        public AccountController(
            ILogger<AccountController> logger,
            IOptions<AuthSettings> authSettings,
            UnitOfWork unitOfWork,
            IJwtTokenValidator jwtTokenValidator,
            IJwtFactory jwtFactory,
            ITokenFactory tokenFactory,
            ICurrentUser currentUser)
        : base(unitOfWork, logger)
        {
            _logger = logger;
            _authSettings = authSettings.Value;

            _jwtFactory = jwtFactory;
            _tokenFactory = tokenFactory;
            _jwtTokenValidator = jwtTokenValidator;
            _currentUser = currentUser;

            _appUserRepo = new AppUserRepository(unitOfWork);
            _groupRepo = new AppGroupRepository(unitOfWork);
        }

        /// <summary>
        /// Register a new user account (Authenticated: User account create/admin permission is required)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST {baseurl}/{api_endpoint}
        ///     {        
        ///       "firstName": "Emilia",
        ///       "lastName": "Clarke",
        ///       "username": "newaccount2create@gmail.com" ,
        ///       "password":"$trongPa5sw0rd"
        ///     }
        /// </remarks>
        [HttpPost("register")]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult Register([FromBody] RegistrationModel model)
        {
            if (!_currentUser.HasRole(Permission.UserAccountCreator, Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Creating a new user account: {0}", model.Username);
            _logger.LogDebug("Request body: {0}", JsonSerializer.Serialize(model));

            Result<Email> email = Email.Create(model.Username);
            Result<Password> password = Password.Create(model.Password);

            Result result = Result.Combine(email, password);
            if (result.IsFailure)
                return Error(result.Error);

            if (_appUserRepo.HasUsername(model.Username.Trim().ToUpper()))
                return Error($"Username already taken: {model.Username.Trim()}");

            AppUser account = new AppUser(model.FirstName, model.LastName, email.Value, password.Value);
            _appUserRepo.Insert(account);

            return Ok("Registration successful");
        }

        /// <summary>
        /// Authenticate an existing user (Public open endpoint)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST {baseurl}/{api_endpoint}
        ///     {        
        ///       "username": "emailaddress@gmail.com" ,
        ///       "password":"$trongPa5sw0rd"
        ///     }
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(Envelope<AuthenticateResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateModel model)
        {
            _logger.LogInformation("Authenticating user: {0}", model.Username);

            Result<Email> email = Email.Create(model.Username);
            Result<Password> password = Password.Create(model.Password, enforceStrongPassword: false);

            Result result = Result.Combine(email, password);
            if (result.IsFailure)
                return Error(result.Error);

            Maybe<AppUser> account = _appUserRepo.FindByUsername(model.Username);
            if (account.HasNoValue)
                return Error($"No matching user account found: {model.Username.Trim()}");

            if (account.Unwrap(d => d.IsAccountLocked()))
                return Error($"Too many unsuccessful login attempts. Please try again after {account.Value.LockoutEndTime()}");

            Result<Password> verifypassword = Password.Verify(account.Unwrap(acc => acc.PasswordHash), model.Password);
            if (verifypassword.IsFailure)
            {
                account.Value.RecordFailedLoginAttempt();
                _appUserRepo.Update(account.Value);
                _ = Ok(); //save changes

                return Error(verifypassword.Error);
            }

            account.Value.ResetAccountLockout();

            List<AppGroup> groups = _groupRepo.GetList(account.Value.UserGroups.Select(d => d.AppGroupId).ToArray()).ToList();
            string[] permissions = groups.SelectMany(d => d.GroupPermissions.Select(p => p.Permission.Name)).ToArray();

            // authentication successful
            // generate refresh token & save to in-memory
            var jwtToken = await _jwtFactory.GenerateEncodedToken(account.Unwrap(d => d.Id.ToString()), account.Unwrap(d => d.Username), permissions);
            var refreshToken = _tokenFactory.GenerateToken();
            if (!RefereshTokenStore.TryAdd(account.Unwrap(d => d.Id.ToString()), refreshToken))
                RefereshTokenStore[account.Unwrap(d => d.Id.ToString())] = refreshToken;

            // save refresh token
            account.Value.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddHours(4), //  4 hours
                Created = DateTime.UtcNow,
                CreatedByIp = IpAddress()
            });
            _appUserRepo.Update(account.Value);

            return Ok(new AuthenticateResponseDTO(account.Value, jwtToken, refreshToken));
        }

        /// <summary>
        /// Refresh expired or existing access token (Public open endpoint)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST {baseurl}/{api_endpoint}
        ///     {        
        ///       "accessToken": "current_access_token" ,
        ///       "refreshToken":"refresh_token_issued_with_current_access_token"
        ///     }
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("refreshtoken")]
        [ProducesResponseType(typeof(Envelope<ExchangeRefreshTokenResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] ExchangeRefreshTokenModel model)
        {
            _logger.LogInformation("Refreshing access token: {0} with refresh token: {1}", model.AccessToken, model.RefreshToken);

            if (!ModelState.IsValid)
                return Error("Invalid request paramters");

            var cp = _jwtTokenValidator.GetPrincipalFromToken(model.AccessToken, _authSettings.SecretKey);

            // invalid token/signing key was passed and we can't extract user claims
            if (cp != null)
            {
                var id = cp.Claims.First(c => c.Type == "id").Value;
                Maybe<AppUser> account = _appUserRepo.GetById(int.Parse(id));
                if (account.HasNoValue)
                    return Error($"No matching user account found: {id}");

                Maybe<RefreshToken> refreshToken = account.Value.RefreshTokens.FirstOrDefault(x => x.Token == model.RefreshToken);
                // validate referesh token
                if (
                    //RefereshTokenStore.TryGetValue(id, out string oldRefereshToken) && oldRefereshToken == model.RefreshToken
                    //&& 
                    refreshToken.Unwrap(d => d.IsActive))
                {
                    List<AppGroup> groups = _groupRepo.GetList(account.Value.UserGroups.Select(d => d.AppGroupId).ToArray()).ToList();
                    string[] permissions = groups.SelectMany(d => d.GroupPermissions.Select(p => p.Permission.Name)).ToArray();

                    var jwtToken = await account.Unwrap(d => _jwtFactory.GenerateEncodedToken(d.Id.ToString(), d.Username, permissions));
                    string newRefreshToken = _tokenFactory.GenerateToken();
                    RefereshTokenStore[id] = newRefreshToken; // delete the token exchanged and store the new one

                    refreshToken.Value.Revoked = DateTime.UtcNow;
                    refreshToken.Value.RevokedByIp = IpAddress();
                    refreshToken.Value.ReplacedByToken = newRefreshToken;
                    account.Value.RefreshTokens.Add(new RefreshToken
                    {
                        Token = newRefreshToken,
                        Expires = DateTime.UtcNow.AddHours(4), //  4 hours
                        Created = DateTime.UtcNow,
                        CreatedByIp = IpAddress()
                    });
                    _appUserRepo.Update(account.Value);

                    return Ok(new ExchangeRefreshTokenResponseDTO(jwtToken, newRefreshToken));
                }
            }

            return Error("Provided token is invalid or expired.");
        }

        private string IpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

        /// <summary>
        /// Assign user groups for an existing account (Authenticated: User account edit/admin permission is required)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT {baseurl}/{api_endpoint}
        ///     {        
        ///       "id": "1",
        ///       "selectedUserGroups": ["Admin"],
        ///     }
        /// </remarks>
        [HttpPut("usergroups")]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult AssignUserGroup([FromBody] AssignUserGroupModel model)
        {
            if (!_currentUser.HasRole(Permission.UserAccountEditor, Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Assigning user groups for user account: {0}", model.Id);
            _logger.LogDebug("Request body: {0}", JsonSerializer.Serialize(model));

            Maybe<AppUser> account = _appUserRepo.GetById(model.Id);
            if (account.HasNoValue)
                return Error($"No matching user account found: {model.Id}");

            model.SelectedUserGroups ??= new string[] { };

            Result<List<AppGroup>> selectedUserGroups = GetMatchingUserGroups(_groupRepo, model.SelectedUserGroups);
            if (selectedUserGroups.IsFailure)
                return Error(selectedUserGroups.Error);

            account.Value.UpdateUserGroups(selectedUserGroups.Value);
            _appUserRepo.Update(account.Value);

            return Ok("User groups assigned");
        }

        private Result<List<AppGroup>> GetMatchingUserGroups(
            IAppGroupRepository groupRepo,
            string[] groupNames
        )
        {
            if (groupRepo == null)
                return Result.Fail<List<AppGroup>>("AppGroup repo is not specified");

            if (groupNames == null)
                return Result.Fail<List<AppGroup>>("No user groups are specified");

            groupNames = groupNames.Select(d => d.Trim().ToUpper()).Distinct().ToArray();

            List<AppGroup> appGroups = groupRepo.GetList().ToList();
            List<AppGroup> result = new List<AppGroup>();

            foreach (var name in groupNames)
            {
                Maybe<AppGroup> appgroup = appGroups.FirstOrDefault(d => d.Name.ToUpper() == name);
                if (appgroup.HasNoValue)
                    return Result.Fail<List<AppGroup>>($"No matching user group found: {name}");

                result.Add(appgroup.Value);
            }

            return Result.Ok(result);
        }

        /// <summary>
        /// Reset password of an existing account (Authenticated: User account admin permission is required)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST {baseurl}/{api_endpoint}
        ///     {        
        ///       "username": "account2reset@gmail.com",
        ///       "password": "new$trongPa5sw0rd",
        ///     }
        /// </remarks>
        [HttpPost("password/reset")]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (!_currentUser.HasRole(Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Reseting user account password: {0}", model.Username);
            _logger.LogDebug("Request body: {0}", JsonSerializer.Serialize(model));

            Result<Password> password = Password.Create(model.Password);
            if (password.IsFailure)
                return Error(password.Error);

            Maybe<AppUser> account = _appUserRepo.FindByUsername(model.Username);
            if (account.HasNoValue)
                return Error<AuthenticateResponseDTO>($"No matching user account found: {model.Username.Trim()}");

            account.Value.ChangePassowrd(password.Value);
            _appUserRepo.Update(account.Value);

            return Ok("Account password resetted");
        }

        /// <summary>
        /// Block access for an existing account (Authenticated: User account admin permission is required)
        /// </summary>
        /// <param name="username">username</param>
        [HttpPost("block")]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult BlockAccount([FromQuery] string username)
        {
            if (!_currentUser.HasRole(Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Blocking user account: {0}", username);

            Maybe<AppUser> account = _appUserRepo.FindByUsername(username);
            if (account.HasNoValue)
                return Error($"No matching user account found: {username.Trim()}");

            if (account.Value.IsRelatedToUser(_currentUser.Id))
                return Error("Unable to block your own account");

            account.Value.BlockAccount();
            _appUserRepo.Update(account.Value);

            return Ok("Account blocked");
        }

        /// <summary>
        /// Unblock access of a blocked account (Authenticated: User account admin permission is required)
        /// </summary>
        /// <param name="username">username</param>
        [HttpPost("unblock")]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult UnblockAccount([FromQuery] string username)
        {
            if (!_currentUser.HasRole(Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Unblocking user account: {0}", username);

            Maybe<AppUser> account = _appUserRepo.FindByUsername(username);
            if (account.HasNoValue)
                return Error<AuthenticateResponseDTO>($"No matching user account found: {username.Trim()}");

            account.Value.UnblockAccount();
            _appUserRepo.Update(account.Value);

            return Ok("Account unblocked");
        }


        /// <summary>
        /// List and search all existing accounts with paging behaviour (Authenticated: User account view/admin permission is required)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST {baseurl}/{api_endpoint}
        ///     {        
        ///       "search": {
        ///         "value":""
        ///       },
        ///       "start": 0, // starting row index. next iteration = start + length
        ///       "length":10. // no. of records per page (take count)
        ///     }
        /// </remarks>
        [HttpPost("list")]
        [ProducesResponseType(typeof(Envelope<IDataTableAjaxPostResponse<UserAccountSummaryDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult GetAccounts([FromBody] DataTableAjaxPostModel model)
        {
            if (!_currentUser.HasRole(Permission.UserAccountViewer, Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Retrieving user accounts for search");
            _logger.LogDebug("Request body: {0}", JsonSerializer.Serialize(model));

            Specification<AppUser> spec = string.IsNullOrWhiteSpace(model.Search.Value) ? Specification<AppUser>.All : Specification<AppUser>.None;
            string[] searchTerms = model.Search.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            foreach (string term in searchTerms)
            {
                if (Email.Create(term).IsSuccess) // search term is an email address
                    spec = spec.Or(new UserAccountWithUsernameSpec(term));
                else
                    spec = spec.Or(new UserAccountWithNameLikeSpec(term));
            }

            AppUserSortableColumn sortBy = AppUserSortableColumn.Id;
            bool sortDir = true;

            List<AppUser> appUsers = _appUserRepo.GetList(spec,
                take: model.Length,
                skip: model.Start,
                filteredResultsCount: out int filteredResultsCount,
                totalResultsCount: out int totalResultsCount,
                sortBy: sortBy,
                sortDir: sortDir).ToList();

            var result = appUsers.Select(d => new UserAccountSummaryDTO
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Username = d.Username,
                AccessFailedCount = d.AccessFailedCount,
                LockoutEnabled = d.LockoutEnabled,
                LockoutEndDateTimeUtc = d.LockoutEndDateTimeUtc?.ToString("yyyy-MM-dd hh:mm tt"),
                DateTimeRegistered = d.CreatedDateTimeUtc?.ToLocalTime().ToString("yyyy-MM-dd hh:mm tt")
            });

            return Ok(new DataTableAjaxPostResponse<UserAccountSummaryDTO>
            {
                Draw = model.Draw,
                RecordsTotal = totalResultsCount,
                RecordsFiltered = filteredResultsCount,
                Data = result
            }, contextReadonly: true);
        }

        /// <summary>
        /// View an existing user account details (Authenticated: User account view/admin permission is required)
        /// </summary>
        /// <param name="id">user account id</param>
        [HttpGet("")]
        [ProducesResponseType(typeof(Envelope<UserAccountDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult Get(int id)
        {
            if (!_currentUser.HasRole(Permission.UserAccountViewer, Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Retrieving user account details: {0}", id);

            Maybe<AppUser> account = _appUserRepo.GetById(id);
            if (account.HasNoValue)
                return Error<AuthenticateResponseDTO>($"No matching user account found: {id}");

            List<AppGroup> groups = _groupRepo
                .GetList(account.Value.UserGroups.Select(d => d.AppGroupId)).ToList();

            return Ok(account.Unwrap(d => new UserAccountDTO
            {
                Id = d.Id,
                Username = d.Username,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Fullname = d.Fullname,
                AccessFailedCount = d.AccessFailedCount,
                LockoutEnabled = d.LockoutEnabled,
                LockoutEndDateTimeUtc = d.LockoutEndDateTimeUtc?.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss tt"),
                AccountLocked = d.IsAccountLocked(),
                DateTimeRegistered = d.CreatedDateTimeUtc?.ToLocalTime().ToString("yyyy-MM-dd hh:mm tt"),
                DateTimeLastModified = d.ModifiedDateTimeUtc?.ToLocalTime().ToString("yyyy-MM-dd hh:mm tt"),
                LockoutEndTime = d.LockoutEndTime(),
                AssignedUserGroups = groups.Select(d => new UserGroupDTO
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description
                })
            }), contextReadonly: true);
        }

        /// <summary>
        /// List not assigned user groups for an existing user account (Authenticated: User account view/admin permission is required)
        /// </summary>
        /// <param name="id">user account id</param>
        [HttpGet("notassigned/usergroups")]
        [ProducesResponseType(typeof(Envelope<IEnumerable<UserGroupDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Envelope<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IActionResult GetNotAssignedUserGroups(int id)
        {
            if (!_currentUser.HasRole(Permission.UserAccountViewer, Permission.UserAccountAdmin))
                return Forbidden();

            _logger.LogInformation("Retrieving not assigned user groups for user account: {0}", id);

            Maybe<AppUser> account = _appUserRepo.GetById(id);
            if (account.HasNoValue)
                return Error($"No matching user account found: {id}");

            List<AppGroup> groups = _groupRepo.GetList().ToList();
            List<int> assignedGroups = account.Value.UserGroups.Select(d => d.AppGroupId).ToList();

            var result = groups
               .Where(p => !assignedGroups.Any(d => d == p.Id)) // exclude already assigned permissions to specified group
               .Select(d => new UserGroupDTO
               {
                   Id = d.Id,
                   Name = d.Name,
                   Description = d.Description,
                   PermissionsCount = d.GroupPermissions.Count()
               });

            return Ok(result, contextReadonly: true);
        }
    }
}
