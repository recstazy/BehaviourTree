using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Recstazy.BehaviourTree
{
    [System.Serializable]
    internal class TreeNodeData : IEnumerable<NodeData>
    {
        [SerializeField]
        private TaskNodeData[] _taskData;

        [SerializeField]
        private VarNodeData[] _varData;

        public TaskNodeData[] TaskData { get => _taskData; internal set => _taskData = value; }
        public VarNodeData[] VarData { get => _varData; internal set => _varData = value; }

        public TreeNodeData() 
        {
            TaskData = new TaskNodeData[0];
            VarData = new VarNodeData[0];
        }

        public TreeNodeData(TaskNodeData[] data, params VarNodeData[] varData)
        {
            TaskData = data;
            VarData = varData;
        }

        public IEnumerator<NodeData> GetEnumerator()
        {
            return new NodeDataEnumerator(_taskData, _varData);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddData(params NodeData[] data)
        {
            var taskData = data.Select(d => d as TaskNodeData).Where(d => d != null).ToArray();
            _taskData = _taskData.Concat(taskData).ToArray();

            var varData = data.Select(d => d as VarNodeData).Where(d => d != null).ToArray();
            _varData = _varData.Concat(varData).ToArray();
        }

        public void RemoveData(params NodeData[] data)
        {
            _taskData = _taskData.Where(d => !data.Contains(d)).ToArray();
            _varData = _varData.Where(d => !data.Contains(d)).ToArray();
        }

        public class NodeDataEnumerator : IEnumerator<NodeData>
        {
            private TaskNodeData[] _taskData;
            private VarNodeData[] _varData;
            private int _curIndex;
            private int _sumLength;
            public NodeData Current => GetCurrent();
            object IEnumerator.Current => Current;

            public NodeDataEnumerator(TaskNodeData[] taskData, VarNodeData[] varData)
            {
                _taskData = taskData;
                _varData = varData;
                _sumLength = _taskData.Length + _varData.Length;
                _curIndex = -1;
            }

            public void Dispose()
            {
                _taskData = null;
                _varData = null;
            }

            public bool MoveNext()
            {
                _curIndex++;
                return _curIndex < _sumLength;
            }

            public void Reset()
            {
                _curIndex = -1;
            }

            private NodeData GetCurrent()
            {
                if (_curIndex < _taskData.Length) return _taskData[_curIndex];
                else if (_curIndex < _sumLength) return _varData[_curIndex - _taskData.Length];

                return null;
            }
        }
    }
}
