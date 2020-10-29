using AutoRenamerLib.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AutoRenamer
{
    public static class Initializer
    {
        const string FOLDER_NAME = "Plugins";
        const string CONFIG_FILE_NAME = "config.json";
        private static DirectoryInfo PluginFolder;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <exception cref="PluginFolderNotExistException"></exception>
        public static void Init()
        {
            CurrentCulture = Thread.CurrentThread.CurrentUICulture;
            var curFolder = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            PluginFolder = new DirectoryInfo($@"{curFolder}\{FOLDER_NAME}");

            try
            {
                if (!PluginFolder.Exists)
                {
                    CreatePluginFolder();
                    throw new HasNoPluginException(PluginFolder.FullName);
                }
                ReadPlugins();

            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="PluginFolderNotExistException"></exception>
        private static void ReadPlugins()
        {
            if (PluginFolder is null || !PluginFolder.Exists)
            {
                throw new PluginFolderNotExistException();
            }
            var allPluginFolders = PluginFolder.GetDirectories();
            foreach (var folder in allPluginFolders)
            {
                if (folder.HasConfig(out var config))
                {
                    try
                    {
                        if (!config.Exists)
                        {
                            continue;
                        }
                        ReadConfig(config);
                    }
                    catch
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        private static FileInfo GetPluginFolderBy([NotNull] string relativePath)
        {
            var pluginFile = new FileInfo($@"{PluginFolder.FullName}\{relativePath}");
            if (pluginFile.Exists)
            {
                return pluginFile;
            }
            return null;
        }


        private static CultureInfo CurrentCulture;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns>A path of the Rule(s),which was registered . The path is relative to the Plugin Folder . </returns>
        private static void ReadConfig([NotNull] FileInfo configFile)
        {
            try
            {
                var json = File.ReadAllText(configFile.FullName);
                var config = JObject.Parse(json);

                var meta = config["Meta"];

                foreach (var metaItem in meta)
                {
                    try
                    {
                        var path = metaItem["Path"];
                        var pluginFile = GetPluginFolderBy(path.ToString());

                        var packageID = metaItem["PackageID"].ToString();
                        var ruleID = metaItem["RuleID"].ToString();

                        var ruleTypeFullName = $"{packageID}.{ruleID}";
                        var asm = Assembly.LoadFile(pluginFile.FullName);
                        var ruleType = asm.GetType(ruleTypeFullName);
                        ResgisterPlugin(packageID, ruleID, ruleType);

                        var defaultCultureName = metaItem["DefaultCulture"].ToString();
                        var resources = metaItem["Resources"];
                        var resourcesJObject = JObject.Parse(resources.ToString());

                        var tree = new MatchTree();
                        foreach (var res in resourcesJObject.Properties())
                        {
                            try
                            {
                                var culture = new CultureInfo(res.Name);
                                tree.Add(culture);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        var matchResult = tree.GetBestMatch(CurrentCulture);
                        if (matchResult.Equals(MatchTree.DEFAULT))
                        {
                            var defaultCulture = new CultureInfo(defaultCultureName);
                            var matchResult_2 = tree.GetBestMatch(defaultCulture);
                            if (matchResult_2.Equals(MatchTree.DEFAULT))
                            {
                                continue;
                            }

                        }
                        var language = resources[matchResult];
                        var languageJObject = JObject.Parse(language.ToString());
                        foreach (var content in languageJObject.Properties())
                        {
                            RegisterResources(packageID, ruleID, content.Name, content.Value.ToString());
                        }
                    }
                    catch (NullReferenceException)
                    {
                        continue;
                    }
                    catch (CultureNotFoundException)
                    {
                        continue;
                    }
                    catch (FileLoadException)
                    {
                        continue;
                    }
                }
            }

            catch (NullReferenceException)
            {
                //It lacked one or more properties which the App need
                throw new ConfigNotCompleteException();
            }
            catch (JsonReaderException)
            {
                //It cannot recognize the json
                throw;
            }

        }


        public static bool RegisterResources(string packageID, string ruleID, string key, string content)
        {
            return ResourcesManager.RegisterResource(packageID, ruleID, key, content);
        }

        private static bool ResgisterPlugin(string packageID, string ruleID, Type ruleType)
        {
            return PluginsManager.RegisterPlugin(packageID, ruleID, ruleType);
        }


        private static bool HasConfig(this DirectoryInfo directory, out FileInfo configFile)
        {
            var allFiles = directory.GetFiles();
            var res = (from file in allFiles where file.FullName.EndsWith(CONFIG_FILE_NAME) select file).ToArray();
            if (res.Length > 0)
            {
                configFile = res[0];
                return true;
            }
            else
            {
                configFile = null;
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="CannotCreatePluginFolderException"></exception>
        private static void CreatePluginFolder()
        {
            try
            {
                Directory.CreateDirectory(PluginFolder.FullName);
            }
            catch (Exception e)
            {
                throw new CannotCreatePluginFolderException(PluginFolder.FullName, e);
            }
        }

        public class PluginFolderNotExistException : Exception
        {
            public PluginFolderNotExistException() { }
            public PluginFolderNotExistException(string message) : base(message) { }
        }
        /// <summary>
        /// It lacked one or more properties which the App need
        /// </summary>
        public class ConfigNotCompleteException : Exception
        {
            public ConfigNotCompleteException() { }
            public ConfigNotCompleteException(string message) : base(message) { }
        }

        public class HasNoPluginException : Exception
        {
            public HasNoPluginException() { }
            public HasNoPluginException(string message) : base(message) { }
        }

        public class CannotCreatePluginFolderException : Exception
        {
            public CannotCreatePluginFolderException() { }
            public CannotCreatePluginFolderException(string message) : base(message) { }
            public CannotCreatePluginFolderException(string message, Exception inner) : base(message, inner) { }
        }
    }
}
