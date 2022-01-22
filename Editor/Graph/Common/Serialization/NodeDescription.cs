using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [System.Serializable]
    internal struct NodeDescription
    {
        public bool IsVariableData;
        public int Index;
        public int TaskTypeIndex;
        public string TaskTypeString;
        public string TaskJson;
        public string VariableName;
        public string VariableType;
        public TaskConnection[] Connections;
        public Vector2 Position;

        public NodeDescription(NodeData data)
        {
            Index = data.Index;
            IsVariableData = data is VarNodeData;
            VariableName = VariableType = string.Empty;
            TaskTypeIndex = -1;
            TaskTypeString = TaskJson = string.Empty;

            if (IsVariableData)
            {
                var varData = data as VarNodeData;
                VariableName = varData.VariableName;
                VariableType = varData.VariableTypeName;
            }
            else if (data is TaskNodeData taskData)
            {
                var type = taskData.TaskImplementation?.GetType();
                TaskTypeIndex = TaskFactory.GetIndex(type);
                TaskTypeString = type?.FullName;
                TaskJson = JsonUtility.ToJson(taskData.TaskImplementation);
            }

            Connections = data.Connections.ToArray();
            Position = data.Position;
        }

        public void OffsetAllIndices(int offset)
        {
            Index += offset;

            if (Connections != null)
            {
                for (int i = 0; i < Connections.Length; i++)
                {
                    Connections[i] = new TaskConnection(Connections[i].OutPin, Connections[i].InNode + offset);
                }
            }
        }

        public NodeData GenerateData(bool useTaskTypeString)
        {
            NodeData data;

            if (!IsVariableData)
            {
                BehaviourTask task = null;

                if (useTaskTypeString)
                {
                    string typeName = TaskTypeString;
                    var type = TypeCache.GetTypesDerivedFrom<BehaviourTask>().FirstOrDefault(t => t.FullName == typeName);

                    if (type != null)
                    {
                        task = (BehaviourTask)JsonUtility.FromJson(TaskJson, type);
                    }
                }
                else
                {
                    task = TaskFactory.CreateTaskImplementationEditor(TaskTypeIndex);
                    if (task != null) JsonUtility.FromJsonOverwrite(TaskJson, task);
                }

                data = new TaskNodeData(Index, task, Connections);
            }
            else
            {
                data = new VarNodeData(Index, VariableName, VariableType, Connections);
            }

            data.Position = Position;
            return data;
        }
    }
}
