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

        private GameObject _gameObject;
        private NavMeshAgent _navAgent;

        #endregion

        #region Properties

        /// <summary> Currently available variables </summary>
        public Dictionary<string, ITypedValue> Values { get; private set; }

        #endregion

#if UNITY_EDITOR

        private void OnValidate()
        {
            UpdateValuesDictionary(false);
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
            _gameObject = gameObject;
            _navAgent = gameObject.GetComponentInChildren<NavMeshAgent>();
            UpdateValuesDictionary(true);
        }

        internal void UpdateValuesDictionary(bool onInitialization)
        {
            Values = new Dictionary<string, ITypedValue>()
            {
                { CommonNames.GameObject, new GameObjectValue(_gameObject) },
                { CommonNames.Transform, new TransformValue(_gameObject != null ? _gameObject.transform : null) },
                { CommonNames.NavAgent, new NavAgentValue(_navAgent) }
            };

            if (_values == null) return;

            foreach (var v in _values)
            {
                if (Values.ContainsKey(v.Name))
                {
                    if (onInitialization)
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
    }

}
