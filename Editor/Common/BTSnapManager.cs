using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    public class BTSnapManager
    {
        #region Fields

        #endregion

        #region Properties

        public static bool SnapEnabled { get; private set; } = true;
        public static int GridSize => 10;

        #endregion

        public static Vector2 RoundToSnap(Vector2 vector)
        {
            return new Vector2(RoundToSnap(vector.x), RoundToSnap(vector.y));
        }

        public static float RoundToSnap(float value)
        {
            return Mathf.Round(value / GridSize) * GridSize;
        }
    }
}
