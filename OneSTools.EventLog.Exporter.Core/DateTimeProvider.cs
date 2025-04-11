using System;

namespace OneSTools.EventLog.Exporter.Core
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow() => DateTime.UtcNow;
    }
}