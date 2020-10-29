using System;
using System.Collections.Generic;

namespace AutoRenamerWPF
{
    public class PluginsManager
    {
        private static readonly Dictionary<string, Type> Plugins = new Dictionary<string, Type>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageID">The Package ID of the resource's content.</param>
        /// <param name="ruleID">The Rule ID of the resource's content.</param>
        /// <param name="ruleType">The rule's type.</param>
        /// <returns>If it register successfully , it'll return true . Otherwise , It returns false due to there is equal key.</returns>
        public static bool RegisterPlugin(string packageID, string ruleID, Type ruleType)
        {
            var fullName = $"{packageID}.{ruleID}";
            if (!Plugins.ContainsKey(fullName))
            {
                Plugins.Add(fullName, ruleType);
                return true;
            }
            return false;
        }
    }
}
