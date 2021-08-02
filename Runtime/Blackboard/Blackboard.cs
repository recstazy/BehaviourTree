using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Recstazy.BehaviourTree
{
    public enum ValueType { None = 0, Bool = 1, Number = 2, String = 3, Object = 4 }

    /// <summary>
    /// The container of variables which can be used by tasks
    /// </summary>
    [CreateAssetMenu(fileName = "NewBlackboard", menuName = "Behaviour Tree/New Blackboard", order = 132)]
    public class Blackboard : ScriptableObject
    {
        #region Fields

        [SerializeField]
        private TypedValue[] _values;

        private static readonly string[] _emptyNames = new string[0];
        private static readonly string[] _reservedNames = new string[]
        {
            "GameObject",
            "Transform",
            "NavAgent"
        };

        #endregion

        #region Properties

        /// <summary>Currently available variables, only exists in runtime.</summary>
        public Dictionary<string, ITypedValue> Values { get; private set; }

        public Transform Transform { get; private set; }
        public GameObject GameObject { get; private set; }
        public NavMeshAgent NavAgent { get; private set; }

        #endregion

#if UNITY_EDITOR

        private void OnValidate()
        {
            UpdateValuesDictionary();
        }

#endif

        /// <summary>Get the variable corresponding to the name, including editor</summary>
        /// <returns>Returns false if there's no value with this name</returns>
        public bool TryGetValue(string name, out ITypedValue value)
        {
            return TryGetValueRuntime(name, out value);
        }

        /// <summary>Get the variable corresponding to the name and try cast it to T, including editor</summary>
        /// <returns>Returns false if there's no value with this name or cast was unsuccessful</returns>
        public bool TryGetValue<T>(string name, out T value)
        {
            return TryGetValueRuntime(name, out value);
        }

        /// <summary>Set value by name, works only in runtime</summary>
        public void SetValue(string name, ITypedValue value)
        {
            if (!Application.isPlaying) return;

            if (Values.ContainsKey(name))
            {
                Values[name] = value;
            }
            else
            {
                Values.Add(name, value);
            }
        }

        public bool IsValueSet(string valueName)
        {
            if (TryGetValue(valueName, out var value))
            {
                if (value != null)
                {
                    if (value.MainValue is UnityEngine.Object unityObject)
                    {
                        return unityObject != null;
                    }
                    else
                    {
                        return value.MainValue != null;
                    }
                }
            }

            return false;
        }

        private bool TryGetValueRuntime(string name, out ITypedValue value)
        {
            return Values.TryGetValue(name, out value);
        }

        private bool TryGetValueRuntime<T>(string name, out T value)
        {
            if (TryGetValue(name, out var iValue))
            {
                if (iValue is T tvalue)
                {
                    value = tvalue;
                    return true;
                }
            }

            value = default;
            return false;
        }

        [RuntimeInstanced]
        internal void InitializeAtRuntime(GameObject gameObject)
        {
            GameObject = gameObject;
            Transform = gameObject.transform;
            NavAgent = gameObject.GetComponentInChildren<NavMeshAgent>();
            UpdateValuesDictionary();
        }

        internal void UpdateValuesDictionary()
        {
            Values = new Dictionary<string, ITypedValue>()
            {
                { _reservedNames[0], new GameObjectValue(GameObject) },
                { _reservedNames[1], new TransformValue(Transform) },
                { _reservedNames[2], new NavAgentValue(NavAgent) }
            };

            foreach (var v in _values)
            {
                if (Values.ContainsKey(v.Name))
                {
                    if (Application.isPlaying)
                    {
                        Debug.LogError($"Blackboard {name} has multiple values with name \"{v.Name}\"");
                    }
#if UNITY_EDITOR
                    else
                    {
                        string valueName = v.Name;
                        var nameSplitted = v.Name.Split(' ');

                        if (nameSplitted.Length > 1)
                        {
                            if (int.TryParse(nameSplitted[nameSplitted.Length - 1], out var index))
                            {
                                valueName = v.Name.Replace($" {nameSplitted[nameSplitted.Length - 1]}", "");
                            }
                        }

                        var nameIndices = _values.Where(value => value.Name.Contains(valueName)).Select(value => value.Name.Replace($"{valueName} ", "")).ToArray();
                        int maxIndex = 0;

                        foreach (var i in nameIndices)
                        {
                            if (int.TryParse(i, out var index))
                            {
                                maxIndex = Mathf.Max(index, maxIndex);
                            }
                        }

                        v.ChangeName($"{valueName} {maxIndex + 1}");
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
#endif
                }
                else
                {
                    Values.Add(v.Name, v.Value);
                }
            }
        }

        internal string[] GetNames()
        {
            if (Values != null && Values.Count > 0)
            {
                return Values.Where(v => !string.IsNullOrEmpty(v.Key)).Select(v => v.Key).ToArray();
            }

            return _emptyNames;
        }

        internal string[] GetNamesTyped(params Type[] compatableTypes)
        {
            if (compatableTypes != null && compatableTypes.Length > 0)
            {
                var names = GetNames();
                return names.Where(n => compatableTypes.Contains(Values[n].GetType())).ToArray();
            }

            return _emptyNames;
        }
    }

}
