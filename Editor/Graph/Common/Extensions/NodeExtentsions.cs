using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal static class NodeExtensions
    {
        public static BTNode CreateGraphNode(this NodeData data)
        {
            if (data is TaskNodeData taskData)
            {
                return new TaskNode(taskData);
            }
            else if (data is FuncNodeData varData)
            {
                return new VarNode(varData);
            }
            else return null;
        }

        public static OutputDescription GetOutDescription(this Port outPort)
        {
            return (OutputDescription)outPort.userData;
        }

        public static InputDescription GetInputDescription(this Port inPort)
        {
            return (InputDescription)inPort.userData;
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
    }
}
