using System;

namespace OneSTools.EventLog.Exporter.Core
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow();
    }
}