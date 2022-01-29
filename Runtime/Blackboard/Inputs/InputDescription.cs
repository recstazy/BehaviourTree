using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    internal struct InputDescription : IConnectionDescription
    {
        public const string ExecutionInName = "[execution]";
        public static InputDescription ExecutionInput => new InputDescription(typeof(ExecutionPin), false, ExecutionInName, string.Empty);

        public readonly Type ValueType;
        public readonly bool IsGetter;
        public readonly string IdName;
        public readonly string DisplayName;

        public string PortName => DisplayName;
        public Type PortType => ValueType;
        public bool IsValid { get; private set; }

        public InputDescription(Type valueType, bool isGetter, string idName, string displayName)
        {
            ValueType = valueType;
            IsGetter = isGetter;
            IdName = idName;
            DisplayName = displayName;
            IsValid = true;
        }
    }
}
