using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class PinConnectionDrawer
    {
        #region Fields

        private Pin source;
        private Pin target;
        private const float tangent = 30f;
        private const float width = 3f;
        private static readonly Color activeColor = Color.white;
        private static readonly Color disabledColor = new Color(1f, 1f, 1f, 0.5f);

        #endregion

        #region Properties

        public Pin Source { get => source; }
        public Pin Target { get => target; }
        public bool SourceIsOutput { get; private set; }
        public bool RuntimeIsRunning { get; set; }
        public int OutPin { get; set; }

        #endregion

        public PinConnectionDrawer(Pin source, Pin target, bool sourceIsOutput)
        {
            this.source = source;
            this.target = target;
            SourceIsOutput = sourceIsOutput;
        }

        public void OnGUI()
        {
            Vector2 sourceTangent = source.Rect.center + tangent * Vector2.up * (SourceIsOutput ? 1f : -1f);
            Vector2 targetTangent = target.Rect.center + tangent * Vector2.down * (SourceIsOutput ? 1f : -1f);
            Handles.DrawBezier(Source.Rect.center, Target.Rect.center, sourceTangent, targetTangent, GetCurrentColor(), null, width);
            RuntimeIsRunning = false;
        }

        private Color GetCurrentColor()
        {
            if (BTModeManager.IsPlaymode)
            {
                return RuntimeIsRunning ? activeColor : disabledColor;
            }
            else return activeColor;
        }
    }
}
