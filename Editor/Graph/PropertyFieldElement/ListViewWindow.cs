using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class ListViewWindow : EditorWindow
    {
        public event System.Action OnClosed;
        public event System.Action OnChanged;

        #region Fields

        private static ListViewWindow s_currentInstance;
        private static bool s_isShowing;
        private Vector2 _scrollPos;

        private Object _serializedTargetObject;
        private string _propertyPath;
        private SerializedObject _sObject;
        private SerializedProperty _property;
        private bool _disposed;

        #endregion

        #region Properties

        public int ArraySize { get; private set; }

        #endregion

        public static ListViewWindow Show(Object targetObject, string propertyPath, string displayName)
        {
            var window = new ListViewWindow();
            window._serializedTargetObject = targetObject;
            window._propertyPath = propertyPath;
            window.titleContent = new GUIContent(displayName);
            window.ShowUtility();
            window.Focus();
            return window;
        }

        public static void CloseIfAny()
        {
            while (!HasOpenInstances<ListViewWindow>())
            {
                GetWindow<ListViewWindow>().Close();
            }

            s_currentInstance = null;
        }

        private void OnGUI()
        {
            

            if (_sObject == null || _property == null)
            {
                FieldUtility.CreateSerializedObjectAndProperty(_serializedTargetObject, _propertyPath, out _sObject, out _property);
                if (_sObject == null || _property == null) return;

                _disposed = false;
                ArraySize = _property.arraySize;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUI.BeginChangeCheck();
            _property.isExpanded = true;
            EditorGUILayout.PropertyField(_property);
            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                ArraySize = _property.arraySize;
                _property.serializedObject.ApplyModifiedProperties();
                _property.Dispose();
                _sObject.Dispose();
                _property = null;
                _sObject = null;
                _disposed = true;
                OnChanged?.Invoke();
            }
        }

        private void OnEnable()
        {
            CloseIfAny();
            s_currentInstance = this;
            s_isShowing = true;
        }

        private void OnDisable()
        {
            s_currentInstance = null;
            s_isShowing = false;

            if (!_disposed)
            {
                _sObject.Dispose();
                _property.Dispose();
            }

            OnClosed?.Invoke();
        }
    }
}
