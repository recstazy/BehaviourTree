using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal static class NodeCopier
    {
        #region Fields

        private static NodeData[] buffer;
        private static Vector2 bufferCenter;

        #endregion

        #region Properties
		
        public static int LastPasteStartIndex { get; private set; }

        #endregion

        public static void Copy(NodeData[] data)
        {
            if (data is null || data.Length == 0) return;

            buffer = data.Where(d => !(d.TaskImplementation is EntryTask)).Select(d => d.CreateCopy(true)).ToArray();
            Vector2 center = Vector2.zero;

            for (int i = 0; i < buffer.Length; i++)
            {
                center += buffer[i].Position;
            }

            center /= buffer.Length;
            bufferCenter = center;
            RemoveOutsideConnections(buffer);
        }

        public static NodeData[] GetCopiedNodes()
        {
            if (buffer is null) return null;

            var newData = new NodeData[buffer.Length];

            for (int i = 0; i < newData.Length; i++)
            {
                newData[i] = buffer[i].CreateCopy(true);
            }

            return newData;
        }

        public static NodeData[] GetCopiedNodes(int newStartIndex, Vector2 offset = default)
        {
            var nodes = GetCopiedNodes();
            if (nodes is null) return null;

            SetupForPastingToGraph(nodes, newStartIndex, offset);
            LastPasteStartIndex = newStartIndex;
            return nodes;
        }

        public static void SetupForPastingToGraph(this NodeData[] data, int newStartIndex, Vector2 offset = default)
        {
            if (data is null || data.Length == 0) return;

            int currentStartIndex = data.Min(d => d.Index);
            int deltaIndex = newStartIndex - currentStartIndex;
            var mousePos = BTEventProcessor.LastMousePosition;
            var deltaCenter = mousePos - bufferCenter;

            foreach (var d in data)
            {
                d.OverrideIndex(d.Index + deltaIndex);
                var newConnections = new TaskConnection[d.Connections.Length];

                for (int i = 0; i < newConnections.Length; i++)
                {
                    newConnections[i] = new TaskConnection(d.Connections[i].OutPin, d.Connections[i].InNode + deltaIndex);
                }

                d.SetConnections(newConnections);
                d.Position += deltaCenter + offset;
            }
        }

        private static void RemoveOutsideConnections(NodeData[] data)
        {
            int[] dataIndices = data.Select(d => d.Index).ToArray();

            foreach (var d in data)
            {
                List<TaskConnection> newConnections = new List<TaskConnection>();

                foreach (var c in d.Connections)
                {
                    if (dataIndices.Contains(c.InNode))
                    {
                        newConnections.Add(c);
                    }
                }

                d.SetConnections(newConnections.ToArray());
            }
        }
    }
}
