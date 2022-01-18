using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [System.Serializable]
    internal struct TaskConnection
    {
        [SerializeField]
        private int _outPin;

        [SerializeField]
        private int _inNode;

        public int OutPin => _outPin;
        public int InNode => _inNode;
        public bool IsValid { get; private set; }

        public TaskConnection(int outPin, int inNode)
        {
            _outPin = outPin;
            _inNode = inNode;
            IsValid = true;
        }
    }
}
