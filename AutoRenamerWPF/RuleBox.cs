using RuleAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AutoRenamerWPF
{
    public class RuleBox
    {
        private static Dictionary<string, Type> AllPlugins = new Dictionary<string, Type>();

        public static bool RegisterPlugin(string packageID, string ruleID, Type ruleType)
        {
            var fullName = $"{packageID}.{ruleID}";
            if (!AllPlugins.ContainsKey(fullName))
            {
                AllPlugins.Add(fullName, ruleType);
                return true;
            }
            return false;
        }

        private ObservableCollection<IRule> AllRules;

        public RuleBox()
        {
            foreach (var pair in AllPlugins)
            {
                var type = pair.Value;
                var rule = (IRule)Activator.CreateInstance(type);
                rule.SetResourcesManager(null);
                AllRules.Add(rule);
            }
        }
    }
} 