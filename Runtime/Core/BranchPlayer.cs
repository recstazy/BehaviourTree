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

        private BehaviourTask _root;
        private BehaviourTask _currentTask;

        private Coroutine _branchRountine;
        private CoroutineRunner _coroutineRunner;

        #endregion

        #region Properties
		
        /// <summary> True if all the executed tasks suceeded </summary>
        public bool BranchSucceed { get; private set; }
        public bool IsRunning { get; private set; }

        #endregion

        public IEnumerator WaitUntilFinished()
        {
            yield return new WaitUntil(() => !IsRunning);
        }

        internal BranchPlayer(BehaviourTask root)
        {
            if (root == null) throw new System.NullReferenceException("BranchPlayer root task must not be null");
            _root = root;
            _coroutineRunner = _root.CoroutineRunner;
        }

        internal void Start()
        {
            _branchRountine = _coroutineRunner.StartCoroutine(PlayBranchRoutine());
        }

        internal void Stop()
        {
            if (_branchRountine != null)
            {
                _coroutineRunner.StopCoroutine(_branchRountine);
            }

            if (_root != null)
            {
                _root.ForceFinishTask(false);
            }

            IsRunning = false;
        }

        /// <summary> Yield or start coroutine to play the branch </summary>
        private IEnumerator PlayBranchRoutine()
        {
            IsRunning = true;
            BranchSucceed = false;
            _currentTask = _root;

            while (_currentTask != null)
            {
                yield return _currentTask.StartTask();

                if (_currentTask.Succeed)
                {
                    var newOut = _currentTask.GetCurrentOut();
                    var next = _currentTask.GetConnectionSafe(newOut);

                    if (next == null)
                    {
                        BranchSucceed = true;
                        break;
                    }

                    _currentTask = next;
                }
                else
                {
                    BranchSucceed = false;
                    break;
                }
            }

            IsRunning = false;
        }
    }
}
