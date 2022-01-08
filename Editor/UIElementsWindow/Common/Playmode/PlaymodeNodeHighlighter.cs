using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class PlaymodeNodeHighlighter : IPlaymodeDependent
    {
        #region Fields

        private Func<IList<BTNode>> _nodes;
        private Func<IList<EdgeReference>> _edges;
        private Dictionary<VisualElement, VisualElement> _highlights;

        #endregion

        #region Properties

        #endregion

        public void Bind(Func<IList<BTNode>> nodes, Func<IList<EdgeReference>> edges)
        {
            Clear();
            _nodes = nodes;
            _edges = edges;
        }

        public void PlaymodeChanged(bool isPlaymode)
        {
            if (isPlaymode) CreateHighlights();
            else Clear();
        }

        public void Clear()
        {
            if (_highlights == null) return;

            foreach (var pair in _highlights)
            {
                pair.Key.Remove(pair.Value);
            }

            _highlights.Clear();
            _highlights = null;
        }

        private void CreateHighlights()
        {
            _highlights = new Dictionary<VisualElement, VisualElement>();
            var nodes = _nodes().ToArray();

            foreach (var n in nodes)
            {
                var element = new VisualElement();
                element.AddToClassList("node-highlight");
                element.SetEnabled(false);
                n.Add(element);
                _highlights.Add(n, element);
            }
        }
    }
}
