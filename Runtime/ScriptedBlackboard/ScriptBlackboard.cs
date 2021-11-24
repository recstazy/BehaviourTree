using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// The container of variables which can be used by tasks
    /// </summary>
    public class ScriptBlackboard : MonoBehaviour
    {
        private class PropertyDelegateBinding<T> where T : Delegate
        {
            public T Accessor;
            public Type PropertyType;

            public PropertyDelegateBinding(T accessor, Type propertyType)
            {
                Accessor = accessor;
                PropertyType = propertyType;
            }
        }

        #region Fields

        private Dictionary<string, PropertyDelegateBinding<Func<object, object>>> _getters;
        private Dictionary<string, PropertyDelegateBinding<Action<object, object>>> _setters;

        #endregion

        #region Properties

        public float SomeGetSetFloat { get; set; } = 1.5f;

        #endregion

        private void Start()
        {
            CreateMappings();
            Test();
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            if (_getters.TryGetValue(name, out var binding))
            {
                if (typeof(T).IsAssignableFrom(binding.PropertyType))
                {
                    value = (T)binding.Accessor(this);
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
                    binding.Accessor(this, value);
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

        internal void CreateMappings()
        {
            var bindableProperties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
            bindableProperties = bindableProperties.Where(p => typeof(ScriptBlackboard).IsAssignableFrom(p.DeclaringType) && !p.GetIndexParameters().Any()).ToArray();

            var publicGetters = bindableProperties.Where(p => p.CanRead && p.GetGetMethod() != null && p.GetGetMethod().IsPublic).ToArray();
            _getters = new Dictionary<string, PropertyDelegateBinding<Func<object, object>>>();

            foreach (var getterProp in publicGetters)
            {
                var getterFunc = PropertyBindHelper.CreateGetter(getterProp);
                _getters.Add(getterProp.Name, new PropertyDelegateBinding<Func<object, object>>(getterFunc, getterProp.PropertyType));
            }

            var publicSetters = bindableProperties.Where(p => p.CanWrite && p.GetSetMethod() != null && p.GetSetMethod().IsPublic).ToArray();
            _setters = new Dictionary<string, PropertyDelegateBinding<Action<object, object>>>();

            foreach (var setterProp in publicSetters)
            {
                var setterAction = PropertyBindHelper.CreateSetter(setterProp);
                _setters.Add(setterProp.Name, new PropertyDelegateBinding<Action<object, object>>(setterAction, setterProp.PropertyType));
            }
        }

        private void Test()
        {
            var hasValue = TryGetValue<float>("SomeGetSetFloat", out var value);
            Debug.Log(hasValue ? value.ToString() : "No value");

            TrySetValue("SomeGetSetFloat", 2f);
            TryGetValue("SomeGetSetFloat", out value);
            Debug.Log("newValue = " + value);

            TrySetValue("DoubleGetter", 15);
            TryGetValue("DoubleGetter", out double doubl);
            Debug.Log("Double = " + doubl);

            TrySetValue("LambdaIntGetter", 99);
            TryGetValue("LambdaIntGetter", out int integer);
            Debug.Log("LambdaIntGetter = " + integer);

            TrySetValue("IntSetter", 74);
            TryGetValue("IntSetter", out integer);
            Debug.Log("IntSetter = " + integer);

            TrySetValue("PrivateString", "aaa");
            TryGetValue("PrivateString", out string str);
            Debug.Log("PrivateString = " + str);

            Debug.Log(string.Join(", ", _getters.Select(g => g.Key).ToArray()));
            Debug.Log(string.Join(", ", _setters.Select(s => s.Key).ToArray()));

            Debug.Log("Can set float -> string " + TrySetValue("SomeGetSetFloat", "aaa"));
            Debug.Log("Can get float -> string " + TryGetValue("SomeGetSetFloat", out string sss));

            TryGetValue<object>("Fake name", out var val);
            TrySetValue<object>("Fake name", val);
        }
    }

    internal static class PropertyBindHelper
    {
        public static Func<object, object> CreateGetter(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException("Property is null");

            var getter = property.GetGetMethod();
            if (getter == null)
                throw new ArgumentException("The specified property does not have a public accessor.");

            var genericMethod = typeof(PropertyBindHelper).GetMethod("CreateGetterGeneric");
            MethodInfo genericHelper = genericMethod.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Func<object, object>)genericHelper.Invoke(null, new object[] { getter });
        }

        public static Func<object, object> CreateGetterGeneric<T, R>(MethodInfo getter) where T : class
        {
            Func<T, R> getterTypedDelegate = (Func<T, R>)Delegate.CreateDelegate(typeof(Func<T, R>), getter);
            Func<object, object> getterDelegate = (Func<object, object>)((object instance) => getterTypedDelegate((T)instance));
            return getterDelegate;
        }

        public static Action<object, object> CreateSetter(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException("Property is null");

            var setter = property.GetSetMethod();
            if (setter == null)
                throw new ArgumentException("The specified property does not have a public setter.");

            var genericMethod = typeof(PropertyBindHelper).GetMethod("CreateSetterGeneric");
            MethodInfo genericHelper = genericMethod.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Action<object, object>)genericHelper.Invoke(null, new object[] { setter });
        }

        public static Action<object, object> CreateSetterGeneric<T, V>(MethodInfo setter) where T : class
        {
            Action<T, V> setterTypedDelegate = (Action<T, V>)Delegate.CreateDelegate(typeof(Action<T, V>), setter);
            Action<object, object> setterDelegate = (Action<object, object>)((object instance, object value) => { setterTypedDelegate((T)instance, (V)value); });
            return setterDelegate;
        }
    }
}