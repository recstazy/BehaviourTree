using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Reflection;
using System;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class ListPropertyElement : VisualElement
    {
        #region Fields

        private VisualElement _itemsContainer;
        private SerializedProperty _listProperty;
        private IList _list;

        #endregion

        #region Properties
	
        #endregion

        public ListPropertyElement()
        {
            var layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(System.IO.Path.Combine(MainPaths.UxmlRoot, "ListViewLayout.uxml"));
            VisualElement layoutInstance = layout.Instantiate();
            Add(layoutInstance);
            _itemsContainer = this.Q(className: "list-items-container");
        }

        public void SetList(IList list, SerializedProperty listProperty)
        {
            _list = list;
            _listProperty = listProperty;
            CreateItems();
        }

        private void CreateItems()
        {
            for (int i = 0; i < _list.Count; i++)
            {
                var index = i;
                var listElement = new ListElement();
                _itemsContainer.Add(listElement);

                var property = _listProperty.GetArrayElementAtIndex(i);
                var field = new PropertyFieldElement();
                field.SetField(property, () => GetItemValue(index), (value) => SetItemValue(index, value));
                field.Label = $"{index}:";
                listElement.Add(field);
            }
        }

        private void SetItemValue(int index, object value)
        {
            _list[index] = value;
        }

        private object GetItemValue(int index)
        {
            return _list[index];
        }
    }
}
