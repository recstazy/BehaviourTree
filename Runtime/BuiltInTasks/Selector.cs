using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Execute outs in index order until one of them succeeds - then stop and succeed too. If no out succeeded - fail.
    /// </summary>
    [NoInspector]
    [TaskMenu("Multiout/Selector")]
    public class Selector : MultioutTask
    {
        #region Fields

        private BranchPlayer _branchPlayer;

        #endregion

        #region Properties

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            bool anySucceed = false;

            for (int i = 0; i < Connections.Count; i++)
            {
                _branchPlayer = PlayConnectedBranch(i);
                yield return _branchPlayer.WaitUntilFinished();

                if (_branchPlayer.BranchSucceed)
                {
                    anySucceed = true;
                    break;
                }
            }

            yield return null;
            Succeed = anySucceed;
        }

        protected override int GetNextOutIndex()
        {
            return NoOut;
        }
    }
}
