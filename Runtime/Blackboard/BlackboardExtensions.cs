using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    internal static class BlackboardExtensions
    {
        private static readonly string[] s_emptyNames = new string[0];

        internal static string[] GetNames(this Blackboard blackboard)
        {
            if (blackboard != null && blackboard.Values != null && blackboard.Values.Count > 0)
            {
                return blackboard.Values.Where(v => !string.IsNullOrEmpty(v.Key)).Select(v => v.Key).ToArray();
            }

            return s_emptyNames;
        }

        internal static string[] GetNamesTyped(this Blackboard blackboard, params Type[] compatableTypes)
        {
            if (blackboard != null && compatableTypes != null && compatableTypes.Length > 0)
            {
                var result = new List<string>();
                var names = blackboard.GetNames();

                foreach (var n in names)
                {
                    if (TypeArrayContainsTypeOrBaseType(compatableTypes, blackboard.Values[n].GetType()))
                    {
                        result.Add(n);
                    }
                }

                return result.ToArray();
            }

            return s_emptyNames;
        }

        internal static bool TryGetBlackboard(this UnityEngine.Object unityObject, out Blackboard blackboard)
        {
            if (unityObject != null)
            {
                if (unityObject is IBlackboardProvider bbProvider)
                {
                    blackboard = bbProvider.Blackboard;
                    return blackboard != null;
                }
                else if (unityObject is Component component)
                {
                    if (component.TryGetComponent<IBlackboardProvider>(out var provider))
                    {
                        blackboard = provider.Blackboard;
                        return blackboard != null;
                    }
                }
            }
            
            blackboard = null;
            return false;
        }

        private static bool TypeArrayContainsTypeOrBaseType(Type[] typeArray, Type type)
        {
            foreach (var t in typeArray)
            {
                if (type == t || t.IsAssignableFrom(type))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
