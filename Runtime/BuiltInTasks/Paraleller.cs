using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Execute task on out 0 and execute all other outs while out 0 is running
    /// </summary>
    [NoInspector]
    public class Paraleller : MultioutTask
    {
        #region Fields

        private BranchPlayer mainPlayer;
        private List<BranchPlayer> dependentPlayers;

        private Coroutine mainTaskRoutine;
        private List<Coroutine> dependentRoutines;

        #endregion

        #region Properties

        #endregion

        protected override IEnumerator TaskRoutine()
        {
            dependentRoutines = new List<Coroutine>();
            dependentPlayers = new List<BranchPlayer>();

            var mainTask = GetConnectionSafe(0);
            mainPlayer = new BranchPlayer(mainTask);
            mainTaskRoutine = StartCoroutine(MainRunner(mainPlayer));

            for (int i = 1; i < Connections.Count; i++)
            {
                var player = new BranchPlayer(GetConnectionSafe(i));
                dependentPlayers.Add(player);
                dependentRoutines.Add(StartCoroutine(ParallelRunner(player)));
            }

            yield return new WaitUntil(() => !mainPlayer.IsRunning);
            yield return null;
            StopAllCoroutines();
            Succeed = mainPlayer.BranchSucceed;
        }

        protected override int GetCurrentOutIndex()
        {
            return NoOut;
        }

        private IEnumerator MainRunner(BranchPlayer player)
        {
            yield return player.PlayBranchRoutine();
            mainTaskRoutine = null;
        }

        private IEnumerator ParallelRunner(BranchPlayer branchPlayer)
        {
            while(mainTaskRoutine != null)
            {
                yield return branchPlayer.PlayBranchRoutine();
            }
        }
    }
}
