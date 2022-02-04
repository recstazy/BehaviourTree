using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace Recstazy.BehaviourTree
{
    internal static class NodeDataExtensions
    {
        public static InputValueBase GetGetter(this NodeData data, string name)
        {
            if (data is TaskNodeData taskData)
            {
                if (taskData.TaskImplementation != null)
                {
                    var taskType = taskData.TaskImplementation.GetType();
                    var field = taskType.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (field != null)
                    {
                        return field.GetValue(taskData.TaskImplementation) as InputValueBase;
                    }
                }
            }

            return null;
        }
    }
}
