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
            var newX = Mathf.RoundToInt(vector.x / GridSize) * GridSize;
            var newY = Mathf.RoundToInt(vector.y / GridSize) * GridSize;
            return new Vector2(newX, newY);
        }
    }
}
