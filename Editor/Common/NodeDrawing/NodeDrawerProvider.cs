using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class NodeDrawerProvider
    {
        #region Fields

        private static Dictionary<Type, Type> taskTypeToDrawer = new Dictionary<Type, Type>();

        #endregion

        #region Properties
		
        #endregion

        public static void UpdateTaskDrawers()
        {
            taskTypeToDrawer.Clear();

            var nodeDrawerTypes = Assembly.GetAssembly(typeof(BehaviourTreeNode)).GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BehaviourTreeNode)))
                .Where(t => t.GetCustomAttributes(typeof(CustomNodeDrawerAttribute), false).Count() > 0)
                .ToArray();
                
            foreach (var drawerType in nodeDrawerTypes)
            {
                var attributes = drawerType.GetCustomAttributes(typeof(CustomNodeDrawerAttribute)) as CustomNodeDrawerAttribute[];

                foreach (var a in attributes)
                {
                    if (a.TaskType != null && !taskTypeToDrawer.ContainsKey(a.TaskType))
                    {
                        taskTypeToDrawer.Add(a.TaskType, drawerType);
                    }
                }
            }
        }

        public static BehaviourTreeNode GetDrawerForData(NodeData data)
        {
            if (data?.TaskImplementation != null)
            {
                if (taskTypeToDrawer.TryGetValue(data.TaskImplementation.GetType(), out var drawerType))
                {
                    return Activator.CreateInstance(drawerType, data) as BehaviourTreeNode;
                }
            }

            return new BehaviourTreeNode(data);
        }
    }
}
