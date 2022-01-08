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
        private Dictionary<VisualElement, VisualElement> _highlights;
        private TreePlayer _currentPlayer;
        private Dictionary<BTNode, VisualElement> _nodesInChain;

        #endregion

        #region Properties

        #endregion

        public void Bind(Func<IList<BTNode>> nodes)
        {
            Clear();
            _nodes = nodes;
        }

        // This also called when tree asset is changed 
        // because BTWindow will set new dependencies to "PlaymodeWatcher"
        public void PlaymodeChanged(bool isPlaymode)
        {
            if (_currentPlayer != null) _currentPlayer.Stack.OnStackChanged -= StackChanged;
            
            if (isPlaymode)
            {
                _currentPlayer = TreeSelector.CurrentPlayer;

                if (_currentPlayer != null)
                {
                    CreateHighlights();
                    _currentPlayer.Stack.OnStackChanged += StackChanged;
                }
            }
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

        private void StackChanged()
        {
            foreach (var n in _nodesInChain)
            {
                SetHighlightActive(n.Value, false);
            }

            _nodesInChain.Clear();

            foreach (var info in _currentPlayer.Stack.Stack)
            {
                var node = _nodes().FirstOrDefault(n => n.Data.Index == info.Node);

                if (node != null)
                {
                    var highlight = _highlights[node];
                    SetHighlightActive(highlight, true);
                    _nodesInChain.Add(node, highlight);
                }
            }
        }

        private void CreateHighlights()
        {
            _highlights = new Dictionary<VisualElement, VisualElement>();
            _nodesInChain = new Dictionary<BTNode, VisualElement>();
            var nodes = _nodes().ToArray();

            foreach (var n in nodes)
            {
                var element = new VisualElement();
                element.AddToClassList("node-highlight");
                SetHighlightActive(element, false);
                n.Add(element);
                _highlights.Add(n, element);
            }
        }

        private void SetHighlightActive(VisualElement highlight, bool isActive)
        {
            highlight.style.opacity = isActive ? 1f : 0f;
        }
    }
}
