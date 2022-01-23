using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    [Serializable]
    internal struct ExecutionPin { }

    [Serializable]
    internal struct TaskConnection
    {
        [SerializeField]
        private int _outPin;

        [SerializeField]
        private int _inNode;

        [SerializeField]
        private string _inName;

        [SerializeField]
        private string _outTypeName;

        public int OutPin => _outPin;
        public int InNode => _inNode;
        public bool IsValid { get; private set; }
        public string OutTypeName { get => string.IsNullOrEmpty(_outTypeName) ? typeof(ExecutionPin).Name : _outTypeName; }
        public string InName => _inName;

        public TaskConnection(int outPin, int inNode, string inName, Type type = null)
        {
            _outPin = outPin;
            _inNode = inNode;
            _inName = inName;
            _outTypeName = type == null ? typeof(ExecutionPin).Name : type.Name; 
            IsValid = true;
        }
    }
}
