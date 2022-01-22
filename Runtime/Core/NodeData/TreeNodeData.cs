using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Recstazy.BehaviourTree
{
    [System.Serializable]
    internal class TreeNodeData
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
    }
}
