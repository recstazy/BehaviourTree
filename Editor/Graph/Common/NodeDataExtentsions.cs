using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal static class BTNodeExtensions
    {
        public static TaskOutDescription[] GetOuts(this TaskNode node)
        {
            return node?.Data?.GetOuts();
        }

        public static bool IsEntryNode(this GraphElement element)
        {
            return element is TaskNode btNode && btNode.IsEntry;
        }

        public static bool IsEntryOutputEdge(this GraphElement element)
        {
            return element is Edge edge && edge.output != null && edge.output.node.IsEntryNode();
        }

        public static IEnumerable<TaskNode> OnlyNodes(this IEnumerable<GraphElement> elements)
        {
            return elements.Where(e => e is TaskNode).Select(e => (TaskNode)e);
        }

        public static TaskOutDescription[] GetOuts(this NodeData data)
        {
            var attributes = data?.TaskImplementation?.GetType()?.GetCustomAttributes(typeof(TaskOutAttribute), false) as TaskOutAttribute[];
            if (attributes == null) attributes = new TaskOutAttribute[0];

            var descriptions = new TaskOutDescription[attributes.Length];

            for (int i = 0; i < attributes.Length; i++)
            {
                descriptions[i] = new TaskOutDescription(i, attributes[i].Name);
            }

            if (data?.TaskImplementation is MultioutTask)
            {
                int solidOutsCount = attributes.Length;
                int reordableCount = data.Connections.Where(c => c.OutPin >= solidOutsCount).Count();
                var generated = GenerateOuts(reordableCount, solidOutsCount);
                descriptions = descriptions.Concat(generated).ToArray();
            }

            return descriptions;
        }

        private static TaskOutDescription[] GenerateOuts(int count, int startIndex)
        {
            count = Mathf.Max(count, 0) + 1;
            var outs = new TaskOutDescription[count];

            for (int i = 0; i < outs.Length; i++)
            {
                outs[i] = new TaskOutDescription(startIndex + i, i == outs.Length - 1 ? "+" : i.ToString());
            }

            return outs;
        }
    }
}
