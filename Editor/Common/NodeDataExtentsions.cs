using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal static class NodeDataExtentsions
    {
        public static TaskOutAttribute[] GetOuts(this BehaviourTreeNode node)
        {
            return node?.Data?.GetOuts();
        }

        public static TaskOutAttribute[] GetOuts(this NodeData data)
        {
            if (data?.TaskImplementation is MultioutTask)
            {
                return GenerateOuts(data.Connections.Length, true);
            }

            return data?.TaskImplementation?.GetType()?.GetCustomAttributes(typeof(TaskOutAttribute), false) as TaskOutAttribute[];
        }

        private static TaskOutAttribute[] GenerateOuts(int count, bool generatePlusSign)
        {
            count = Mathf.Max(count, 0) + 1;
            var outs = new TaskOutAttribute[count];

            for (int i = 0; i < outs.Length; i++)
            {
                outs[i] = new TaskOutAttribute(i, generatePlusSign && i == outs.Length - 1 ? "+" : i.ToString());
            }

            return outs;
        }
    }

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
    }
}
