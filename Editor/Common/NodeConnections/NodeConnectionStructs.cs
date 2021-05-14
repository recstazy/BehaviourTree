using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal struct NodeConnectionEventArgs
    {
        public readonly int OutNode;
        public readonly int OutPin;
        public readonly int InNode;

        public NodeConnectionEventArgs(int outNode, int outPin, int inNode)
        {
            OutNode = outNode;
            OutPin = outPin;
            InNode = inNode;
        }
    }
}
