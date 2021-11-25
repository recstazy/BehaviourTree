using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.AI;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// The container of variables which can be used by tasks
    /// </summary>
    public class ScriptBlackboard : ScriptableObject
    {
        public class PropertyAccessor<T> where T : Delegate
        {
            public T Accessor { get; private set; }
            public Type PropertyType { get; private set; }

            public PropertyAccessor(T accessor, Type propertyType)
            {
                Accessor = accessor;
                PropertyType = propertyType;
            }
        }

        #region Fields

        private Dictionary<string, PropertyAccessor<Func<object>>> _getters;
        private Dictionary<string, PropertyAccessor<Action<object>>> _setters;

        #endregion

        #region Properties

        public GameObject GameObject { get; private set; }
        public NavMeshAgent NavAgent { get; private set; }
        public IReadOnlyDictionary<string, PropertyAccessor<Func<object>>> GetterValues => _getters;
        public IReadOnlyDictionary<string, PropertyAccessor<Action<object>>> SetterValues => _setters;

        #endregion

        public bool TryGetValue<T>(string name, out T value)
        {
            if (_getters.TryGetValue(name, out var binding))
            {
                if (typeof(T).IsAssignableFrom(binding.PropertyType))
                {
                    value = (T)binding.Accessor();
                    return true;
                }
                else
                {
                    Debug.LogError($"You're trying to get [{typeof(T)}] value from [{binding.PropertyType.Name}] property with name '{name}' in Blackboard '{this.name}'");
                }
            }
            else
            {
                Debug.LogError($"No getter with name '{name}' found in Blackboard '{this.name}'");
            }

            value = default;
            return false;
        }

        public bool TrySetValue<T>(string name, T value)
        {
            if (_setters.TryGetValue(name, out var binding))
            {
                if (binding.PropertyType.IsAssignableFrom(typeof(T)))
                {
                    binding.Accessor(value);
                    return true;
                }
                else
                {
                    Debug.LogError($"You're trying to set [{typeof(T)}] value to [{binding.PropertyType.Name}] property with name '{name}' in Blackboard '{this.name}'");
                }
            }
            else
            {
                Debug.LogError($"No setter with name '{name}' found in Blackboard '{this.name}'");
            }

            return false;
        }

        public bool IsValueSetAndNotNull<T>(string valueName)
        {
            if (TryGetValue(valueName, out T value))
            {
                return value != null;
            }

            return false;
        }

        [RuntimeInstanced]
        internal void InitializeAtRuntime(GameObject gameObject)
        {
            GameObject = gameObject;
            NavAgent = gameObject.GetComponentInChildren<NavMeshAgent>();
            CreateMappings();
        }

        internal void CreateMappings()
        {
            var bindableProperties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
            bindableProperties = bindableProperties.Where(p => typeof(ScriptBlackboard).IsAssignableFrom(p.DeclaringType) && !p.GetIndexParameters().Any()).ToArray();

            var publicGetters = bindableProperties.Where(p => p.CanRead && p.GetGetMethod() != null && p.GetGetMethod().IsPublic).ToArray();
            _getters = new Dictionary<string, PropertyAccessor<Func<object>>>();

            foreach (var getterProp in publicGetters)
            {
                var getterFunc = PropertyBindHelper.CreateGetter(getterProp);
                _getters.Add(getterProp.Name, new PropertyAccessor<Func<object>>(() => getterFunc(this), getterProp.PropertyType));
            }

            var publicSetters = bindableProperties.Where(p => p.CanWrite && p.GetSetMethod() != null && p.GetSetMethod().IsPublic).ToArray();
            _setters = new Dictionary<string, PropertyAccessor<Action<object>>>();

            foreach (var setterProp in publicSetters)
            {
                var setterAction = PropertyBindHelper.CreateSetter(setterProp);
                _setters.Add(setterProp.Name, new PropertyAccessor<Action<object>>((value) => setterAction(this, value), setterProp.PropertyType));
            }
        }
    }
}