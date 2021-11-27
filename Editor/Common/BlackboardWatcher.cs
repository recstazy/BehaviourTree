using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class BlackboardWatcher : EditorWindow
    {
        #region Fields

        private const int UpdatesCountToRepaint = 10;
        private const float ValueLeftPadding = 5f;
        private const string NoneCaption = "None";
        private GUIStyle _valueDrawStyle;
        private GUIStyle _nameDrawStyle;
        private BTTargetWatcher _watcher;
        private float _watcherHeight;
        private int _updatesSkipped;

        #endregion

        #region Properties

        #endregion

        [MenuItem("Window/AI/Blackboard Watcher")]
        private static void ShowWindow()
        {
            var window = GetWindow<BlackboardWatcher>("Blackboard Watcher");
        }

        private void OnEnable()
        {
            EditorApplication.update += EditorUpdate;
            BTTargetWatcher.UpdateIsPlaymode();
        }

        private void OnDisable()
        {
            _watcher = null;
            EditorApplication.update -= EditorUpdate;
        }

        private void EditorUpdate()
        {
            if (!Application.isPlaying) return;

            if (GUI.changed || _updatesSkipped >= UpdatesCountToRepaint)
            {
                _updatesSkipped = 0;
                Repaint();
            }
            else
            {
                _updatesSkipped++;
            }
        }

        private void OnGUI()
        {
            DrawWatcher();
            DrawRuntimeBlackboard(_watcher?.Current?.Blackboard);

            if (GUI.changed)
            {
                Repaint();
            }
        }

        private void DrawWatcher()
        {
            if (Application.isPlaying)
            {
                CreateWatcherIfNeeded();
                _watcher.OnGUI(position);
                _watcherHeight = _watcher.Rect.height;
            }
        }

        private void DrawRuntimeBlackboard(Blackboard blackboard)
        {
            CreateStyleIfNeeded();
            
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(_watcherHeight);
                DrawBlackboardImmediate(blackboard);
            }
            else
            {
                EditorGUILayout.LabelField("Below you will see blackboard runtime values", _nameDrawStyle, GUILayout.ExpandWidth(true));
            }
        }

        private void DrawBlackboardImmediate(Blackboard blackboard)
        {
            EditorGUILayout.LabelField("Blackboard Properties:");
            if (blackboard == null || !blackboard.ArePropertiesBound) return;

            var bindable = blackboard.GetBindableProperties().Where(prop => prop.GetCustomAttribute<HideInWatcherAttribute>() == null).ToArray();
            EditorGUILayout.Space();

            foreach (var propertyInfo in bindable)
            {
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(propertyInfo.Name), _nameDrawStyle, GUILayout.ExpandWidth(true));
                var value = propertyInfo.GetValue(blackboard);

                string valueString;

                if (value == null)
                {
                    valueString = NoneCaption;
                }
                else if (value is Object objectValue)
                {
                    valueString = objectValue != null ? objectValue.name : NoneCaption;
                }
                else
                {
                    valueString = value != null ? value.ToString() : NoneCaption;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Box("", GUILayout.Width(ValueLeftPadding));
                EditorGUILayout.LabelField($"{ObjectNames.NicifyVariableName(propertyInfo.Name)} = {valueString}", _valueDrawStyle, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10f);
            }
        }

        private void CreateStyleIfNeeded()
        {
            if (_valueDrawStyle == null || _nameDrawStyle == null)
            {
                _valueDrawStyle = new GUIStyle("box");
                _valueDrawStyle.alignment = TextAnchor.MiddleLeft;
                _nameDrawStyle = new GUIStyle("box");
                _nameDrawStyle.alignment = TextAnchor.MiddleLeft;
                _nameDrawStyle.fontStyle = FontStyle.Bold;
                _nameDrawStyle.normal.textColor = new Color(0.5f, 1f, 0.7f, 1f);
            }
        }

        private void CreateWatcherIfNeeded()
        {
            if (_watcher == null)
            {
                _watcher = new BTTargetWatcher();
            }
        }
    }
}
