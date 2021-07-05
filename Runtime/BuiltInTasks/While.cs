using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [TaskOut(0, "Condition")][TaskOut(1, "Execute")][TaskOut(2, "Exit")]
    [NoInspector]
    public class While : BehaviourTask
    {
        #region Fields

        #endregion

        #region Properties

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
                conditionBranch.WaitUntilFinished();
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
