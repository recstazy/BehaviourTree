using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Execute outs in index order until one of them fails - then stop and fail too. If all outs succeeded - succeed.
    /// </summary>
    [NoInspector]
    [TaskMenu("Multiout/Sequencer")]
    public class Sequencer : MultioutTask
    {
        #region Fields

        private BranchPlayer _branchPlayer;

        #endregion

        #region Properties

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            bool allSucceed = true;

            for (int i = 0; i < Connections.Count; i++)
            {
                _branchPlayer = PlayConnectedBranch(i);
                yield return _branchPlayer.WaitUntilFinished();

                if (!_branchPlayer.BranchSucceed)
                {
                    allSucceed = false;
                    break;
                }
            }

            yield return null;
            Succeed = allSucceed;
        }

        protected override int GetNextOutIndex()
        {
            return NoOut;
        }
    }
}
