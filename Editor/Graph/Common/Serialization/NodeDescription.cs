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
        public bool IsFuncData;
        public string ImplTypeString;
        public string ImplJson;
        public int Index;
        public TaskConnection[] Connections;
        public Vector2 Position;

        public NodeDescription(NodeData data)
        {
            IsFuncData = data is FuncNodeData;
            Index = data.Index;
            var type = data.Implementation?.GetType();
            ImplTypeString = type?.FullName;
            ImplJson = JsonUtility.ToJson(data.Implementation);

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
                    Connections[i] = new TaskConnection(Connections[i].OutPin, Connections[i].InNode + offset, Connections[i].InName);
                }
            }
        }

        public NodeData GenerateData()
        {
            object implementation = null;
            string typeName = ImplTypeString;
            var type = TypeCache.GetTypesDerivedFrom<INodeImplementation>().FirstOrDefault(t => t.FullName == typeName);

            if (type != null)
            {
                implementation = JsonUtility.FromJson(ImplJson, type);
            }

            var data = CreateData(implementation);
            data.Position = Position;
            return data;
        }

        private NodeData CreateData(object impl)
        {
            NodeData data;

            if (!IsFuncData) data = new TaskNodeData(Index, impl as BehaviourTask, Connections);
            else data = new FuncNodeData(Index, impl as BehaviourFunc, Connections);
            return data;
        }
    }
}
