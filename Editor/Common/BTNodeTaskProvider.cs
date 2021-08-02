using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTNodeTaskProvider
    {
        #region Fields

        private int _currentIndex;
        private GUIStyle _dropDownStyle;

        #endregion

        #region Properties

        public BehaviourTask CurrentImplementation { get; private set; }

        #endregion

        public BTNodeTaskProvider(NodeData data)
        {
            var index = GetIndexOfTask(data.TaskImplementation);
            index = Mathf.Clamp(index, 0, TaskFactory.TypesEditor.Length - 1);
            _currentIndex = index;
            CurrentImplementation = data.TaskImplementation;
        }

        public void OnGUI(Rect rect)
        {
            if (_dropDownStyle == null) CreateStyle();

            var name = TaskFactory.NamesEditor[_currentIndex];

            if (GUI.Button(rect, name, _dropDownStyle))
            {
                var menu = TaskFactory.CreateGenericMenu(CurrentImplementation, TaskInMenuSelected);
                menu.ShowAsContext();
            }
        }

        private void CreateStyle()
        {
            _dropDownStyle = new GUIStyle("ShurikenDropdown");
            _dropDownStyle.padding = new RectOffset(15, 0, 0, 0);
            _dropDownStyle.normal.textColor = Color.gray;
        }

        private void TaskInMenuSelected(object index)
        {
            int newIndex = (int)index;

            if (newIndex != _currentIndex)
            {
                ChangeTask(newIndex);
            }
        }

        private void ChangeTask(int newIndex)
        {
            _currentIndex = newIndex;
            CurrentImplementation = TaskFactory.CreateTaskImplementationEditor(newIndex);
        }

        private int GetIndexOfTask(BehaviourTask taskImpl)
        {
            if (taskImpl is null) return 0;
            return System.Array.IndexOf(TaskFactory.NamesEditor, taskImpl.GetType().Name);
        }
    }
}
