using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class PinConnectionDrawer
    {
        #region Fields

        private const float Tangent = 30f;
        private const float Width = 3f;
        private static readonly Color s_activeColor = Color.white;
        private static readonly Color s_disabledColor = new Color(1f, 1f, 1f, 0.5f);

        #endregion

        #region Properties

        public Pin Source { get; }
        public Pin Target { get; }
        public bool SourceIsOutput { get; private set; }
        public bool RuntimeIsRunning { get; set; }
        public int OutPin { get; set; }

        #endregion

        public PinConnectionDrawer(Pin source, Pin target, bool sourceIsOutput)
        {
            Source = source;
            Target = target;
            SourceIsOutput = sourceIsOutput;
        }

        public void OnGUI()
        {
            Vector2 sourceTangent = Source.Rect.center + Tangent * Vector2.up * (SourceIsOutput ? 1f : -1f);
            Vector2 targetTangent = Target.Rect.center + Tangent * Vector2.down * (SourceIsOutput ? 1f : -1f);
            Handles.DrawBezier(Source.Rect.center, Target.Rect.center, sourceTangent, targetTangent, GetCurrentColor(), null, Width);
            RuntimeIsRunning = false;
        }

        private Color GetCurrentColor()
        {
            if (BTModeManager.IsPlaymode)
            {
                return RuntimeIsRunning ? s_activeColor : s_disabledColor;
            }
            else return s_activeColor;
        }
    }
}
