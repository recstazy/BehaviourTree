using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    internal interface IConnectionDescription
    {
        string PortName { get; }
        object UserData { get; }
        Type PortType { get; }
    }
}
