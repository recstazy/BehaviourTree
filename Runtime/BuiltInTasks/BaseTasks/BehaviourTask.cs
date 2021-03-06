using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Base class to derive to make your own tasks
    /// </summary>
    [TaskOut]
    [System.Serializable]
    public class BehaviourTask : INodeImplementation
    {
        public event System.Action<BehaviourTask> OnStarted;
        public event System.Action<BehaviourTask> OnFinished;

        #region Fields

        /// <summary> Return -1 in <c>GetCurrentOutIndex</c> to tell player that no out can be selected to go further </summary>
        public const int NoOut = -1;

        [System.NonSerialized]
        private List<BehaviourTask> _connections;

        [System.NonSerialized]
        private Blackboard _blackboard;

        private Coroutine _taskBodyRoutine;
        private bool _taskBodyIsRunning;
        private HashSet<Coroutine> _currentTaskCoroutines = new HashSet<Coroutine>();
        private HashSet<BranchPlayer> _currentTaskBranches = new HashSet<BranchPlayer>();

        #endregion

        #region Properties

        /// <summary> Tasks connected to this task's outs, only exists in runtime </summary>
        public List<BehaviourTask> Connections { get => _connections; set => _connections = value; }

        /// <summary> Was the task execution successfull? True by default </summary>
        public bool Succeed { get; protected set; } = true;

        public bool IsRunning { get; private set; }
        public Blackboard Blackboard { get => GetBlackboard(); set => SetBlackboard(value); }

        internal CoroutineRunner CoroutineRunner { get; private set; }
        internal int Index { get; set; }

        #endregion

        /// <summary> Ask this task to provide out index to go further </summary>
        public int GetCurrentOut()
        {
            int nextOutIndex = GetNextOutIndex();
            return nextOutIndex;
        }

        /// <summary> Finish task without waiting to it's end and force to succeed or fail </summary>
        /// <param name="shouldSucceed">Should task be marked as successful after finishing</param>
        public void ForceFinishTask(bool shouldSucceed)
        {
            StopCoroutineSafe(_taskBodyRoutine);
            _taskBodyIsRunning = false;
            AfterBodyFinished();
            Succeed = shouldSucceed;
        }

        /// <summary> Called when task was initialized while Behaviour Tree is initializing in runtime </summary>
        protected virtual void Initialized() { }

        /// <summary> What out index to choose after task completion </summary>
        protected virtual int GetNextOutIndex()
        {
            return 0;
        }

        /// <summary> Coroutine which will be executed when task is running </summary>
        protected virtual IEnumerator TaskRoutine()
        {
            Succeed = true;
            yield return null;
        }

        /// <summary> Start coroutine as MonoBehaviours do </summary>
        protected Coroutine StartCoroutine(IEnumerator coroutine)
        {
            var newCoroutine = CoroutineRunner.StartCoroutine(coroutine);
            _currentTaskCoroutines.Add(newCoroutine);
            return newCoroutine;
        }

        /// <summary> Check for null and stop coroutine </summary>
        protected void StopCoroutineSafe(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                if (_currentTaskCoroutines.Contains(coroutine))
                {
                    _currentTaskCoroutines.Remove(coroutine);
                }

                CoroutineRunner.StopCoroutine(coroutine);
            }
        }

        /// <summary> Stop all coroutines started in this task </summary>
        protected void StopAllCoroutines()
        {
            foreach (var c in _currentTaskCoroutines)
            {
                if (c != null)
                {
                    CoroutineRunner.StopCoroutine(c);
                }
            }

            _currentTaskCoroutines.Clear();
        }

        /// <summary> Start executing new branch starting from connected out task with index </summary>
        /// <param name="outIndex"> Out index of this task to use as branch root node </param>
        protected BranchPlayer PlayConnectedBranch(int outIndex)
        {
            var root = GetConnectionSafe(outIndex);

            if (root != null)
            {
                var branch = new BranchPlayer(root);
                _currentTaskBranches.Add(branch);
                branch.Start();
                return branch;
            }

            return null;
        }

        /// <summary> Stop running branch if it was started from this task </summary>
        protected void StopBranch(BranchPlayer branch)
        {
            if (_currentTaskBranches.Contains(branch))
            {
                _currentTaskBranches.Remove(branch);
                branch.Stop();
            }
        }

        /// <summary> Stop all branches started on this task </summary>
        protected void StopAllBranches()
        {
            foreach (var b in _currentTaskBranches)
            {
                b.Stop();
            }

            _currentTaskBranches.Clear();
        }

        private Blackboard GetBlackboard()
        {
            return _blackboard;
        }

        private void SetBlackboard(Blackboard value)
        {
            if (Application.isPlaying)
            {
                _blackboard = value;
            }
        }

        private IEnumerator TaskBodyCoroutine()
        {
            yield return TaskRoutine();
            _taskBodyIsRunning = false;
        }

        private void AfterBodyFinished()
        {
            StopAllCoroutines();
            StopAllBranches();
            _taskBodyRoutine = null;
            IsRunning = false;
            OnFinished?.Invoke(this);
        }

        internal void Initialize()
        {
            Initialized();
        }

        /// <summary> Get actual task connected to out index. Returns null if index out of bounds </summary>
        internal BehaviourTask GetConnectionSafe(int index)
        {
            if (Connections != null && index >= 0 && index < Connections.Count)
            {
                return Connections[index];
            }

            return null;
        }

        internal IEnumerator StartTask()
        {
            IsRunning = true;
            Succeed = true;
            _taskBodyIsRunning = true;
            OnStarted?.Invoke(this);

            _taskBodyRoutine = CoroutineRunner.StartCoroutine(TaskBodyCoroutine());
            yield return new WaitUntil(() => !_taskBodyIsRunning);
            AfterBodyFinished();
        }

        internal BehaviourTask CreateShallowCopy()
        {
            var taskType = GetType();
            var copy = System.Activator.CreateInstance(taskType);
            var fieldsToCopy = taskType.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                .Where(f => f.GetCustomAttributes(typeof(SerializeField), true).Length > 0)
                .ToArray();

            foreach (var f in fieldsToCopy)
            {
                f.SetValue(copy, f.GetValue(this));
            }

            return copy as BehaviourTask;
        }

        [RuntimeInstanced]
        internal void SetRuntimeConnections(List<BehaviourTask> connections)
        {
            Connections = connections;
        }

        [RuntimeInstanced]
        internal void SetCoroutineRunner(CoroutineRunner runner)
        {
            CoroutineRunner = runner;
        }

        internal virtual TaskConnection[] PostProcessConnectionsAfterChange(TaskConnection[] connections)
        {
            return connections;
        }
    }
}
