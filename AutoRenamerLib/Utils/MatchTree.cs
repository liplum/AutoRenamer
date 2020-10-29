using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AutoRenamerLib.Utils
{
    public class MatchTree
    {
        public const string DEFAULT = "default";

        private readonly List<Node> firstLayer = new List<Node>();

        public MatchTree() { }

        public bool Add(CultureInfo culture)
        {
            try
            {
                if (firstLayer.Count == 0 || culture.IsSuperCulture())
                {
                    var newNode = new Node(culture);
                    firstLayer.Add(newNode);
                    return true;
                }
                bool hasAdded = false;
                foreach (var node in firstLayer)
                {
                    hasAdded |= node.Add(culture);
                    if (hasAdded)
                    {
                        return true;
                    }
                }
                if (!hasAdded)
                {
                    var newNode = new Node(culture);
                    firstLayer.Add(newNode);
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }

        public string GetBestMatch(CultureInfo culture)
        {
            //All nodes in the first layer
            //such as : en-UK ,en-US ,zh ,fr
            var groups_q = from node in firstLayer
                           let cur = node.Content
                           group node by cur.IsSuperCulture() ? cur.Name : cur.Parent.Name;

            var groups = groups_q.ToArray();

            foreach (var group in groups)
            {
                //Current group's all nodes.
                //such as : en-UK ,en-US

                var allNodes = group.ToArray();

                if (HasRelevance())
                {
                    //First , Find the same culture
                    string result = null;
                    foreach (var node in allNodes)
                    {
                        var res = node.MatchTheSame(culture);
                        if (!(res is null))
                        {
                            result = res;
                            break;
                        }
                    }

                    if (result is null)//It means the first step can't find the same culture.
                    {
                        //Second , Find the parent culture
                        foreach (var node in firstLayer)
                        {
                            var res = node.MatchTheParent(culture);
                            if (!(res is null))
                            {
                                result = res;
                                break;
                            }
                        }

                        if (result is null)//It means the second step can't find the parent culture.
                        {
                            //Third , Find the first cousin culture
                            foreach (var node in firstLayer)
                            {
                                var res = node.MatchTheCousin(culture);
                                if (!(res is null))
                                {
                                    result = res;
                                    break;
                                }
                            }
                        }
                    }

                    return result ?? DEFAULT;
                }
                else
                {
                    continue;
                }

                bool HasRelevance()
                {
                    foreach (var node in allNodes)
                    {
                        var content = node.Content;
                        if (content.IsEqual(culture) || content.IsParent(culture) || content.HasTheSameImmediateParent(culture))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            return DEFAULT;
        }

        private class Node
        {
            public CultureInfo Content { get; set; }

            private readonly List<Node> subTree = new List<Node>();

            private bool IsLeafNode
            {
                get
                {
                    return subTree.Count == 0;
                }
            }

            public Node(CultureInfo content)
            {
                Content = content;
            }

            /// <summary>
            /// Add a new culture.If this culture is not sub of the culture this node holds ,it returns false and doesn't add it into the sub-tree.
            /// </summary>
            /// <param name="culture"></param>
            /// <returns>If this culture is sub of the culture this node holds,it returns ture.Otherwise,it returns false.</returns>
            /// <exception cref="CultureNotFoundException"></exception>
            public bool Add(CultureInfo culture)
            {
                if (culture.IsImmediateSub(Content))
                {
                    var newNode = new Node(culture);
                    subTree.Add(newNode);
                    return true;
                }
                if (culture.IsSub(Content))
                {
                    //The culture is the sub of the Content , but not immediate sub of the Content -- it's the indirect sub of the Content.
                    bool hasAdded = false;
                    foreach (var node in subTree)
                    {
                        hasAdded |= node.Add(culture);
                        if (hasAdded)
                        {
                            return true;
                        }
                    }
                    if (!hasAdded)
                    {
                        var newNode = new Node(culture);
                        subTree.Add(newNode);
                        return true;
                    }
                }
                return false;
            }

            public string MatchTheSame(CultureInfo culture)
            {
                if (IsLeafNode)
                {
                    if (Content.IsEqual(culture))
                    {
                        return Content.Name;
                    }
                    return null;
                }
                else
                {
                    string result = null;
                    foreach (var subNode in subTree)
                    {
                        var res = subNode.MatchTheSame(culture);
                        if (!(res is null))
                        {
                            result = res;
                            break;
                        }
                    }
                    return result;
                }
            }

            public string MatchTheParent(CultureInfo culture)
            {
                if (IsLeafNode)
                {
                    if (Content.IsImmediateParent(culture))
                    {
                        return Content.Name;
                    }
                    return null;
                }
                else
                {
                    string result = null;
                    foreach (var subNode in subTree)
                    {
                        var res = subNode.MatchTheParent(culture);
                        if (!(res is null))
                        {
                            result = res;
                            break;
                        }
                    }
                    return result;
                }
            }

            public string MatchTheCousin(CultureInfo culture)
            {
                if (IsLeafNode)
                {
                    if (Content.HasTheSameImmediateParent(culture))
                    {
                        return Content.Name;
                    }
                    return null;
                }
                else
                {
                    string result = null;
                    foreach (var subNode in subTree)
                    {
                        var res = subNode.MatchTheCousin(culture);
                        if (!(res is null))
                        {
                            result = res;
                            break;
                        }
                    }
                    return result;
                }
            }
        }
    }
    public static class CultureTool
    {

        public static bool HasTheSameImmediateParent(this CultureInfo a, CultureInfo b)
        {
            var a_parent = a.Parent;
            var b_parent = b.Parent;
            if (a_parent.IsExisted() && b_parent.IsExisted())
            {
                return a_parent.IsEqual(b_parent);
            }
            return false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="culture"></param>
        /// <returns>Whether the culture has a parent.</returns>
        public static bool IsSuperCulture(this CultureInfo culture)
        {
            var parent = culture.Parent;
            return !parent.IsExisted();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="culture">Itself can be a super culture.</param>
        /// <returns></returns>
        public static CultureInfo GetSuperCulture(this CultureInfo culture)
        {
            var super = culture;
            while (!super.IsSuperCulture())
            {
                super = super.Parent;
            }
            return super;
        }

        public static bool IsParent(this CultureInfo parent, CultureInfo sub)
        {
            if (parent.IsEqual(sub))
            {
                return false;
            }

            for (var cur = sub.Parent; cur.IsExisted(); cur = cur.Parent)
            {
                if (parent.IsEqual(cur))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsImmediateParent(this CultureInfo parent, CultureInfo sub)
        {
            if (parent.IsEqual(sub))
            {
                return false;
            }

            var parentOfSub = sub.Parent;

            if (!parentOfSub.IsExisted())
            {
                return false;
            }

            return parent.IsEqual(parentOfSub);
        }
        public static bool IsImmediateSub(this CultureInfo sub, CultureInfo parent)
        {
            return parent.IsImmediateParent(sub);
        }

        public static bool IsSub(this CultureInfo sub, CultureInfo parent)
        {
            return parent.IsParent(sub);
        }

        public static bool IsEqual(this CultureInfo a, CultureInfo b)
        {
            return a.Name.Equals(b.Name);
        }

        public static bool IsExisted(this CultureInfo culture)
        {
            return !string.IsNullOrEmpty(culture.Name);
        }
    }
}
