using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class ConnectionPin : Pin
    {
        #region Fields

        private const float emptyPinHeight = 10f;
        private const float textPinHeight = 15f;
        private GUIStyle pinStyle;

        #endregion

        #region Properties

        public bool IsHover { get; private set; }
        public bool MouseDrag { get; private set; }
        public bool MouseDown { get; private set; }
        public bool MouseUp { get; private set; }
        public bool IsConnected { get; set; }
        public bool IsInput { get; private set; }
        public System.Func<Vector2> GetMousePosition;
        public string Description { get; set; }

        #endregion

        public ConnectionPin(int index, bool isInput, string description = "") : base(index)
        {
            IsInput = isInput;
            rect.size = Vector2.one * emptyPinHeight;
            Description = description;
        }

        public override void OnGUI(Vector2 posOnNodeBorder, float width)
        {
            if (pinStyle == null)
            {
                CreateStyle();
            }

            MouseDrag = false;
            MouseUp = false;

            float height = string.IsNullOrEmpty(Description) ? emptyPinHeight : textPinHeight;
            rect.size = new Vector2(width, height);
            rect.center = posOnNodeBorder + height * 0.5f * Vector2.up * (IsInput ? -1f : 1f);
            GUI.Box(rect, Description, pinStyle);

            var mousePos = GetMousePosition is null ? Vector2.positiveInfinity : GetMousePosition();
            IsHover = rect.Contains(mousePos);

            switch (Event.current.type)
            {
                case EventType.MouseDrag:
                    {
                        if (Event.current.button == 0)
                        {
                            if (MouseDown && IsHover)
                            {
                                MouseDrag = true;
                            }
                        }

                        break;
                    }
                case EventType.MouseDown:
                    {
                        if (Event.current.button == 0)
                        {
                            if (IsHover)
                            {
                                MouseDown = true;
                            }
                        }

                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (Event.current.button == 0)
                        {
                            if (IsHover)
                            {
                                MouseUp = true;
                            }

                            MouseDown = false;
                        }

                        break;
                    }
            }
        }

        private void CreateStyle()
        {
            pinStyle = new GUIStyle("EditModeSingleButton");
            pinStyle.alignment = TextAnchor.MiddleCenter;
            pinStyle.fontSize = 9;
        }
    }
}
