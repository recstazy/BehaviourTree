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
            layoutInstance.StretchToParentSize();
            Add(new Label("bbbbbbb"));
        }

        public void SetList(IList list, SerializedProperty listProperty)
        {
            _list = list;
            _listProperty = listProperty;
        }


    }
}
