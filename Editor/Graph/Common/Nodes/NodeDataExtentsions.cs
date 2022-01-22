using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal static class BTNodeExtensions
    {
        public static BTNode CreateGraphNode(this NodeData data)
        {
            if (data is TaskNodeData taskData)
            {
                return new TaskNode(taskData);
            }
            else if (data is VarNodeData varData)
            {
                return new VarNode(varData);
            }
            else return null;
        }

        public static TaskOutDescription[] GetOuts(this BTNode node)
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

        public static TaskOutDescription[] GetOuts(this NodeData data)
        {
            TaskOutDescription[] descriptions;

            if (data is TaskNodeData taskData)
            {
                var attributes = taskData.TaskImplementation?.GetType()?.GetCustomAttributes(typeof(TaskOutAttribute), false) as TaskOutAttribute[];
                if (attributes == null) attributes = new TaskOutAttribute[0];

                descriptions = new TaskOutDescription[attributes.Length];

                for (int i = 0; i < attributes.Length; i++)
                {
                    descriptions[i] = new TaskOutDescription(i, attributes[i].Name);
                }

                if (taskData.TaskImplementation is MultioutTask)
                {
                    int solidOutsCount = attributes.Length;
                    int reordableCount = taskData.Connections.Where(c => c.OutPin >= solidOutsCount).Count();
                    var generated = GenerateOuts(reordableCount, solidOutsCount);
                    descriptions = descriptions.Concat(generated).ToArray();
                }
            }
            else if (data is VarNodeData varData)
            {
                descriptions = new TaskOutDescription[1];
                descriptions[0] = new TaskOutDescription(0, varData.VariableName);
            }
            else descriptions = new TaskOutDescription[0];

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
