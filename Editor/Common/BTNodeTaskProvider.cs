using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTNodeTaskProvider
    {
        #region Fields

        private int currentIndex;
        private GUIStyle dropDownStyle;

        #endregion

        #region Properties

        public BehaviourTask CurrentImplementation { get; private set; }

        #endregion

        public BTNodeTaskProvider(NodeData data)
        {
            var index = GetIndexOfTask(data.TaskImplementation);
            index = Mathf.Clamp(index, 0, TaskFactory.TypesEditor.Length - 1);
            currentIndex = index;
            CurrentImplementation = data.TaskImplementation;
        }

        public void OnGUI(Rect rect)
        {
            if (dropDownStyle == null) CreateStyle();

            int newIndex = currentIndex;
            newIndex = EditorGUI.Popup(rect, newIndex, TaskFactory.NamesEditor, dropDownStyle);

            if (newIndex != currentIndex)
            {
                ChangeTask(newIndex);
            }
        }

        private void CreateStyle()
        {
            dropDownStyle = new GUIStyle("ShurikenDropdown");
            dropDownStyle.padding = new RectOffset(15, 0, 0, 0);
            dropDownStyle.normal.textColor = Color.gray;
        }

        private void ChangeTask(int newIndex)
        {
            currentIndex = newIndex;
            CurrentImplementation = TaskFactory.CreateTaskImplementationEditor(newIndex);
        }

        private int GetIndexOfTask(BehaviourTask taskImpl)
        {
            if (taskImpl is null) return 0;
            return System.Array.IndexOf(TaskFactory.NamesEditor, taskImpl.GetType().Name);
        }
    }
}
