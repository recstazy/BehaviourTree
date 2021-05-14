using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BehaviourTreeNode
    {
        #region Fields

        private const float selectionSize = 5f;
        private const float headerHeight = 14f;
        private const float minDescriptionHeight = 26f;
        private const float widthPadding = 5f;
        private const float heightPadding = 5f;
        private const float runninigMarkWidth = 5f;
        private const int wrapCountTreshold = 16;
        private const int minOutPinWidth = 20;
        private static readonly Vector2 minSize = new Vector2(150f, headerHeight + minDescriptionHeight);
        private static readonly Color backColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        private static readonly Color selectionColor = new Color(0.6f, 0.6f, 0.5f, 1f);
        private static readonly Color runningMarkColor = new Color(0.5f, 1f, 0.7f, 1f);

        private Rect mainRect;
        private NodeData data = null;
        private BTNodeTaskProvider taskProvider;
        private GUIStyle descriptionStyle;
        private GUIContent description;
        private bool wasRuningOnLastCheck;

        #endregion

        #region Properties

        public NodeData Data => data;
        public bool Selected { get; set; }
        public Rect MainRect => mainRect;
        public int Index { get; private set; }
        public bool IsDirty { get; set; }
        public TaskConnection[] Connections { get; private set; }
        public bool RuntimeIsRunning => data?.TaskImplementation is null ? false : data.TaskImplementation.IsRunning;
        public bool RuntimeIsRunningChanged => CheckIsRuntimeRunningChanged();

        protected virtual Vector2 Size { get => DefaultGetSize(); }
        protected virtual Color BackColor { get => backColor; }
        protected virtual Color SelectionColor => selectionColor;
        protected virtual bool ShouldDrawTaskProvider => true;

        #endregion

        public BehaviourTreeNode(NodeData data)
        {
            taskProvider = new BTNodeTaskProvider(data);
            mainRect = new Rect(data.Position, minSize);
            this.data = data;
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
            mainRect.size = Size;
            var rect = mainRect;
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
            mainRect.position += delta;
            UpdateData();
        }

        protected void DrawSelection(Rect rect)
        {
            var selectionRect = rect;
            selectionRect.size += Vector2.one * selectionSize;
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
                rect.x += widthPadding;
                rect.y += heightPadding;
                rect.height = headerHeight;
                rect.width -= widthPadding * 2f;

                EditorGUI.BeginDisabledGroup(BTModeManager.IsPlaymode);
                taskProvider.OnGUI(rect);
                var task = taskProvider.CurrentImplementation;
                EditorGUI.EndDisabledGroup();

                if (task != data.TaskImplementation)
                {
                    data.TaskImplementation = task;
                    IsDirty = true;
                }
            }
        }

        protected void DrawDescription(Rect transformedRect)
        {
            var rect = transformedRect;
            rect.y += headerHeight + heightPadding * 2f;
            rect.x += widthPadding;
            rect.width -= widthPadding * 2f;
            rect.height = transformedRect.height - heightPadding * 3f - headerHeight;
            EditorGUI.LabelField(rect, description, descriptionStyle);
        }

        protected string GetNameDescription()
        {
            return data.TaskImplementation is null ? "Empty Task" : ObjectNames.NicifyVariableName(data.TaskImplementation.GetType().Name);
        }

        protected virtual string GetDescription()
        {
            var taskDescription = data.TaskImplementation?.GetDescription();
            taskDescription = string.IsNullOrEmpty(taskDescription) ? GetNameDescription() : taskDescription;
            return WrapDescription(taskDescription);
        }

        private void DrawRunningMark(Rect transformedRect)
        {
            if (BTModeManager.IsPlaymode && RuntimeIsRunning)
            {
                var rect = transformedRect;
                rect.x -= runninigMarkWidth;
                rect.width = runninigMarkWidth;

                EditorGUI.DrawRect(rect, runningMarkColor);
            }
        }

        private void UpdateData()
        {
            data.Position = mainRect.position;
        }

        private void CreateStyleIfNeeded()
        {
            if (descriptionStyle == null)
            {
                descriptionStyle = new GUIStyle();
                descriptionStyle.alignment = TextAnchor.MiddleCenter;
                descriptionStyle.normal.textColor = Color.white;
            }
        }

        private void UpdateDescription()
        {
            string descriptionString = GetDescription();

            if (description is null)
            {
                description = new GUIContent(descriptionString);
            }
            else
            {
                description.text = descriptionString;
            }
        }

        protected Vector2 DefaultGetSize()
        {
            if (descriptionStyle != null)
            {
                var descriptionSize = descriptionStyle.CalcSize(description);
                var width = Mathf.Max(minSize.x, descriptionSize.x + widthPadding * 2f);

                int outsCount = data.Connections is null ? 0 : data.Connections.Length;
                float currentOutPinWidth = width / outsCount;

                if (currentOutPinWidth < minOutPinWidth)
                {
                    width = minOutPinWidth * outsCount;
                }

                var height = Mathf.Max(minSize.y, headerHeight + descriptionSize.y + heightPadding * 2f);
                return new Vector2(width, height);
            }

            return minSize;
        }

        private bool CheckIsRuntimeRunningChanged()
        {
            bool changed = RuntimeIsRunning != wasRuningOnLastCheck;
            wasRuningOnLastCheck = RuntimeIsRunning;
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
                bool shouldWrap = symbolsCount >= wrapCountTreshold && i != parts.Length - 1;

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
