using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        private TypedValue[] values;

        private static readonly string[] emptyNames = new string[0];

        #endregion

        #region Properties

        /// <summary>Currently available variables, only exists in runtime.</summary>
        public Dictionary<string, ITypedValue> Values { get; private set; }

        #endregion

        /// <summary>Get the variable corresponding to the name, including editor</summary>
        /// <returns>Returns false if there's no value with this name</returns>
        public bool TryGetValue(string name, out ITypedValue value)
        {
            if (Application.isPlaying)
            {
                return TryGetValueRuntime(name, out value);
            }
            else
            {
                return TryGetValueEditor(name, out value);
            }
        }

        /// <summary>Get the variable corresponding to the name and try cast it to T, including editor</summary>
        /// <returns>Returns false if there's no value with this name or cast was unsuccessful</returns>
        public bool TryGetValue<T>(string name, out T value)
        {
            if (Application.isPlaying)
            {
                return TryGetValueRuntime(name, out value);
            }
            else
            {
                return TryGetValueEditor(name, out value);
            }
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
                var result = Equals(value, null) ? false : Equals(value.MainValue, null);
                return result;
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

        private bool TryGetValueEditor(string name, out ITypedValue value)
        {
            if (values.Length > 0)
            {
                var bbValue = values.FirstOrDefault(v => v.Name == name);

                if (bbValue != null)
                {
                    value = bbValue.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        private bool TryGetValueEditor<T>(string name, out T value)
        {
            if (values.Length > 0)
            {
                if (TryGetValueEditor(name, out var bbValue))
                {
                    if (bbValue is T tValue)
                    {
                        value = tValue;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        [RuntimeInstanced]
        internal void InitializeAtRuntime()
        {
            Values = new Dictionary<string, ITypedValue>();

            foreach (var v in values)
            {
                if (Values.ContainsKey(v.Name))
                {
                    Debug.LogError($"Blackboard {name} has multiple values with name {v.Name}, skipping duplications...");
                }
                else
                {
                    Values.Add(v.Name, v.Value);
                }
            }
        }

        internal string[] GetNames()
        {
            if (values != null && values.Length > 0)
            {
                return values.Where(v => !string.IsNullOrEmpty(v?.Name)).Select(v => v.Name).ToArray();
            }

            return emptyNames;
        }

        internal string[] GetNamesTyped(params Type[] compatableTypes)
        {
            if (values != null && values.Length > 0 && compatableTypes != null && compatableTypes.Length > 0)
            {
                return values.Where(v => !string.IsNullOrEmpty(v?.Name) && v?.Value != null && compatableTypes.Contains(v.Value.GetType())).Select(v => v.Name).ToArray();
            }

            return emptyNames;
        }

        internal void AddNameInEditor(string name)
        {
            var value = new TypedValue(name, null);
            AddValueInEditor(value);
        }

        internal void AddValueInEditor(TypedValue value)
        {
            if (value is null) return;
            
            foreach (var v in values)
            {
                if (v?.Name == value.Name) return;
            }

            System.Array.Resize(ref values, values.Length + 1);
            values[values.Length - 1] = value;
        }
    }

}
