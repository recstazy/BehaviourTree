using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal struct OutputDescription : IConnectionDescription
    {
        public readonly string Name;
        public readonly int Index;
        public readonly Type OutType;

        public string PortName => Name;
        public Type PortType => OutType;
        public InputDescription LastConnectedInput { get; set; }
        public int LastConnectedNode { get; set; }

        public OutputDescription(int index, string name, Type outType)
        {
            Name = name;
            Index = index;
            OutType = outType == null ? typeof(ExecutionPin) : outType;
            LastConnectedInput = default;
            LastConnectedNode = -1;
        }
    }

    internal static class OutpuExtensions
    {
        public static OutputDescription GetOutDescription(this Port outPort)
        {
            return (OutputDescription)outPort.userData;
        }

        public static void SetLastConnected(this Port outPort, int node, InputDescription inputDesc)
        {
            var outDescription = outPort.GetOutDescription();
            outDescription.LastConnectedInput = inputDesc;
            outDescription.LastConnectedNode = node;
            outPort.userData = outDescription;
        }
    }

}
