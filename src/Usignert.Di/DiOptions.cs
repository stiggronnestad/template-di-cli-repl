using System.Reflection;

namespace Usignert.Di
{
    public class DiOptions
    {
        public enum AppDataLocation
        {
            None = 0,
            AssemblyPath,
            RoamingPath,
            Specified
        }

        public string AssemblyDirectory => GetAppDataLocation(AppDataLocation.AssemblyPath);

        public string RoamingDirectory => GetAppDataLocation(AppDataLocation.RoamingPath);

        public string ConfigPath
        {
            get
            {
                if (string.IsNullOrEmpty(_configPath))
                {
                    var directory = GetAppDataLocation(AppDataLocation.AssemblyPath);
                    return Path.Combine(directory, "config.json");
                }

                return _configPath;
            }
        }

        public string LogPath
        {
            get
            {
                if (string.IsNullOrEmpty(_logPath))
                {
                    var directory = GetAppDataLocation(AppDataLocation.AssemblyPath);
                    return Path.Combine(directory, "log.txt");
                }

                return _logPath;
            }
        }

        public AppDataLocation ConfigLocation { get; private set; } = AppDataLocation.None;

        public AppDataLocation LogLocation { get; private set; } = AppDataLocation.None;

        private string _configPath = string.Empty;

        private string _logPath = string.Empty;

        private DiOptions()
        {
            ConfigLocation = AppDataLocation.AssemblyPath;
            LogLocation = AppDataLocation.AssemblyPath;
        }

        private DiOptions(AppDataLocation appDataLocation, string externalConfigPath = "", string externalLogPath = "")
        {
            if (string.IsNullOrEmpty(externalConfigPath))
            {
                WithConfigLocation(appDataLocation);
            }
            else
            {
                WithConfigLocation(externalConfigPath);
            }

            if (string.IsNullOrEmpty(externalLogPath))
            {
                WithLogLocation(appDataLocation);
            }
            else
            {
                WithLogLocation(externalLogPath);
            }
        }

        public static DiOptions Empty => new();

        public static DiOptions Build(AppDataLocation appDataPath = AppDataLocation.AssemblyPath, string externalConfigPath = "", string externalLogPath = "")
        {
            return new DiOptions(appDataPath, externalConfigPath, externalLogPath);
        }

        public static string GetAppDataLocation(AppDataLocation appDataPath)
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? "template-di-cli-repl";
            var roamingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), assemblyName);

            return appDataPath switch
            {
                AppDataLocation.AssemblyPath => Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? throw new DirectoryNotFoundException("Directory not found."),
                AppDataLocation.RoamingPath => roamingPath,
                _ => throw new ArgumentOutOfRangeException(nameof(appDataPath), appDataPath, null)
            };
        }

        public DiOptions WithAppDataLocation(AppDataLocation appDataPath)
        {
            WithConfigLocation(appDataPath);
            WithLogLocation(appDataPath);
            return this;
        }

        public DiOptions WithConfigLocation(string externalConfigPath)
        {
            _configPath = externalConfigPath;
            ConfigLocation = AppDataLocation.Specified;
            return this;
        }

        public DiOptions WithConfigLocation(AppDataLocation appDataPath)
        {
            _configPath = appDataPath switch
            {
                AppDataLocation.AssemblyPath => Path.Combine(GetAppDataLocation(AppDataLocation.AssemblyPath), "config.json"),
                AppDataLocation.RoamingPath => Path.Combine(GetAppDataLocation(AppDataLocation.RoamingPath), "config.json"),
                _ => throw new ArgumentOutOfRangeException(nameof(appDataPath), appDataPath, null)
            };

            ConfigLocation = appDataPath;

            return this;
        }

        public DiOptions WithLogLocation(string externalLogPath)
        {
            _logPath = externalLogPath;
            LogLocation = AppDataLocation.Specified;
            return this;
        }

        public DiOptions WithLogLocation(AppDataLocation appDataPath)
        {
            _logPath = appDataPath switch
            {
                AppDataLocation.AssemblyPath => Path.Combine(GetAppDataLocation(AppDataLocation.AssemblyPath), "log.txt"),
                AppDataLocation.RoamingPath => Path.Combine(GetAppDataLocation(AppDataLocation.RoamingPath), "log.txt"),
                _ => throw new ArgumentOutOfRangeException(nameof(appDataPath), appDataPath, null)
            };

            LogLocation = appDataPath;

            return this;
        }
    }
}
