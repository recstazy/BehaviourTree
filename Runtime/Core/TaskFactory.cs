using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Recstazy.BehaviourTree
{
    internal class TaskFactory
    {
        #region Fields

        #endregion

        #region Properties

        public const string NoTaskLabel = "None";
        public static string[] Names { get; private set; }
        public static Type[] Types { get; private set; }
        public static string[] NamesEditor { get; private set; }
        public static Type[] TypesEditor { get; private set; }
        public static string[] PathsEditor { get; private set; }

        #endregion

        public static void UpdateTaskTypes()
        {
            FindTaskTypes();
            CreateTaskNames();
            CreateTaskPaths();
        }

        public static BehaviourTask CreateTaskImplementationEditor(int index)
        {
            if (index >= 0 && index < TypesEditor.Length && TypesEditor[index] != null)
            {
                return Activator.CreateInstance(TypesEditor[index]) as BehaviourTask;
            }

            return null;
        }

        public static BehaviourTask CreateTaskImplementationRuntime(int index)
        {
            if (index >= 0 && index < Types.Length)
            {
                return Activator.CreateInstance(Types[index]) as BehaviourTask;
            }

            return null;
        }

        public static GenericMenu CreateGenericMenu(BehaviourTask selectedTask, GenericMenu.MenuFunction2 onSelected)
        {
            var selectedType = selectedTask == null ? null : selectedTask.GetType();
            var menu = new GenericMenu();

            for (int i = 0; i < TypesEditor.Length; i++)
            {
                string path = string.IsNullOrEmpty(PathsEditor[i]) ? ObjectNames.NicifyVariableName(NamesEditor[i]) : PathsEditor[i];
                menu.AddItem(new GUIContent(path), Equals(TypesEditor[i], selectedType), onSelected, i);
            }

            return menu;
        }

        public static string GetName(Type type)
        {
            if (type == null) return NoTaskLabel;
            var index = Array.IndexOf(TypesEditor, type);
            if (index == -1) return NoTaskLabel;

            return NamesEditor[index];
        }

        public static int GetIndex(Type type)
        {
            if (type == null) return 0;
            return Array.IndexOf(TypesEditor, type);
        }

        private static void FindTaskTypes()
        {
            var types = TypeCache.GetTypesDerivedFrom<BehaviourTask>()
                .Where(t => !t.IsAbstract)
                .Where(t => t.GetCustomAttribute(typeof(ExcludeFromTaskSelectorAttribute)) == null)
                .OrderBy(t => t.Name);

            Types = types.ToArray();
            TypesEditor = new Type[] { null }.Concat(Types).ToArray();
        }

        private static void CreateTaskNames()
        {
            Names = Types.Select(t => t.Name).ToArray();
            NamesEditor = new string[] { NoTaskLabel }.Concat(Names).ToArray();
        }

        private static void CreateTaskPaths()
        {
            var paths = new string[Types.Length];

            for (int i = 0; i < paths.Length; i++)
            {
                var t = Types[i];
                var menuAttribute = t.GetCustomAttribute<TaskMenuAttribute>();
                paths[i] = menuAttribute == null ? string.Empty : menuAttribute.Path;
            }

            PathsEditor = new string[] { string.Empty }.Concat(paths).ToArray();
        }
    }
}
