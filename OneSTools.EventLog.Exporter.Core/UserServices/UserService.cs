using Microsoft.Extensions.Logging;

namespace OneSTools.EventLog.Exporter.Core.UserServices
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUserCache _userCache;
        
        public UserService(IUserCache userCache, ILogger<UserService> logger)
        {
            _logger = logger;
            _userCache = userCache;
        }
        

        public string GetUserNameByUid(string uid)
        {
            if (_userCache.Users.TryGetValue(uid, out string userEmail))
            {
                return userEmail;
            }
            _logger.LogWarning("Can't find user {uid}", uid);
            return string.Empty;
        }
    }
}