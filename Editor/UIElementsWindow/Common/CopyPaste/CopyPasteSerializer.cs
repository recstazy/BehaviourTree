using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal static class CopyPasteSerializer
    {
        private static Vector2 PasteOffset = new Vector2(50, -50);

        #region Structs

        [System.Serializable]
        private struct NodeDescription
        {
            public int Index;
            public int TaskTypeIndex;
            public string TaskJson;
            public TaskConnection[] Connections;
            public Vector2 Position;

            public NodeDescription(BTNode node)
            {
                Index = node.Data.Index;
                TaskTypeIndex = node.TaskTypeIndex;
                TaskJson = JsonUtility.ToJson(node.Data.TaskImplementation);
                Connections = node.Data.Connections.ToArray();
                Position = node.GetPosition().position;
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

            public NodeData GenerateData()
            {
                var task = TaskFactory.CreateTaskImplementationEditor(TaskTypeIndex);
                JsonUtility.FromJsonOverwrite(TaskJson, task);
                var data = new NodeData(Index, task, Connections);
                data.Position = Position + PasteOffset;
                return data;
            }
        }

        [System.Serializable]
        private struct SerializedStructure
        {
            public NodeDescription[] Nodes;

            public SerializedStructure(BTNode[] nodes)
            {
                Nodes = nodes.Select(n => new NodeDescription(n)).ToArray();
                var availableNodeIndices = new HashSet<int>(Nodes.Select(n => n.Index));

                for (int i = 0; i < Nodes.Length; i++)
                {
                    var node = Nodes[i];
                    if (node.Connections == null) continue;

                    node.Connections = node.Connections
                        .Where((c) => availableNodeIndices.Contains(c.InNode))
                        .ToArray();

                    Nodes[i] = node;
                }
            }

            public void OffsetAllIndices(int offset)
            {
                if (Nodes == null) return;

                for (int i = 0; i < Nodes.Length; i++)
                {
                    var desc = Nodes[i];
                    desc.OffsetAllIndices(offset);
                    Nodes[i] = desc;
                }
            }

            public NodeData[] GenerateData()
            {
                if (Nodes == null) return new NodeData[0];
                var data = Nodes.Select(n => n.GenerateData()).ToArray();
                return data;
            }
        }

        #endregion

        public static string Serialize(IEnumerable<GraphElement> elements)
        {
            var nodes = elements
                .OnlyNodes()
                .Where(e => !(e.IsEntryNode() || e.IsEntryOutputEdge()))
                .ToArray();

            var structure = new SerializedStructure(nodes);
            return JsonUtility.ToJson(structure);
        }

        public static NodeData[] Deserialize(string data, int startNodeIndex)
        {
            var structure = JsonUtility.FromJson<SerializedStructure>(data);
            if (structure.Nodes == null || structure.Nodes.Length == 0) return new NodeData[0];

            var maxNodeIndex = structure.Nodes.Min(n => n.Index);
            int offset = startNodeIndex - maxNodeIndex;
            structure.OffsetAllIndices(offset);
            return structure.GenerateData();
        }
    }
}
