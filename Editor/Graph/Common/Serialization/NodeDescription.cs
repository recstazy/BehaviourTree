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
        public int Index;
        public int TaskTypeIndex;
        public string TaskTypeString;
        public string TaskJson;
        public TaskConnection[] Connections;
        public Vector2 Position;

        public NodeDescription(NodeData data)
        {
            Index = data.Index;
            var type = data.TaskImplementation?.GetType();
            TaskTypeIndex = TaskFactory.GetIndex(type);
            TaskTypeString = type?.FullName;
            TaskJson = JsonUtility.ToJson(data.TaskImplementation);
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

        public NodeData GenerateData(bool useTypeString)
        {
            BehaviourTask task = null;

            if (useTypeString)
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
            
            var data = new NodeData(Index, task, Connections);
            data.Position = Position;
            return data;
        }
    }
}
