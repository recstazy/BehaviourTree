using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    public enum BlackboardProperty { GetterOrSetter = 0, Getter = 1, Setter = 2 }

    internal static class BlackboardExtensions
    {
        private static readonly string[] s_emptyNames = new string[0];

        internal static bool IncludesGetter(this BlackboardProperty type)
        {
            return type == BlackboardProperty.Getter || type == BlackboardProperty.GetterOrSetter;
        }

        internal static bool IncludesSetter(this BlackboardProperty type)
        {
            return type == BlackboardProperty.Setter || type == BlackboardProperty.GetterOrSetter;
        }

        internal static string[] GetNames(this Blackboard blackboard, BlackboardProperty typeMask)
        {
            if (blackboard == null || blackboard.GetterValues == null || blackboard.SetterValues == null) return s_emptyNames;
            string[] namesResult = new string[0];

            if (typeMask.IncludesGetter())
            {
                var names = blackboard.GetterValues.Keys.Where(key => !string.IsNullOrEmpty(key));
                namesResult = namesResult.Concat(names).ToArray();
            }

            if (typeMask.IncludesSetter())
            {
                var names = blackboard.SetterValues.Keys.Where(key => !string.IsNullOrEmpty(key) && !namesResult.Contains(key));
                namesResult = namesResult.Concat(names).ToArray();
            }

            return namesResult;
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

                    if (typeMask.IncludesGetter())
                    {
                        if (blackboard.GetterValues.TryGetValue(n, out var getAccessor))
                        {
                            type = getAccessor.PropertyType;
                        }
                    }
                    else if (typeMask.IncludesSetter())
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
