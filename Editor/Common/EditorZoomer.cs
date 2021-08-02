using UnityEngine;
using System;

namespace Recstazy.BehaviourTree.EditorScripts
{
    //based on the code in this post: http://martinecker.com/martincodes/unity-editor-window-zooming/
    //but I changed how the API works and made it much more flexible
    //usage: create an EditorZoomer instance wherever you want to use it (it tracks the pan + zoom state)
    //in your OnGUI, draw your scrollable content between zoomer.Begin() and zoomer.End();
    //you also must offset your content by zoomer.GetContentOffset();

    internal class EditorZoomer
    {
        public static Vector2 ContentOffset => GetContentOffset();
        public static float Zoom { get; set; } = 1f;
        public static Vector2 ZoomOrigin { get; set; } = Vector2.zero;
        public static bool PanOrZoomChanged { get; private set; }

        private const float EditorWindowTabHeight = 21.0f;
        private static Rect s_zoomArea = new Rect();
        private static Vector2 s_lastMouse = Vector2.zero;
        private static Matrix4x4 s_prevMatrix;
        private static Rect s_possibleZoomArea;

        public static Rect Begin(params GUILayoutOption[] options)
        {
            HandleEvents();

            // Because I don't know how to fix this exception thrown 
            // - Only if returing to editor from playmode 
            // - And only After selecting runtime behaviour player in window
            // - And only in first repaint after entering editor mode
            // - And skipping first repaint doesn't help but skipping zoom in only first repaint works
            try
            {
                s_possibleZoomArea = GUILayoutUtility.GetRect(0, 10000, 0, 10000, options);
            }
            catch (ArgumentException) { }

            if (Event.current.type == EventType.Repaint) //the size is correct during repaint, during layout it's 1,1
            {
                s_zoomArea = s_possibleZoomArea;
            }

            GUI.EndGroup(); // End the group Unity begins automatically for an EditorWindow to clip out the window tab. This allows us to draw outside of the size of the EditorWindow.

            Rect clippedArea = s_zoomArea.ScaleSizeBy(1f / Zoom, s_zoomArea.TopLeft());
            clippedArea.y += EditorWindowTabHeight;
            GUI.BeginGroup(clippedArea);

            s_prevMatrix = GUI.matrix;
            Matrix4x4 translation = Matrix4x4.TRS(clippedArea.TopLeft(), Quaternion.identity, Vector3.one);
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(Zoom, Zoom, 1.0f));
            GUI.matrix = translation * scale * translation.inverse * GUI.matrix;

            return clippedArea;
        }

        public static void End()
        {
            GUI.matrix = s_prevMatrix; //restore the original matrix
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(0.0f, EditorWindowTabHeight, Screen.width, Screen.height));
        }

        public static void HandleEvents()
        {
            PanOrZoomChanged = false;

            if (Event.current.isMouse)
            {
                if (Event.current.type == EventType.MouseDrag && ((Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) || Event.current.button == 2))
                {
                    var mouseDelta = Event.current.mousePosition - s_lastMouse;
                    ZoomOrigin += mouseDelta;
                    Event.current.Use();
                }

                s_lastMouse = Event.current.mousePosition;
                PanOrZoomChanged = true;
            }

            if (Event.current.type == EventType.ScrollWheel)
            {
                float oldZoom = Zoom;

                float zoomChange = 1.10f;

                Zoom *= Mathf.Pow(zoomChange, -Event.current.delta.y / 3f);
                Zoom = Mathf.Clamp(Zoom, 0.1f, 2f);

                bool shouldZoomTowardsMouse = true; //if this is false, it will always zoom towards the center of the content (0,0)

                if (shouldZoomTowardsMouse)
                {
                    //we want the same content that was under the mouse pre-zoom to be there post-zoom as well
                    //in other words, the content's position *relative to the mouse* should not change

                    Vector2 areaMousePos = Event.current.mousePosition - s_zoomArea.center;

                    Vector2 contentOldMousePos = (areaMousePos / oldZoom) - (ZoomOrigin / oldZoom);
                    Vector2 contentMousePos = (areaMousePos / Zoom) - (ZoomOrigin / Zoom);

                    Vector2 mouseDelta = contentMousePos - contentOldMousePos;

                    ZoomOrigin += mouseDelta * Zoom;
                }

                PanOrZoomChanged = true;
            }
        }

        public static Vector2 GetContentOffset()
        {
            Vector2 offset = -ZoomOrigin / Zoom; //offset the midpoint
            offset -= (s_zoomArea.size / 2f) / Zoom; //offset the center
            return offset;
        }
    }

    // Helper Rect extension methods
    public static class RectExtensions
    {
        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }

        public static Rect ScaleSizeBy(this Rect rect, float scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }

        public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale;
            result.xMax *= scale;
            result.yMin *= scale;
            result.yMax *= scale;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }
        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }
        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale.x;
            result.xMax *= scale.x;
            result.yMin *= scale.y;
            result.yMax *= scale.y;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }
    }
}