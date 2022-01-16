using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal static class BTNodeExtensions
    {
        public static TaskOutAttribute[] GetOuts(this BTNode node)
        {
            return node?.Data?.GetOuts();
        }

        public static bool IsEntryNode(this GraphElement element)
        {
            return element is BTNode btNode && btNode.IsEntry;
        }

        public static bool IsEntryOutputEdge(this GraphElement element)
        {
            return element is Edge edge && edge.output != null && edge.output.node.IsEntryNode();
        }

        public static IEnumerable<BTNode> OnlyNodes(this IEnumerable<GraphElement> elements)
        {
            return elements.Where(e => e is BTNode).Select(e => (BTNode)e);
        }

        public static TaskOutAttribute[] GetOuts(this NodeData data)
        {
            var attributes = data?.TaskImplementation?.GetType()?.GetCustomAttributes(typeof(TaskOutAttribute), false) as TaskOutAttribute[];

            if (data?.TaskImplementation is MultioutTask)
            {
                int solidOutsCount = attributes.Length;
                attributes = attributes.Concat(GenerateOuts(data.Connections.Length - solidOutsCount, solidOutsCount, true)).ToArray();
            }

            return attributes;
        }

        private static TaskOutAttribute[] GenerateOuts(int count, int startIndex, bool generatePlusSign)
        {
            count = Mathf.Max(count, 0) + 1;
            var outs = new TaskOutAttribute[count];

            for (int i = 0; i < outs.Length; i++)
            {
                outs[i] = new TaskOutAttribute(startIndex + i, generatePlusSign && i == outs.Length - 1 ? "+" : i.ToString());
            }

            return outs;
        }
    }
}
