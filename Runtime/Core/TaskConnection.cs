using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [System.Serializable]
    internal struct TaskConnection
    {
        [SerializeField]
        private int outPin;

        [SerializeField]
        private int inNode;

        public int OutPin => outPin;
        public int InNode => inNode;
        public bool IsValid { get; private set; }

        public TaskConnection(int outPin, int inNode)
        {
            this.outPin = outPin;
            this.inNode = inNode;
            IsValid = true;
        }
    }
}
