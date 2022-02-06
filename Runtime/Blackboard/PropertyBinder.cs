using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace Recstazy.BehaviourTree.PropertyBinding
{
    public class PropertyAccessor<T> where T : Delegate
    {
        public T Accessor { get; private set; }
        public Type PropertyType { get; private set; }
        public Delegate GenericDelegate { get; }

        public PropertyAccessor(T accessor, Delegate genericDel, Type propertyType)
        {
            Accessor = accessor;
            PropertyType = propertyType;
            GenericDelegate = genericDel;
        }
    }

    internal static class PropertyBinder
    {
        public static PropertyAccessor<Func<object>> CreateGetter(PropertyInfo property, object target)
        {
            var getter = GetGetMethod(property);

            var genericMethod = typeof(PropertyBinder).GetMethod("CreateGetterGeneric");
            var genericHelper = genericMethod.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            var genericGetter = (Delegate)genericHelper.Invoke(null, new object[] { getter, target });

            var boxedMethod = typeof(PropertyBinder).GetMethod("CreateGetterBoxed");
            var boxedHelper = boxedMethod.MakeGenericMethod(property.PropertyType);
            var boxedGetter = (Func<object>)boxedHelper.Invoke(null, new object[] { genericGetter });

            var accessor = new PropertyAccessor<Func<object>>(boxedGetter, genericGetter, property.PropertyType);
            return accessor;
        }

        public static Delegate CreateGenericFuncForceType(PropertyInfo property, object target, Type funcReturnType)
        {
            var getter = GetGetMethod(property);
            var genericMethod = typeof(PropertyBinder).GetMethod("CreateGetterGeneric");
            var genericHelper = genericMethod.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            var genericGetter = (Delegate)genericHelper.Invoke(null, new object[] { getter, target });

            if (getter.ReturnType != funcReturnType)
            {
                var castMethod = typeof(PropertyBinder).GetMethod("CastFuncToType");
                var castHelper = castMethod.MakeGenericMethod(funcReturnType, getter.ReturnType);
                var castedGetter = (Delegate)castHelper.Invoke(null, new object[] { genericGetter });
                return castedGetter;
            }
            else return genericGetter;
        }

        public static Func<TResult> CreateGetterGeneric<T, TResult>(MethodInfo getter, object target) where T : class
        {
            var getterDelegate = (Func<T, TResult>)Delegate.CreateDelegate(typeof(Func<T, TResult>), getter);
            var upcastedTarget = (T)target;
            return () => getterDelegate.Invoke(upcastedTarget);
        }

        public static Func<object> CreateGetterBoxed<T>(Func<T> func)
        {
            return () => func();
        }

        public static Func<TResult> CastFuncToType<TResult, TInput>(Func<TInput> func) where TResult : TInput
        {
            return () => (TResult)func();
        }

        public static Func<object, object> CreateGetterBoxedFromMethod<T, R>(MethodInfo getter) where T : class
        {
            Func<T, R> getterTypedDelegate = (Func<T, R>)Delegate.CreateDelegate(typeof(Func<T, R>), getter);
            Func<object, object> getterDelegate = (Func<object, object>)((object instance) => getterTypedDelegate((T)instance));
            return getterDelegate;
        }

        private static MethodInfo GetGetMethod(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException("Property is null");

            var getter = property.GetGetMethod();
            if (getter == null)
                throw new ArgumentException("The specified property does not have a public accessor.");

            return getter;
        }

        public static PropertyAccessor<Action<object>> CreateSetter(PropertyInfo property, object target)
        {
            if (property == null)
                throw new ArgumentNullException("Property is null");

            var setter = property.GetSetMethod();
            if (setter == null)
                throw new ArgumentException("The specified property does not have a public setter.");

            var genericMethod = typeof(PropertyBinder).GetMethod("CreateSetterGeneric");
            var genericHelper = genericMethod.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            var genericSetter = (Delegate)genericHelper.Invoke(null, new object[] { setter, target });

            var boxedMethod = typeof(PropertyBinder).GetMethod("CreateSetterBoxed");
            var boxedHelper = boxedMethod.MakeGenericMethod(property.PropertyType);
            var boxedSetter = (Action<object>)boxedHelper.Invoke(null, new object[] { genericSetter });

            var accessor = new PropertyAccessor<Action<object>>(boxedSetter, genericSetter, property.PropertyType);
            return accessor;
        }

        public static Action<TValue> CreateSetterGeneric<T, TValue>(MethodInfo setter, object target) where T : class
        {
            var setterDelegate = (Action<T, TValue>)Delegate.CreateDelegate(typeof(Action<T, TValue>), setter);
            var upcastedTarget = (T)target;
            return (value) => setterDelegate.Invoke(upcastedTarget, value);
        }

        public static Action<object> CreateSetterBoxed<T>(Action<T> setAction)
        {
            return (value) => setAction((T)value);
        }

        public static Action<object, object> CreateSetterGenericFromMethod<T, V>(MethodInfo setter) where T : class
        {
            Action<T, V> setterTypedDelegate = (Action<T, V>)Delegate.CreateDelegate(typeof(Action<T, V>), setter);
            Action<object, object> setterDelegate = (Action<object, object>)((object instance, object value) => { setterTypedDelegate((T)instance, (V)value); });
            return setterDelegate;
        }
    }
}
