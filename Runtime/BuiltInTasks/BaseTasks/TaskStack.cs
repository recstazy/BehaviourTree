using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Linq;

namespace Recstazy.BehaviourTree
{
    internal class TaskStack
    {
        public event System.Action OnStackChanged;

        private struct NodeDescription
        {
            public readonly int Index;
            public readonly TaskConnection[] Connections;

            public NodeDescription(int index, TaskConnection[] connections)
            {
                Index = index;
                int maxConnectionIndex;

                if (connections == null || connections.Length == 0) maxConnectionIndex = -1;
                else maxConnectionIndex = connections.Max(c => c.OutPin);

                Connections = new TaskConnection[maxConnectionIndex + 1];

                for (int i = 0; i < connections.Length; i++)
                {
                    Connections[connections[i].OutPin] = connections[i];
                }
            }
        }

        public struct CallInfo
        {
            public int Node;
            public int OutIndex;

            public CallInfo(int node, int outIndex)
            {
                Node = node;
                OutIndex = outIndex;
            }

            public override string ToString()
            {
                return $"{Node}({OutIndex})->";
            }
        }

        #region Fields

        private NodeDescription[] _description;
        private List<CallInfo> _stack;

        #endregion

        #region Properties

        public ReadOnlyCollection<CallInfo> Stack => _stack.AsReadOnly();

        #endregion

        public TaskStack(TreeNodeData nodeData)
        {
            _stack = new List<CallInfo>();
            var maxIndex = nodeData.Data.Max(d => d.Index);
            _description = new NodeDescription[maxIndex + 1];

            foreach (var data in nodeData.Data)
            {
                _description[data.Index] = new NodeDescription(data.Index, data.Connections);
            }
        }

        public void Clear()
        {
            _stack.Clear();
        }

        public void TaskStarted(int taskIndex, int startedFromPin)
        {
            bool wasCut = CutStackToEntryPoint(taskIndex, startedFromPin);
            _stack.Add(new CallInfo(taskIndex, -1));
            Debug.Log(string.Join(", ", _stack));
            OnStackChanged?.Invoke();
        }

        private bool CutStackToEntryPoint(int currentTask, int startedFromPin)
        {
            int countToRemove = 0;
            CallInfo prevCall;

            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                prevCall = _stack[i];
                var prevDescription = _description[prevCall.Node];
                var prevConnections = prevDescription.Connections;

                if (startedFromPin < prevConnections.Length && prevConnections[startedFromPin].IsValid
                    && prevConnections[startedFromPin].InNode == currentTask)
                {
                    prevCall.OutIndex = startedFromPin;
                    _stack[i] = prevCall;
                    break;
                }

                countToRemove++;
            }

            bool shouldCut = countToRemove > 0;

            if (shouldCut)
            {
                if (countToRemove == _stack.Count)
                {
                    _stack.Clear();
                }
                else
                {
                    for (int i = 0; i < countToRemove; i++)
                    {
                        _stack.RemoveAt(_stack.Count - 1);
                    }
                }
            }

            return shouldCut;
        }
    }
}
