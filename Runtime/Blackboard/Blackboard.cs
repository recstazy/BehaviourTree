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
    public class Blackboard : ScriptableObject
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

        private bool _logErrors;
        private Dictionary<string, PropertyAccessor<Func<object>>> _getters;
        private Dictionary<string, PropertyAccessor<Action<object>>> _setters;

        #endregion

        #region Properties

        public GameObject GameObject { get; private set; }
        public NavMeshAgent NavAgent { get; private set; }

        [HideInTree]
        public bool ArePropertiesBound => _getters != null && _setters != null;

        [HideInTree]
        public IReadOnlyDictionary<string, PropertyAccessor<Func<object>>> GetterValues
        {
            get
            {
                if (_getters == null)
                {
                    CreateMappings();
                }

                return _getters;
            }
        }

        [HideInTree]
        public IReadOnlyDictionary<string, PropertyAccessor<Action<object>>> SetterValues
        {
            get
            {
                if (_setters == null)
                {
                    CreateMappings();
                }

                return _setters;
            }
        }
        
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
                else if (_logErrors)
                {
                    Debug.LogError($"You're trying to get [{typeof(T).Name}] value from [{binding.PropertyType.Name}] property with name '{name}' in Blackboard '{this.name}'");
                }
            }
            else if (_logErrors)
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
                else if (_logErrors)
                {
                    Debug.LogError($"You're trying to set [{typeof(T).Name}] value to [{binding.PropertyType.Name}] property with name '{name}' in Blackboard '{this.name}'");
                }
            }
            else if(_logErrors)
            {
                Debug.LogError($"No setter with name '{name}' found in Blackboard '{this.name}'");
            }

            return false;
        }

        public bool TryGetValue(string name, out object value)
        {
            return TryGetValue(name, out value, out Type type);
        }

        public bool TryGetValue(string name, out object value, out Type valueType)
        {
            if (_getters.TryGetValue(name, out var binding))
            {
                value = binding.Accessor();
                valueType = binding.PropertyType;
                return true;
            }
            else if (_logErrors)
            {
                Debug.LogError($"No getter with name '{name}' found in Blackboard '{this.name}'");
            }

            value = default;
            valueType = default;
            return false;
        }

        public bool TrySetValue(string name, object value)
        {
            if (_setters.TryGetValue(name, out var binding))
            {
                if (binding.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    binding.Accessor(value);
                    return true;
                }
                else if (_logErrors)
                {
                    Debug.LogError($"You're trying to set [{value.GetType().Name}] value to [{binding.PropertyType.Name}] property with name '{name}' in Blackboard '{this.name}'");
                }
            }

            return false;
        }

        public bool IsValueSetAndNotNull(string valueName)
        {
            if (TryGetValue(valueName, out object value))
            {
                if (value is UnityEngine.Object uObject) return uObject != null;
                else return value != null;
            }

            return false;
        }

        public void ClearValue(string name)
        {
            if (_setters.TryGetValue(name, out var binding))
            {
                if (binding.PropertyType.IsValueType)
                {
                    binding.Accessor(Activator.CreateInstance(binding.PropertyType));
                }
                else binding.Accessor(null);
            }
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
            var bindableProperties = GetBindableProperties();
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

        internal PropertyInfo[] GetBindableProperties()
        {
            var bindableProperties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);

            bindableProperties = bindableProperties
                .Where(p => typeof(Blackboard).IsAssignableFrom(p.DeclaringType) && !p.GetIndexParameters().Any())
                .Where(p => p.GetCustomAttribute<HideInTreeAttribute>() == null)
                .ToArray();

            return bindableProperties;
        }
    }
}