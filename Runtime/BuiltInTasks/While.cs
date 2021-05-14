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
                var conditionTask = GetConnectionSafe(0);
                var conditionBranch = new BranchPlayer(conditionTask);
                yield return conditionBranch.PlayBranchRoutine();
                condition = conditionBranch.BranchSucceed;

                if (condition)
                {
                    var executedTask = GetConnectionSafe(1);
                    var executedBranch = new BranchPlayer(executedTask);
                    yield return executedBranch.PlayBranchRoutine();
                }
            }
            while (condition);

            Succeed = true;
        }
    }
}
