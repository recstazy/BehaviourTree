using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    internal struct InputDescription
    {
        public readonly Type ValueType;
        public readonly bool IsGetter;
        public readonly string Name;

        public static InputDescription ExecutionInput => new InputDescription(typeof(ExecutionPin), false, ExecutionInName);
        public const string ExecutionInName = "__execution__";

        public InputDescription(Type valueType, bool isGetter, string name)
        {
            ValueType = valueType;
            IsGetter = isGetter;
            Name = name;
        }
    }
}
