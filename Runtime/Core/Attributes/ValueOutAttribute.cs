using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ValueOutAttribute : Attribute
    {
        public Type ValueType { get; private set; }
        public string Name { get; private set; }

        public ValueOutAttribute(Type valueType, string name = "")
        {
            ValueType = valueType;
            Name = name;
        }
    }
}
