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

        private Func<IList<BTNode>> _getNodes;
        private TreePlayer _currentPlayer;
        private BTNode[] _nodeAccessor;
        private Dictionary<int, VisualElement> _currentHighlights = new Dictionary<int, VisualElement>();

        #endregion

        #region Properties

        #endregion

        public void Bind(Func<IList<BTNode>> nodes)
        {
            Clear();
            _getNodes = nodes;
        }

        // This also called when tree asset is changed 
        // because BTWindow will set new dependencies to "PlaymodeWatcher"
        public void PlaymodeChanged(bool isPlaymode)
        {
            if (isPlaymode)
            {
                var lastPlayer = _currentPlayer;
                _currentPlayer = TreeSelector.CurrentPlayer;

                if (_currentPlayer != null)
                {
                    if (_currentPlayer != lastPlayer)
                    {
                        RegenerateNodeAccessor();
                        BindToTasks(lastPlayer, false);
                        BindToTasks(_currentPlayer, true);
                    }
                }
            }
            else Clear();
        }

        public void Clear()
        {
            foreach (var h in _currentHighlights)
            {
                h.Value.RemoveFromHierarchy();
            }

            _currentHighlights.Clear();
            BindToTasks(_currentPlayer, false);
        }

        // ShouldBind == false means unbind
        private void BindToTasks(TreePlayer player, bool shouldBind)
        {
            if (player != null)
            {
                foreach (var data in player.Tree.NodeData.Data)
                {
                    if (shouldBind)
                    {
                        data.TaskImplementation.OnStarted += TaskStarted;
                        data.TaskImplementation.OnFinished += TaskFinished;
                    }
                    else
                    {
                        data.TaskImplementation.OnStarted -= TaskStarted;
                        data.TaskImplementation.OnFinished -= TaskFinished;
                    }
                }
            }
        }

        private void RegenerateNodeAccessor()
        {
            var nodeList = _getNodes();
            var itemsCount = nodeList.Max(n => n.Data.Index) + 1;
            _nodeAccessor = new BTNode[itemsCount];

            for (int i = 0; i < nodeList.Count; i++)
            {
                _nodeAccessor[nodeList[i].Data.Index] = nodeList[i];
            }
        }

        private void TaskStarted(BehaviourTask task)
        {
            var highlight = CreateHighlight(_nodeAccessor[task.Index]);
            _currentHighlights.Add(task.Index, highlight);
        }

        private void TaskFinished(BehaviourTask task)
        {
            if (_currentHighlights.TryGetValue(task.Index, out var highlight))
            {
                highlight.RemoveFromHierarchy();
                _currentHighlights.Remove(task.Index);
            }
        }

        private VisualElement CreateHighlight(BTNode node)
        {
            var element = new VisualElement();
            element.AddToClassList("node-highlight");
            node.Add(element);
            return element;
        }
    }
}