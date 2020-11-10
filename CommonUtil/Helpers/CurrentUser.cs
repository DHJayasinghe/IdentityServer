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

        public int Id => int.Parse(_httpContext.User.Identity.Name);

        public string Name => (_httpContext.User.Identity as ClaimsIdentity).Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

        public IEnumerable<string> Roles
        {
            get
            {
                IEnumerable<string> roles = (_httpContext.User.Identity as ClaimsIdentity).Claims.Where(c => c.Type == ClaimTypes.Role).Select(d => d.Value);
                return roles.Select(role => EnumInfo.GetDescriptionByInt<Permission>(role)); // convert int roles into it'd description
            }
        }

        public bool IsAuthenticated => _httpContext.User.Identity.IsAuthenticated;

        public bool HasRole(params Permission[] roles) => roles.Any(role => _httpContext.User.IsInRole(EnumInfo.GetValue(role).ToString()));
    }

    public interface ICurrentUser
    {
        int Id { get; }

        string Name { get; }

        /// Determine whether current user is authenticated to access resources
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Determine whether current user is in mentioned role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        bool HasRole(params Permission[] roles);
    }
}
