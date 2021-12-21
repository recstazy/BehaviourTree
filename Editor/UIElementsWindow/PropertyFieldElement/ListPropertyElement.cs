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
        public event Action<IList> OnChanged;

        #region Fields

        private VisualElement _itemsContainer;
        private SerializedProperty _listProperty;
        private List<ListElement> _items;

        private Button _plusButton;
        private Button _minusButton;

        private List<PropertyFieldElement> _fieldElements;

        #endregion

        #region Properties

        public IList List { get; private set; }

        #endregion

        public ListPropertyElement()
        {
            var layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(System.IO.Path.Combine(MainPaths.UxmlRoot, "ListViewLayout.uxml"));
            VisualElement layoutInstance = layout.Instantiate();
            Add(layoutInstance);
            _itemsContainer = this.Q(className: "list-items-container");

            _plusButton = this.Q(name: "plusButton") as Button;
            _minusButton = this.Q(name: "minusButton") as Button;

            _plusButton.clicked += PlusClicked;
            _minusButton.clicked += MinusClicked;

            RegisterCallback<DetachFromPanelEvent>(Removed);
        }

        public void SetList(IList list, SerializedProperty listProperty)
        {
            List = list;
            _listProperty = listProperty;
            CreateItems();
        }

        private void CreateItems()
        {
            _items = new List<ListElement>();
            _fieldElements = new List<PropertyFieldElement>();

            for (int i = 0; i < List.Count; i++)
            {
                CreateItem(i);
            }
        }

        private void CreateItem(int index)
        {
            var listElement = new ListElement();
            listElement.AddToClassList("list-item");
            _itemsContainer.Add(listElement);

            var property = _listProperty.GetArrayElementAtIndex(index);
            var field = new PropertyFieldElement();
            field.OnValueChanged += AnyFieldChanged;
            _fieldElements.Add(field);
            field.SetField(property, () => GetItemValue(index), (value) => SetItemValue(index, value));
            field.Label = $"{index}:";
            listElement.Add(field);
            _items.Add(listElement);
        }

        private void RemoveItem(int index)
        {
            var element = _items[index];
            _itemsContainer.Remove(element);
            _items.RemoveAt(index);
            _fieldElements[index].OnValueChanged -= AnyFieldChanged;
            _fieldElements.RemoveAt(index);
        }

        private void SetItemValue(int index, object value)
        {
            List[index] = value;
        }

        private object GetItemValue(int index)
        {
            return List[index];
        }

        private void PlusClicked()
        {
            List = IncrementListSize(List);
            _listProperty.InsertArrayElementAtIndex(List.Count - 1);
            
            OnChanged?.Invoke(List);
            CreateItem(List.Count - 1);
        }

        private void MinusClicked()
        {
            if (List.Count == 0) return;

            _listProperty.DeleteArrayElementAtIndex(List.Count - 1);
            List = DecrementListSize(List);

            OnChanged?.Invoke(List);
            RemoveItem(List.Count);
        }

        private void AnyFieldChanged(object newValue)
        {
            OnChanged?.Invoke(List);
        }

        private void Removed(DetachFromPanelEvent evt)
        {
            UnregisterCallback<DetachFromPanelEvent>(Removed);
            _plusButton.clicked -= PlusClicked;
            _minusButton.clicked -= MinusClicked;

            if (_fieldElements != null)
            {
                foreach (var f in _fieldElements)
                {
                    f.OnValueChanged -= AnyFieldChanged;
                }
            }
        }

        private IList IncrementListSize(IList list)
        {
            bool isArray = list is Array;

            if (isArray)
            {
                var elementType = list.GetType().GetElementType();
                var newArray = Array.CreateInstance(elementType, list.Count + 1);
                list.CopyTo(newArray, 0);
                var serializedLastElement = list.Count > 0 ? JsonUtility.ToJson(list[list.Count - 1]) : "{}";
                var deserializedElement = JsonUtility.FromJson(serializedLastElement, elementType);
                newArray.SetValue(deserializedElement, newArray.Length - 1);
                return newArray;
            }
            else
            {
                list.Add(default);
                return list;
            }
        }

        private IList DecrementListSize(IList list)
        {
            bool isArray = list is Array;

            if (isArray)
            {
                var newArray = Array.CreateInstance(list.GetType().GetElementType(), list.Count - 1);

                for (int i = 0; i < newArray.Length; i++)
                {
                    newArray.SetValue(list[i], i);
                }

                return newArray;
            }
            else
            {
                list.RemoveAt(list.Count - 1);
                return list;
            }
        }
    }
}
