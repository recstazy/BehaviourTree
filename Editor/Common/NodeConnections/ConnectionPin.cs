using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class ConnectionPin : Pin
    {
        #region Fields

        private const float EmptyPinHeight = 10f;
        private const float TextPinHeight = 15f;
        private GUIStyle _pinStyle;

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
            _rect.size = Vector2.one * EmptyPinHeight;
            Description = description;
        }

        public override void OnGUI(Vector2 posOnNodeBorder, float width)
        {
            if (_pinStyle == null)
            {
                CreateStyle();
            }

            MouseDrag = false;
            MouseUp = false;

            float height = string.IsNullOrEmpty(Description) ? EmptyPinHeight : TextPinHeight;
            _rect.size = new Vector2(width, height);
            _rect.center = posOnNodeBorder + height * 0.5f * Vector2.up * (IsInput ? -1f : 1f);
            GUI.Box(_rect, Description, _pinStyle);

            var mousePos = GetMousePosition is null ? Vector2.positiveInfinity : GetMousePosition();
            IsHover = _rect.Contains(mousePos);

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
            _pinStyle = new GUIStyle("EditModeSingleButton");
            _pinStyle.alignment = TextAnchor.MiddleCenter;
            _pinStyle.fontSize = 9;
        }
    }
}
