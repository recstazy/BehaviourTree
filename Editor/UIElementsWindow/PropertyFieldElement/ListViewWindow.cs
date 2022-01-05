using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

internal class ListViewWindow : EditorWindow
{
    public event System.Action OnClosed;
    public event System.Action OnChanged;

    #region Fields

    private static ListViewWindow s_currentInstance;
    private SerializedProperty _property;
    private Vector2 _scrollPos;

    #endregion

    #region Properties

    #endregion

    public static ListViewWindow Show(SerializedProperty property)
    {
        var window = new ListViewWindow();
        window.titleContent = new GUIContent(property.displayName);
        window._property = property;
        window.ShowUtility();
        window.Focus();
        return window;
    }

    public static void CloseIfAny()
    {
        if (s_currentInstance != null)
        {
            s_currentInstance.Close();
        }
    }

    private void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        EditorGUI.BeginChangeCheck();
        _property.isExpanded = true;
        EditorGUILayout.PropertyField(_property);
        EditorGUILayout.EndScrollView();

        if (EditorGUI.EndChangeCheck())
        {
            _property.serializedObject.ApplyModifiedProperties();
            OnChanged?.Invoke();
        }
    }

    private void OnEnable()
    {
        if (s_currentInstance != null)
        {
            s_currentInstance.Close();
        }

        s_currentInstance = this;
    }

    private void OnDisable()
    {
        s_currentInstance = null;
        OnClosed?.Invoke();
    }
}
