using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BehaviourTreeNode
    {
        #region Fields

        private const float SelectionSize = 5f;
        private const float HeaderHeight = 14f;
        private const float MinDescriptionHeight = 26f;
        private const float WidthPadding = 5f;
        private const float HeightPadding = 5f;
        private const float RunninigMarkWidth = 5f;
        private const int WrapCountTreshold = 16;
        private const int MinOutPinWidth = 20;
        private static readonly Vector2 s_minSize = new Vector2(150f, HeaderHeight + MinDescriptionHeight);
        private static readonly Color s_defaultBackColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        private static readonly Color s_selectionColor = new Color(0.6f, 0.6f, 0.5f, 1f);
        private static readonly Color s_runningMarkColor = new Color(0.5f, 1f, 0.7f, 1f);

        private Rect _mainRect;
        private BTNodeTaskProvider _taskProvider;
        private GUIStyle _descriptionStyle;
        private GUIContent _description;
        private bool _wasRuningOnLastCheck;

        #endregion

        #region Properties

        public NodeData Data { get; } = null;
        public bool Selected { get; set; }
        public Rect MainRect => _mainRect;
        public int Index { get; private set; }
        public bool IsDirty { get; set; }
        public TaskConnection[] Connections { get; private set; }
        public bool RuntimeIsRunning => Data?.TaskImplementation is null ? false : Data.TaskImplementation.IsRunning;
        public bool RuntimeIsRunningChanged => CheckIsRuntimeRunningChanged();

        protected virtual Vector2 Size { get => DefaultGetSize(); }
        protected virtual Color BackColor { get => Data.TaskImplementation != null ? Data.TaskImplementation.GetColor() : s_defaultBackColor; }
        protected virtual Color SelectionColor => s_selectionColor;
        protected virtual bool ShouldDrawTaskProvider => true;

        #endregion

        public BehaviourTreeNode(NodeData data)
        {
            _taskProvider = new BTNodeTaskProvider(data);
            _mainRect = new Rect(data.CenterPosition - s_minSize * 0.5f, s_minSize);
            Data = data;
            Index = data.Index;

            if (data.Connections == null)
            {
                Connections = new TaskConnection[0];
            }
            else
            {
                Connections = data.Connections;
            }
        }

        public Rect GetTransformedRect()
        {
            var lastCenter = _mainRect.center;
            _mainRect.size = Size;
            var deltaCenter = _mainRect.center - lastCenter;
            _mainRect.position -= deltaCenter;
            var rect = _mainRect;

            if (BTSnapManager.SnapEnabled)
            {
                var newPosition = BTSnapManager.RoundToSnap(rect.center) - rect.size * 0.5f;
                rect.position = newPosition;
            }

            rect.position -= EditorZoomer.ContentOffset;
            return rect;
        }

        public void OnGUI()
        {
            CreateStyleIfNeeded();
            UpdateDescription();
            var rect = GetTransformedRect();

            if (Selected)
            {
                DrawSelection(rect);
            }

            EditorGUI.DrawRect(rect, BackColor);
            DrawContent(rect);
        }

        public void Drag(Vector2 delta)
        {
            _mainRect.center += delta;
            UpdateData();
        }

        public void EndDrag()
        {
            if (BTSnapManager.SnapEnabled)
            {
                _mainRect.center = BTSnapManager.RoundToSnap(_mainRect.center);
            }
        }

        protected void DrawSelection(Rect rect)
        {
            var selectionRect = rect;
            selectionRect.size += Vector2.one * SelectionSize;
            selectionRect.center = rect.center;
            EditorGUI.DrawRect(selectionRect, SelectionColor);
        }

        protected virtual void DrawContent(Rect transformedRect)
        {
            DrawTaskProvider(transformedRect);
            DrawDescription(transformedRect);
            DrawRunningMark(transformedRect);
        }

        protected void DrawTaskProvider(Rect transformedRect)
        {
            if (ShouldDrawTaskProvider)
            {
                var rect = transformedRect;
                rect.x += WidthPadding;
                rect.y += HeightPadding;
                rect.height = HeaderHeight;
                rect.width -= WidthPadding * 2f;

                EditorGUI.BeginDisabledGroup(BTModeManager.IsPlaymode);
                _taskProvider.OnGUI(rect);
                var task = _taskProvider.CurrentImplementation;
                EditorGUI.EndDisabledGroup();

                if (task != Data.TaskImplementation)
                {
                    Data.TaskImplementation = task;
                    IsDirty = true;
                }
            }
        }

        protected void DrawDescription(Rect transformedRect)
        {
            var rect = transformedRect;
            rect.y += HeaderHeight + HeightPadding * 2f;
            rect.x += WidthPadding;
            rect.width -= WidthPadding * 2f;
            rect.height = transformedRect.height - HeightPadding * 3f - HeaderHeight;
            EditorGUI.LabelField(rect, _description, _descriptionStyle);
        }

        protected string GetNameDescription()
        {
            return Data.TaskImplementation == null ? "Empty Task" : ObjectNames.NicifyVariableName(Data.TaskImplementation.GetType().Name);
        }

        protected virtual string GetDescription()
        {
            var taskDescription = Data.TaskImplementation?.GetDescription();
            taskDescription = string.IsNullOrEmpty(taskDescription) ? GetNameDescription() : taskDescription;
            return WrapDescription(taskDescription);
        }

        private void DrawRunningMark(Rect transformedRect)
        {
            if (BTModeManager.IsPlaymode && RuntimeIsRunning)
            {
                var rect = transformedRect;
                rect.x -= RunninigMarkWidth;
                rect.width = RunninigMarkWidth;

                EditorGUI.DrawRect(rect, s_runningMarkColor);
            }
        }

        private void UpdateData()
        {
            var savedPosition = _mainRect.center;

            if (BTSnapManager.SnapEnabled)
            {
                savedPosition = BTSnapManager.RoundToSnap(savedPosition);
            }

            Data.CenterPosition = savedPosition;
        }

        private void CreateStyleIfNeeded()
        {
            if (_descriptionStyle == null)
            {
                _descriptionStyle = new GUIStyle();
                _descriptionStyle.alignment = TextAnchor.MiddleCenter;
                _descriptionStyle.normal.textColor = Color.white;
            }
        }

        private void UpdateDescription()
        {
            string descriptionString = GetDescription();

            if (_description == null)
            {
                _description = new GUIContent(descriptionString);
            }
            else
            {
                _description.text = descriptionString;
            }
        }

        protected Vector2 DefaultGetSize()
        {
            if (_descriptionStyle != null)
            {
                var descriptionSize = _descriptionStyle.CalcSize(_description);
                var width = Mathf.Max(s_minSize.x, descriptionSize.x + WidthPadding * 2f);

                int outsCount = Data.Connections is null ? 0 : Data.Connections.Length;
                float currentOutPinWidth = width / outsCount;

                if (currentOutPinWidth < MinOutPinWidth)
                {
                    width = MinOutPinWidth * outsCount;
                }

                var height = Mathf.Max(s_minSize.y, HeaderHeight + descriptionSize.y + HeightPadding * 2f);
                var size = new Vector2(width, height);
                return size;
            }

            return s_minSize;
        }

        private bool CheckIsRuntimeRunningChanged()
        {
            bool changed = RuntimeIsRunning != _wasRuningOnLastCheck;
            _wasRuningOnLastCheck = RuntimeIsRunning;
            return changed;
        }

        private string WrapDescription(string sourceDescription)
        {
            string result = "";
            string[] parts = sourceDescription.Split(' ');
            int symbolsCount = 0;

            for (int i = 0; i < parts.Length; i++)
            {
                symbolsCount += parts[i].Length;
                bool shouldWrap = symbolsCount >= WrapCountTreshold && i != parts.Length - 1;

                if (shouldWrap)
                {
                    symbolsCount = 0;
                }

                result += parts[i] + (shouldWrap ? "\n" : " ");
            }

            return result;
        }
    }
}
