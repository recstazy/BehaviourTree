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
        #region Fields

        private string _currentMenuName;

        #endregion

        #region Properties

        public Type CurrentType { get; private set; }

        #endregion

        public NodeTaskProvider() { }

        public NodeTaskProvider(NodeData data)
        {
            CurrentType = data?.TaskImplementation?.GetType();
            var manipulator = new ContextualMenuManipulator(CreateMenu);

            var button = new Button();
            _currentMenuName = TaskFactory.GetName(CurrentType);
            button.text = _currentMenuName;
            button.AddManipulator(manipulator);
            Add(button);
        }

        private void CreateMenu(ContextualMenuPopulateEvent evt)
        {
            for (int i = 0; i < TaskFactory.TypesEditor.Length; i++)
            {
                evt.menu.AppendAction(TaskFactory.NamesEditor[i], TaskSelected, StatusCallback, TaskFactory.TypesEditor[i]);
            }
        }

        private void TaskSelected(DropdownMenuAction action)
        {
            Debug.Log($"Selected {action.userData as Type}");
        }

        private DropdownMenuAction.Status StatusCallback(DropdownMenuAction action)
        {
            return _currentMenuName == action.name ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
        }
    }
}
