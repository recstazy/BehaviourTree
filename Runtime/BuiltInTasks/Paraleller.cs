using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Execute task on out 0 and execute all other outs while out 0 is running
    /// </summary>
    [NoInspector]
    [TaskMenu("Multiout/Paraleller")]
    public class Paraleller : MultioutTask
    {
        #region Fields

        private BranchPlayer _mainBranch;

        #endregion

        #region Properties

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            _mainBranch = PlayConnectedBranch(0);

            for (int i = 1; i < Connections.Count; i++)
            {
                StartCoroutine(ParallelBranchRoutine(i));
            }

            yield return new WaitUntil(() => !_mainBranch.IsRunning);
            StopAllCoroutines();
            StopAllBranches();
            Succeed = _mainBranch.BranchSucceed;
        }

        protected override int GetCurrentOutIndex()
        {
            return NoOut;
        }

        private IEnumerator ParallelBranchRoutine(int index)
        {
            while(_mainBranch.IsRunning)
            {
                var branch = PlayConnectedBranch(index);
                yield return branch.WaitUntilFinished();
            }
        }
    }
}
