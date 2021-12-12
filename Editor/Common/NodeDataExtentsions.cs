using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            if (data?.TaskImplementation is MultioutTask multioutTask)
            {
                return GenerateOuts(multioutTask.OutsCount, true);
            }

            return data?.TaskImplementation?.GetType()?.GetCustomAttributes(typeof(TaskOutAttribute), false) as TaskOutAttribute[];
        }

        private static TaskOutAttribute[] GenerateOuts(int count, bool generatePlusSign)
        {
            count = Mathf.Max(count, 0) + 1;
            var outs = new TaskOutAttribute[count];

            for (int i = 0; i < outs.Length; i++)
            {
                outs[i] = new TaskOutAttribute(i, generatePlusSign && i == outs.Length - 1 ? string.Empty : i.ToString());
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
    }
}
