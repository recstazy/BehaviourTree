using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    internal struct OutputDescription : IConnectionDescription
    {
        public readonly string Name;
        public readonly int Index;
        public readonly Type OutType;

        public string PortName => Name;
        public Type PortType => OutType;
        public int LastConnectedNode { get; set; }

        public OutputDescription(int index, string name, Type outType)
        {
            Name = name;
            Index = index;
            OutType = outType == null ? typeof(ExecutionPin) : outType;
            LastConnectedNode = -1;
        }
    }
}
