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

        private const int updatesCountToRepaint = 10;
        private const float valueLeftPadding = 5f;
        private const string noneCaption = "None";
        private GUIStyle valueDrawStyle;
        private GUIStyle nameDrawStyle;
        private BTTargetWatcher watcher;
        private float watcherHeight;
        private int updatesSkipped;

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
            watcher = null;
            EditorApplication.update -= EditorUpdate;
        }

        private void EditorUpdate()
        {
            if (!Application.isPlaying) return;

            if (GUI.changed || updatesSkipped >= updatesCountToRepaint)
            {
                updatesSkipped = 0;
                Repaint();
            }
            else
            {
                updatesSkipped++;
            }
        }

        private void OnGUI()
        {
            DrawWatcher();
            DrawRuntimeBlackboard(watcher?.Current?.Blackboard);

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
                watcher.OnGUI(position);
                watcherHeight = watcher.Rect.height;
            }
        }

        private void DrawRuntimeBlackboard(Blackboard blackboard)
        {
            CreateStyleIfNeeded();
            
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(watcherHeight);
                DrawBlackboardImmediate(blackboard);
            }
            else
            {
                EditorGUILayout.LabelField("Below you will see blackboard runtime values", nameDrawStyle, GUILayout.ExpandWidth(true));
            }
        }

        private void DrawBlackboardImmediate(Blackboard blackboard)
        {
            EditorGUILayout.LabelField("Blackboard Properties:");
            if (blackboard?.Values is null) return;

            EditorGUILayout.Space();

            foreach (var key in blackboard.Values.Keys)
            {
                var value = blackboard.Values[key];
                if (value is null) continue;

                var valueType = value.GetType();
                IEnumerable<FieldInfo> fields = new List<FieldInfo>();

                while(valueType != null && valueType != typeof(object))
                {
                    fields = fields.Concat(valueType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
                    valueType = valueType.BaseType;
                }

                var exposedFields = fields.ToArray();
                if (exposedFields.Length == 0) continue;

                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(key), nameDrawStyle, GUILayout.ExpandWidth(true));

                for (int i = 0; i < exposedFields.Length; i++)
                {
                    var fieldInfo = exposedFields[i];
                    var valueExposed = fieldInfo.GetValue(value);

                    string valueString;

                    if (valueExposed is null)
                    {
                        valueString = noneCaption;
                    }
                    else if (valueExposed is Object objectValue)
                    {
                        valueString = objectValue != null ? objectValue.name : noneCaption;
                    }
                    else
                    {
                        valueString = valueExposed != null ? valueExposed.ToString() : noneCaption;
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Box("", GUILayout.Width(valueLeftPadding));
                    EditorGUILayout.LabelField($"{ObjectNames.NicifyVariableName(fieldInfo.Name)} = {valueString}", valueDrawStyle, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(10f);
            }
        }

        private void CreateStyleIfNeeded()
        {
            if (valueDrawStyle is null || nameDrawStyle is null)
            {
                valueDrawStyle = new GUIStyle("box");
                valueDrawStyle.alignment = TextAnchor.MiddleLeft;
                nameDrawStyle = new GUIStyle("box");
                nameDrawStyle.alignment = TextAnchor.MiddleLeft;
                nameDrawStyle.fontStyle = FontStyle.Bold;
                nameDrawStyle.normal.textColor = new Color(0.5f, 1f, 0.7f, 1f);
            }
        }

        private void CreateWatcherIfNeeded()
        {
            if (watcher is null)
            {
                watcher = new BTTargetWatcher();
            }
        }
    }
}
