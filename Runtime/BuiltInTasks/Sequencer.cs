using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Execute outs in index order until one of them fails - then stop and fail too. If all outs succeeded - succeed.
    /// </summary>
    [NoInspector]
    public class Sequencer : MultioutTask
    {
        #region Fields

        private BranchPlayer branchPlayer;

        #endregion

        #region Properties

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            bool allSucceed = true;

            for (int i = 0; i < Connections.Count; i++)
            {
                branchPlayer = PlayConnectedBranch(i);
                yield return branchPlayer.WaitUntilFinished();

                if (!branchPlayer.BranchSucceed)
                {
                    allSucceed = false;
                    break;
                }
            }

            yield return null;
            Succeed = allSucceed;
        }

        protected override int GetCurrentOutIndex()
        {
            return NoOut;
        }
    }
}
