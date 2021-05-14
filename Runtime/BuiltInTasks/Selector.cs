using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Execute outs in index order until one of them succeeds - then stop and succeed too. If no out succeeded - fail.
    /// </summary>
    [NoInspector]
    public class Selector : MultioutTask
    {
        #region Fields

        private BranchPlayer branchPlayer;

        #endregion

        #region Properties

        #endregion

        protected override void StoppedExternaly(bool forceSucceed)
        {
            branchPlayer?.StopImmediate(forceSucceed);
        }

        protected override IEnumerator TaskRoutine()
        {
            bool anySucceed = false;

            for (int i = 0; i < Connections.Count && CanRun; i++)
            {
                branchPlayer = new BranchPlayer(GetConnectionSafe(i));
                yield return branchPlayer.PlayBranchRoutine();

                if (branchPlayer.BranchSucceed)
                {
                    anySucceed = true;
                    break;
                }
            }

            yield return null;
            Succeed = anySucceed;
        }

        protected override int GetCurrentOutIndex()
        {
            return NoOut;
        }
    }
}
