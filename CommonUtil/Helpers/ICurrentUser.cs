namespace CommonUtil.Helpers
{
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
