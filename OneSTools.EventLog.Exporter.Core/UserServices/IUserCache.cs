using System.Collections.Generic;

namespace OneSTools.EventLog.Exporter.Core.UserServices
{
    public interface IUserCache
    {
        Dictionary<string, string> Users { get; }
    }
}