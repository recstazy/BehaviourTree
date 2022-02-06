using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Recstazy.BehaviourTree.PropertyBinding;

namespace Recstazy.BehaviourTree
{
    internal struct AnyValueType { }

    public class TreeValueFunc : VariableFunc
    {
        #region Fields

        [SerializeField]
        private SerializedValue _value;

        #endregion

        #region Properties

        public SerializedValue Value { get => _value; set => _value = value; }

        #endregion

        public TreeValueFunc() : base(typeof(AnyValueType)) { }

        public TreeValueFunc(object value, Type type) : base(type)
        {
            _value = new SerializedValue(value);
        }

        public override FuncOut[] GetOuts()
        {
            return new FuncOut[] { new FuncOut(VariableType, string.Empty) };
        }

        public override Delegate GetValueGetter(FuncOut funcOut)
        {
            string propertyName = GetPropertyName();
            var property = typeof(SerializedValue).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Type funcReturnType;

            if (_value.EnumType != SerializedValue.ValueType.Complex)
            {
                funcReturnType = property.PropertyType;
            }
            else
            {
                funcReturnType = JsonHelper.StringToType(_value.ValueTypeString);
            }

            return PropertyBinder.CreateGenericFuncForceType(property, _value, funcReturnType);
        }

        private string GetPropertyName()
        {
            switch (_value.EnumType)
            {
                case SerializedValue.ValueType.Bool:
                    return "BoolValue";
                case SerializedValue.ValueType.Float:
                    return "FloatValue";
                case SerializedValue.ValueType.Int:
                    return "IntValue";
                case SerializedValue.ValueType.String:
                    return "StringValue";
                default:
                    return "ComplexValue";
            }
        }
    }
}
