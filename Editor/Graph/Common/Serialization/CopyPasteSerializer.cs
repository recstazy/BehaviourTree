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
        private struct SerializedStructure
        {
            public Vector2 AvgPosition;
            public NodeDescription[] Nodes;

            public SerializedStructure(BTNode[] nodes)
            {
                Nodes = nodes.Select(n => new NodeDescription(n.Data)).ToArray();
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

                AvgPosition = Nodes.GetAvgPosition();
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

        public static NodeData[] Deserialize(string data, int startNodeIndex, Vector2 mousePosition)
        {
            var structure = JsonUtility.FromJson<SerializedStructure>(data);
            if (structure.Nodes == null || structure.Nodes.Length == 0) return new NodeData[0];

            var maxNodeIndex = structure.Nodes.Min(n => n.Index);
            int offset = startNodeIndex - maxNodeIndex;
            structure.OffsetAllIndices(offset);
            structure.Nodes.SetAvgPosition(mousePosition);
            return structure.GenerateData();
        }

        private static Vector2 GetAvgPosition(this NodeDescription[] nodes)
        {
            Vector2 avgPos = Vector2.zero;

            for (int i = 0; i < nodes.Length; i++)
            {
                avgPos += nodes[i].Position;
            }

            return avgPos / nodes.Length;
        }

        private static void SetAvgPosition(this NodeDescription[] nodes, Vector2 avgPosition)
        {
            var currentAvgPosition = GetAvgPosition(nodes);

            for (int i = 0; i < nodes.Length; i++)
            {
                var avgOffset = nodes[i].Position - currentAvgPosition;
                nodes[i].Position = avgPosition + avgOffset;
            }
        }
    }
}
