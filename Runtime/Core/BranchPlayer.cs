using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Used to play BehaviourTree branch starting from provided root task
    /// </summary>
    public class BranchPlayer
    {
        #region Fields

        private BehaviourTask root;
        private BehaviourTask currentTask;
        private bool forceSucceed;
        private bool canPlay;

        #endregion

        #region Properties
		
        /// <summary> True if all the executed tasks suceeded </summary>
        public bool BranchSucceed { get; private set; }
        public bool IsRunning { get; private set; }

        #endregion

        /// <param name="root">Root task to start from</param>
        public BranchPlayer(BehaviourTask root)
        {
            this.root = root;
        }

        /// <summary> Yield or start coroutine to play the branch </summary>
        public IEnumerator PlayBranchRoutine()
        {
            IsRunning = true;
            currentTask = root;
            canPlay = true;

            while (currentTask != null)
            {
                yield return TaskRunRoutine(currentTask);

                if (!canPlay)
                {
                    BranchSucceed = forceSucceed;
                    break;
                }

                if (currentTask.Succeed)
                {
                    var next = currentTask.GetConnectionSafe(currentTask.GetCurrentOut());

                    if (next is null)
                    {
                        BranchSucceed = true;
                        break;
                    }

                    currentTask = next;
                }
                else
                {
                    BranchSucceed = false;
                    break;
                }
            }

            yield return null;
            IsRunning = false;
        }

        public void StopImmediate(bool forceSucceed)
        {
            canPlay = false;
            this.forceSucceed = forceSucceed;
        }

        private IEnumerator TaskRunRoutine(BehaviourTask task)
        {
            var enumerator = task.StartTask();

            while(enumerator.MoveNext())
            {
                yield return enumerator.Current;

                if (!canPlay)
                {
                    task.StopImmediate(forceSucceed);
                }
            }
        }
    }
}
