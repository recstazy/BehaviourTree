using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    internal struct InputDescription : IConnectionDescription
    {
        public readonly Type ValueType;
        public readonly bool IsGetter;
        public readonly string IdName;
        public readonly string DisplayName;

        public static InputDescription ExecutionInput => new InputDescription(typeof(ExecutionPin), false, ExecutionInName, string.Empty);

        public string PortName => DisplayName;
        public object UserData => IdName;
        public Type PortType => ValueType;

        public const string ExecutionInName = "[execution]";

        public InputDescription(Type valueType, bool isGetter, string idName, string displayName)
        {
            ValueType = valueType;
            IsGetter = isGetter;
            IdName = idName;
            DisplayName = displayName;
        }
    }
}
