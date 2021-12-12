using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UI;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class NodeTaskProvider : VisualElement
    {
        public event Action OnTaskChanged;

        #region Fields

        private int _currentTaskIndex;
        private Label _label;

        #endregion

        #region Properties

        public Type CurrentType { get; private set; }
        public int CurrentIndex => _currentTaskIndex;

        #endregion

        public NodeTaskProvider() { }

        public NodeTaskProvider(NodeData data)
        {
            CurrentType = data?.TaskImplementation?.GetType();
            _currentTaskIndex = TaskFactory.GetIndex(CurrentType);

            var manipulator = new ContextualMenuManipulator(CreateMenu);
            manipulator.activators.Add(new ManipulatorActivationFilter() { button = MouseButton.LeftMouse, clickCount = 1 });

            _label = new Label();
            _label.text = TaskFactory.NamesEditor[_currentTaskIndex];
            _label.style.fontSize = 13;
            _label.AddToClassList("node-task-button");
            _label.AddManipulator(manipulator);
            Add(_label);
        }

        private void CreateMenu(ContextualMenuPopulateEvent evt)
        {
            evt.StopImmediatePropagation();

            for (int i = 0; i < TaskFactory.TypesEditor.Length; i++)
            {
                var path = TaskFactory.PathsEditor[i];
                path = path.Contains('/') ? path : TaskFactory.NamesEditor[i];
                evt.menu.AppendAction(path, TaskSelected, StatusCallback, i);
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
            _label.text = TaskFactory.NamesEditor[_currentTaskIndex];
            OnTaskChanged?.Invoke();
        }

        private DropdownMenuAction.Status StatusCallback(DropdownMenuAction action)
        {
            return _currentTaskIndex == (int)action.userData ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
        }
    }
}
