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

        public static OutputDescription[] GetOuts(this BTNode node)
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

        public static void ValidateOutputs(this TreeNodeData treeData)
        {
            var enumerator = treeData.GetSumDataEnumerator();

            while (enumerator.MoveNext())
            {
                enumerator.Current.ValideteOutputs(treeData);
            }
        }

        public static bool ValideteOutputs(this NodeData nodeData, TreeNodeData treeData)
        {
            var enumerator = treeData.GetSumDataEnumerator();
            var connectionsToRemove = new List<int>();

            for (int i = 0; i < nodeData.Connections.Length; i++)
            {
                var connection = nodeData.Connections[i];
                if (connection.InName == InputDescription.ExecutionInName) continue;

                NodeData inNode = null;
                enumerator.Reset();

                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Index == connection.InNode)
                    {
                        inNode = enumerator.Current;
                        break;
                    }
                }

                if (inNode == null)
                {
                    connectionsToRemove.Add(i);
                }
                else
                {
                    var inputs = inNode.GetInputs();
                    var connectedInput = inputs.FirstOrDefault(input => input.IdName == connection.InName);

                    if (connectedInput.IsValid)
                    {
                        if (connectedInput.PortType.FullName != connection.OutTypeName)
                        {
                            connectionsToRemove.Add(i);
                        }
                    }
                    else connectionsToRemove.Add(i);
                }
            }

            bool changed = connectionsToRemove.Count > 0;

            if (changed)
            {
                nodeData.RemoveConnectionsWithIndices(connectionsToRemove.ToArray());
            }

            return changed;
        }

        public static OutputDescription[] GetOuts(this NodeData data)
        {
            OutputDescription[] descriptions;

            if (data is TaskNodeData taskData)
            {
                var attributes = taskData.TaskImplementation?.GetType()?.GetCustomAttributes(typeof(TaskOutAttribute), false) as TaskOutAttribute[];
                if (attributes == null) attributes = new TaskOutAttribute[0];

                descriptions = new OutputDescription[attributes.Length];

                for (int i = 0; i < attributes.Length; i++)
                {
                    descriptions[i] = new OutputDescription(i, attributes[i].Name, typeof(ExecutionPin));
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
                descriptions = new OutputDescription[1];
                descriptions[0] = new OutputDescription(0, varData.VariableName, varData.VariableType);
            }
            else descriptions = new OutputDescription[0];

            return descriptions;
        }

        private static OutputDescription[] GenerateOuts(int count, int startIndex)
        {
            count = Mathf.Max(count, 0) + 1;
            var outs = new OutputDescription[count];

            for (int i = 0; i < outs.Length; i++)
            {
                outs[i] = new OutputDescription(startIndex + i, i == outs.Length - 1 ? "+" : i.ToString(), typeof(ExecutionPin));
            }

            return outs;
        }
    }
}
