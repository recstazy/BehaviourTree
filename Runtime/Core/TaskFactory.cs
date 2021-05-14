using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

namespace Recstazy.BehaviourTree
{
    internal class TaskFactory
    {
        #region Fields

        private static Type[] taskTypes;
        private static string[] names;
        private static Type[] taskTypesWithEmpty;
        private static string[] namesWithEmpty;

        #endregion

        #region Properties

        public static string[] Names => names;
        public static Type[] Types => taskTypes;
        public static string[] NamesEditor => namesWithEmpty;
        public static Type[] TypesEditor => taskTypesWithEmpty;

        #endregion

        public static void UpdateTaskTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<Type> types = new List<Type>();

            foreach (var a in assemblies)
            {
                types = types.Concat(a.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BehaviourTask)))
                .Where(t => t.GetCustomAttribute(typeof(ExcludeFromTaskSelectorAttribute)) is null));
            }

            taskTypes = types.ToArray();
            names = taskTypes.Select(t => t.Name).ToArray();
            taskTypesWithEmpty = new Type[] { null }.Concat(taskTypes).ToArray();
            namesWithEmpty = new string[] { "Empty" }.Concat(names).ToArray();
        }

        public static BehaviourTask CreateTaskImplementationEditor(int index)
        {
            if (index >= 0 && index < taskTypesWithEmpty.Length && taskTypesWithEmpty[index] != null)
            {
                return Activator.CreateInstance(taskTypesWithEmpty[index]) as BehaviourTask;
            }

            return null;
        }

        public static BehaviourTask CreateTaskImplementationRuntime(int index)
        {
            if (index >= 0 && index < taskTypes.Length)
            {
                return Activator.CreateInstance(taskTypes[index]) as BehaviourTask;
            }

            return null;
        }
    }
}
