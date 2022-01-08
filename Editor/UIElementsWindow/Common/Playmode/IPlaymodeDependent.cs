using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public interface IPlaymodeDependent
    {
        void PlaymodeChanged(bool isPlaymode);
    }
}
