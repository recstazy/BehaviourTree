using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    public enum BlackboardProperty { Getter = 0, Setter = 1 }

    internal static class BlackboardExtensions
    {
        private static readonly string[] s_emptyNames = new string[0];

        internal static bool Includes(this BlackboardProperty mask, BlackboardProperty type)
        {
            return (mask & type) == type;
        }

        internal static string[] GetNames(this Blackboard blackboard, BlackboardProperty typeMask)
        {
            if (blackboard == null || blackboard.GetterValues == null || blackboard.SetterValues == null) return s_emptyNames;
            IEnumerable<string> namesResult = new string[0];

            if (typeMask.Includes(BlackboardProperty.Getter))
            {
                var names = blackboard.GetterValues.Keys.Where(key => !string.IsNullOrEmpty(key));
                namesResult = namesResult.Concat(names);
            }

            if (typeMask.Includes(BlackboardProperty.Setter))
            {
                var names = blackboard.SetterValues.Keys.Where(key => !string.IsNullOrEmpty(key));
                namesResult = namesResult.Concat(names);
            }

            return namesResult.ToArray();
        }

        internal static string[] GetNamesTyped(this Blackboard blackboard, BlackboardProperty typeMask, params Type[] compatableTypes)
        {
            if (blackboard != null && compatableTypes != null && compatableTypes.Length > 0)
            {
                var result = new List<string>();
                var names = blackboard.GetNames(typeMask);

                foreach (var n in names)
                {
                    Type type = null;

                    if (typeMask.Includes(BlackboardProperty.Getter))
                    {
                        if (blackboard.GetterValues.TryGetValue(n, out var getAccessor))
                        {
                            type = getAccessor.PropertyType;
                        }
                    }
                    else if (typeMask.Includes(BlackboardProperty.Setter))
                    {
                        if (blackboard.SetterValues.TryGetValue(n, out var setAccessor))
                        {
                            type = setAccessor.PropertyType;
                        }
                    }

                    if (type != null)
                    {
                        if (TypeArrayContainsTypeOrBaseType(compatableTypes, type))
                        {
                            result.Add(n);
                        }
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
