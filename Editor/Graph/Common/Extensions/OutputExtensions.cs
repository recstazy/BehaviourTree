using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal static class OutpuExtensions
    {
        public static void ValidateOutputs(this TreeNodeData treeData)
        {
            foreach (var data in treeData)
            {
                data.ValideteOutputs(treeData);
            }
        }

        public static bool ValideteOutputs(this NodeData nodeData, TreeNodeData treeData)
        {
            var enumerator = treeData.GetEnumerator();
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
            else if (data is FuncNodeData funcData)
            {
                if (funcData.FuncImplementation != null)
                {
                    var outs = funcData.FuncImplementation.GetOuts();
                    descriptions = new OutputDescription[outs.Length];

                    for (int i = 0; i < outs.Length; i++)
                    {
                        descriptions[i] = new OutputDescription(i, outs[i].ValueName, outs[i].ValueType);
                    }
                }
                else descriptions = new OutputDescription[0];
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
