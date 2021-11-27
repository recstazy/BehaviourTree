using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [TaskOut(0, "Condition")][TaskOut(1, "Execute")][TaskOut(2, "Exit")]
    [NoInspector]
    [TaskMenu("Multiout/While Loop")]
    public class While : BehaviourTask
    {
        #region Fields

        private static readonly Color s_backColor = new Color(0.1f, 0.05f, 0.15f, 0.5f);

        #endregion

        #region Properties

        protected override Color Color => s_backColor;

        #endregion

        protected override int GetCurrentOutIndex()
        {
            return 2;
        }

        protected override IEnumerator TaskRoutine()
        {
            bool condition;

            do
            {
                var conditionBranch = PlayConnectedBranch(0);
                yield return conditionBranch.WaitUntilFinished();
                condition = conditionBranch.BranchSucceed;

                if (condition)
                {
                    var bodyBranch = PlayConnectedBranch(1);
                    yield return bodyBranch.WaitUntilFinished();
                }
            }
            while (condition);

            Succeed = true;
        }
    }
}
