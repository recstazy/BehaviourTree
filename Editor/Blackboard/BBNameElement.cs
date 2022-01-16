using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomPropertyElement(typeof(BlackboardName))]
    public class BBNameElement : BasePropertyFieldElement
    {
        #region Fields

        private Label _currentNameLabel;
        private Blackboard _currentBB;
        private string[] _availableNames;
        private string _currentName;
        private int _currentNameIndex;

        #endregion

        #region Properties

        #endregion

        protected override void CreateVisualElements(SerializedProperty property)
        {
            _currentNameLabel = new Label("None");
            _currentNameLabel.AddToClassList("bb-name-dropdown");
            FieldsContainer.Add(_currentNameLabel);

            UpdateNames(property);
            var currentName = property.FindPropertyRelative("_name").stringValue;
            SetNewName(currentName);

            var manipulator = new ContextualMenuManipulator(PopulateContextMenu);
            _currentNameLabel.AddManipulator(manipulator);
        }

        private void PopulateContextMenu(ContextualMenuPopulateEvent evt)
        {
            evt.StopImmediatePropagation();

            foreach (var n in _availableNames)
            {
                evt.menu.AppendAction(ObjectNames.NicifyVariableName(n), NameSelected, (evt) => DropdownMenuAction.Status.Normal, n);
            }
        }

        private void NameSelected(DropdownMenuAction evt)
        {
            SetNewName((string)evt.userData);
            CallChanged();
        }

        private void UpdateNames(SerializedProperty property)
        {
            if (_serializedTargetObject is IBlackboardProvider bbProvider)
            {
                _currentBB = bbProvider.Blackboard;
            }

            var valueTypings = FieldInfo.GetCustomAttributes(typeof(ValueTypeAttribute), true) as ValueTypeAttribute[];
            bool hasTyping = valueTypings != null && valueTypings.Length > 0;
            BlackboardProperty propType = property.type.Contains("Setter") ? BlackboardProperty.Setter : BlackboardProperty.Getter;
            string[] names = hasTyping ? _currentBB.GetNamesTyped(propType, valueTypings[0].CompatableTypes) : _currentBB.GetNames(propType);
            _availableNames = new string[]{"None"}.Concat(names).ToArray();
        }

        private void SetNewName(string newName)
        {
            _currentNameIndex = GetNameIndexByName(newName);
            _currentName = _availableNames[_currentNameIndex];
            _currentNameLabel.text = _currentName;
            SetRelativeValue("_name", _currentName);
        }

        private int GetNameIndexByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return 0;
            return Mathf.Max(System.Array.IndexOf(_availableNames, name), 0);
        }
    }
}
