using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    [Serializable]
    public class SerializedValue
    {
        public enum ValueType { Bool, Float, Int, String, Complex }

        [SerializeField]
        private bool _boolValue;

        [SerializeField]
        private float _floatValue;

        [SerializeField]
        private int _intValue;

        [SerializeField]
        private string _stringValue;

        [SerializeReference]
        private object _complexValue;

        [SerializeField]
        private string _valueTypeString;

        [SerializeField]
        private ValueType _enumType;

        public object Value { get => GetValue(); set => SetValue(value); }
        public ValueType EnumType { get => _enumType; }

        public bool BoolValue { get => _boolValue; set => _boolValue = value; }
        public float FloatValue { get => _floatValue; set => _floatValue = value; }
        public int IntValue { get => _intValue; set => _intValue = value; }
        public string StringValue { get => _stringValue; set => _stringValue = value; }
        public object ComplexValue { get => _complexValue; set => _complexValue = value; }
        public string ValueTypeString { get => _valueTypeString; }

        public SerializedValue(object value)
        {
            SetValue(value);
        }

        public object GetValue()
        {
            switch (_enumType)
            {
                case ValueType.Bool:
                    return _boolValue;
                case ValueType.Float:
                    return _floatValue;
                case ValueType.Int:
                    return _intValue;
                case ValueType.String:
                    return _stringValue;
                case ValueType.Complex:
                    return _complexValue;
                default:
                    return null;
            }
        }

        public void SetValue(object value)
        {
            ValueType type;

            if (value is bool) type = ValueType.Bool;
            else if (value is float) type = ValueType.Float;
            else if (value is int) type = ValueType.Int;
            else if (value is string) type = ValueType.String;
            else type = ValueType.Complex;

            switch (type)
            {
                case ValueType.Bool:
                    _boolValue = (bool)value;
                    break;
                case ValueType.Float:
                    _floatValue = (float)value;
                    break;
                case ValueType.Int:
                    _intValue = (int)value;
                    break;
                case ValueType.String:
                    _stringValue = (string)value;
                    break;
                case ValueType.Complex:
                    _complexValue = value;
                    _valueTypeString = JsonHelper.GetTypeAsString(value);
                    break;
                default:
                    break;
            }

            _enumType = type;
        }
    }
}
