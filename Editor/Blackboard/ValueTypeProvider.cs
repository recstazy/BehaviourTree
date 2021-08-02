using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using System;
using System.Linq;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class ValueTypeProvider
    {
        #region Fields

        private const float _nameValueRatio = 0.3f;
        private int _currentEnumIndex = 0;

        #endregion

        #region Properties

        public static string[] FullNames { get; private set; }
        public static string[] Names { get; private set; }
        public static Type[] Types { get; private set; }

        public Type CurrentType { get; private set; }
        public string CurrentName { get; private set; }
        public bool Changed { get; private set; }

        #endregion

        [DidReloadScripts]
        private static void ScriptsReloaded()
        {
            UpdateValueTypes();
        }

        private static void UpdateValueTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<Type> types = new List<Type>();

            foreach (var a in assemblies)
            {
                types = types.Concat(a.GetTypes()
                .Where(t => t.GetInterface(typeof(ITypedValue).FullName) != null)
                .Where(t => !t.IsSubclassOf(typeof(UnityEngine.Object)))
                .Where(t => !t.ContainsGenericParameters));
            }

            var newTypes = types.ToArray();
            Names = new string[] { "None" }.Concat(newTypes.Select(t => RemoveValueCaptionFromEnd(t.Name))).ToArray();
            FullNames = new string[] { "None" }.Concat(newTypes.Select(t => GetShortAssemblyName(t.Assembly.FullName) + " " + t.FullName)).ToArray();
            Types = new Type[] { null }.Concat(newTypes).ToArray();
        }

        private static string GetShortAssemblyName(string assemblyName)
        {
            var versionIndex = assemblyName.IndexOf(", Version");
            return assemblyName.Substring(0, versionIndex);
        }

        private static string RemoveValueCaptionFromEnd(string name)
        {
            var indexOfValue = name.LastIndexOf("Value");

            if (indexOfValue > 0 && indexOfValue == name.Length - 5)
            {
                return name.Replace("Value", "");
            }

            return name;
        }

        public ValueTypeProvider(string valueTypeName)
        {
            int index = Array.IndexOf(FullNames, valueTypeName);
            if (index < 0) index = 0;

            CurrentType = Types[index];
            CurrentName = Names[index];
            _currentEnumIndex = index;
        }

        public void OnGUI(Rect rect, string label = "")
        {
            Changed = false;

            var currentRect = rect;
            currentRect.width *= _nameValueRatio;
            EditorGUI.LabelField(currentRect, string.IsNullOrEmpty(label) ? "Type" : label);

            currentRect.x += currentRect.width;
            currentRect.width = rect.width - currentRect.width;
            int index = _currentEnumIndex;
            index = EditorGUI.Popup(currentRect, index, Names);

            if (index != _currentEnumIndex)
            {
                CurrentName = Names[index];
                CurrentType = Types[index];
                _currentEnumIndex = index;
                Changed = true;
            }
        }
    }
}
