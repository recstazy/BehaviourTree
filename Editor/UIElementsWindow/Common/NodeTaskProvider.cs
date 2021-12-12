using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class NodeTaskProvider : VisualElement
    {
        public event Action OnTaskChanged;

        #region Fields

        private int _currentTaskIndex;
        private Button _button;

        #endregion

        #region Properties

        public Type CurrentType { get; private set; }
        public int CurrentIndex => _currentTaskIndex;

        #endregion

        public NodeTaskProvider() { }

        public NodeTaskProvider(NodeData data)
        {
            CurrentType = data?.TaskImplementation?.GetType();
            var manipulator = new ContextualMenuManipulator(CreateMenu);

            _button = new Button();
            _currentTaskIndex = TaskFactory.GetIndex(CurrentType);
            _button.text = TaskFactory.NamesEditor[_currentTaskIndex];
            _button.AddManipulator(manipulator);
            Add(_button);
        }

        private void CreateMenu(ContextualMenuPopulateEvent evt)
        {
            for (int i = 0; i < TaskFactory.TypesEditor.Length; i++)
            {
                evt.menu.AppendAction(TaskFactory.NamesEditor[i], TaskSelected, StatusCallback, i);
            }
        }

        private void TaskSelected(DropdownMenuAction action)
        {
            var selectedIndex = (int)action.userData;

            if (selectedIndex != _currentTaskIndex)
            {
                SetTaskTypeByIndex(selectedIndex);
            }
        }

        private void SetTaskTypeByIndex(int taskIndex)
        {
            _currentTaskIndex = taskIndex;
            CurrentType = TaskFactory.TypesEditor[taskIndex];
            _button.text = TaskFactory.NamesEditor[_currentTaskIndex];
            OnTaskChanged?.Invoke();
        }

        private DropdownMenuAction.Status StatusCallback(DropdownMenuAction action)
        {
            return _currentTaskIndex == (int)action.userData ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
        }
    }
}
