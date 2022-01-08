using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class EdgeUpdater : IPlaymodeDependent
    {
        private Edge _edge;

        public EdgeUpdater(Edge edge)
        {
            _edge = edge;
        }

        public void PlaymodeChanged(bool isPlaymode)
        {
            _edge.SetEnabled(!isPlaymode);
        }
    }
}
