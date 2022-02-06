using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut]
    [ExcludeFromTaskSelector]
    [NoInspector]
    internal sealed class EntryTask : BehaviourTask { }
}
