using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace CommonUtil.Helpers
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly HttpContext _httpContext;

        public CurrentUser(IHttpContextAccessor httpContextAccessor) => _httpContext = httpContextAccessor.HttpContext;

        public CurrentUser(HttpContext httpContext) => _httpContext = httpContext;

        /// <summary>
        /// Take logged in user Id from HttpContext. default value is -1
        /// </summary>
        public int Id => IsAuthenticated ? int.Parse(_httpContext.User.Identity.Name) : -1;

        /// <summary>
        /// Take NameIdentifier claim value as logged in user name. default is "anonymous"
        /// </summary>
        public string Name => IsAuthenticated
            ? (_httpContext.User.Identity as ClaimsIdentity).Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value
            : "anonymous";

        /// <summary>
        /// Convert int roles values to their corresponding enum values and filter out their corresponding description attribute values
        /// </summary>
        public IEnumerable<string> Roles
        {
            get
            {
                if (!IsAuthenticated)
                    return Enumerable.Empty<string>();

                IEnumerable<string> roles = (_httpContext.User.Identity as ClaimsIdentity).Claims.Where(c => c.Type == ClaimTypes.Role).Select(d => d.Value);
                return roles.Select(role => EnumInfo.GetDescriptionByInt<Permission>(role)); 
            }
        }

        public bool IsAuthenticated => _httpContext.User.Identity.IsAuthenticated;

        public bool HasRole(params Permission[] roles) => roles.Any(role => _httpContext.User.IsInRole(EnumInfo.GetValue(role).ToString()));
    }

}
