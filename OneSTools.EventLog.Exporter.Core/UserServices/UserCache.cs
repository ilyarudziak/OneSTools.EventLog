using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OneSTools.EventLog.Exporter.Core.UserServices
{
    public class UserCache : IUserCache
    {
        private readonly ILogger<UserCache> _logger;
        private readonly Dictionary<string, DateTime> _fileChangesCache = new Dictionary<string, DateTime>();
        private readonly Dictionary<string, string> _usersCache = new Dictionary<string, string>();
        private readonly int _intervalCheckInMinutes;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly string[] _filePathes = Array.Empty<string>();
        private DateTime _lastCheckTime = DateTime.MinValue;

        public UserCache(ILogger<UserCache> logger, IDateTimeProvider dateTimeProvider, IOptions<UserOptions> options)
        {
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _filePathes = options.Value.DataResources;
            _intervalCheckInMinutes = options.Value.IntervalCheckInMin;
        }

        public Dictionary<string, string> Users => Processing();
        
        private Dictionary<string, string> Processing()
        {
            if ((_dateTimeProvider.UtcNow() - _lastCheckTime).TotalMinutes > _intervalCheckInMinutes)
            {
                if (_fileChangesCache.Count == 0 || IsFilesWasChanged())
                {
                    UpdateFileChangesCache();
                    UpdateUserCache();
                }

                _lastCheckTime = _dateTimeProvider.UtcNow();
            }
            return _usersCache;
        }
        
        private bool IsFilesWasChanged()
        {
            _logger.LogInformation("Checking file changes");
            foreach (var file in _fileChangesCache)
            {
                var lastWriteTime = File.GetLastWriteTimeUtc(file.Key);
                if (lastWriteTime != file.Value)
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateUserCache()
        {
            _usersCache.Clear();
            foreach (var file in _fileChangesCache)
            {
                LoadFileContent(file.Key);
            }
        }

        private void UpdateFileChangesCache()
        {
            if (_filePathes.Length == 0)
            {
                _logger.LogWarning("No files were provided for monitoring, please check the configuration file paths");
                return;
            }
            
            _fileChangesCache.Clear();
            foreach (var filePath in _filePathes)
            {
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    _logger.LogError("File {filePath} does not exist", filePath);
                }
                else
                {
                    _fileChangesCache.Add(filePath,  File.GetLastWriteTimeUtc(filePath));
                }
            }
        }
        
        private void LoadFileContent(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1)
            {
                _logger.LogError("File {filePath} does contain any strings", filePath);
                return;
            }
            
            for (int i = 1; i < lines.Length; i++)
            {
                var splitedLine = lines[i].Split(';');
                if (splitedLine.Length >= 2)
                {
                    var key = splitedLine[0].Trim();
                    var value = splitedLine[1].Trim().Replace("\"","");
                    try
                    {
                        if (!_usersCache.TryAdd(key, value))
                        {
                            _logger.LogInformation("User {uid} was already added", key);
                            _usersCache[key] = value;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while add user {uid} {email}", key, value);
                    }
                }
            }
            
            _logger.LogInformation("Loaded file {path}", filePath);
        }
    }
}