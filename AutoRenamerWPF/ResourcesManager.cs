using System;
using System.Collections.Generic;

namespace AutoRenamer
{
    public class ResourcesManager : IResourcesManager
    {
        private static readonly Dictionary<string, string> Resources = new Dictionary<string, string>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageID">The Package ID of the resource's content.</param>
        /// <param name="ruleID">The Rule ID of the resource's content.</param>
        /// <param name="key">The key.</param>
        /// <param name="content">The value.It'll be returned via Resources Manager.</param>
        /// <returns>If it register successfully , it'll return true . Otherwise , It returns false due to there is equal key.</returns>
        public static bool RegisterResource(string packageID, string ruleID, string key, string content)
        {
            var fullKey = $"{packageID}.{ruleID}.{key}";
            if (!Resources.ContainsKey(fullKey))
            {
                Resources.Add(fullKey, content);
                return true;
            }
            return false;
        }

        public string GetResourceBy(string fullKey)
        {
            return Resources[fullKey];
        }

        public bool TryGetResourceBy(string fullKey, out string content)
        {
            return Resources.TryGetValue(fullKey, out content);
        }
    }

    public interface IResourcesManager
    {
        /// <summary>
        /// Get the localized resouce's content via the key.
        /// </summary>
        /// <param name="fullKey">Format by "{packageID}.{ruleID}.{key}" .</param>
        /// <returns>The localized content.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetResourceBy(string fullKey);

        /// <summary>
        /// Try getting the localized resouce's content via the key.
        /// </summary>
        /// <param name="fullKey">Format by "{packageID}.{ruleID}.{key}" .</param>
        /// <param name="content"></param>
        /// <returns>Whether this operation is successful.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryGetResourceBy(string fullKey, out string content);
    }
}
