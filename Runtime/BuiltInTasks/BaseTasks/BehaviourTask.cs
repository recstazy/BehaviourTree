using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Base class to derive to make your own tasks
    /// </summary>
    [TaskOut(0)]
    [System.Serializable]
    public class BehaviourTask
    {
        #region Fields

        [System.NonSerialized]
        private List<BehaviourTask> connections;

        [System.NonSerialized]
        private Blackboard blackboard;

        protected bool CanRun { get; private set; } = false;
        protected bool ForceSucceedOrFail { get; private set; }

        /// <summary> Return -1 in <c>GetCurrentOutIndex</c> to tell player that no out can be selected to go further </summary>
        public const int NoOut = -1;

        #endregion

        #region Properties

        /// <summary> Tasks connected to this task's outs, only exists in runtime </summary>
        public List<BehaviourTask> Connections { get => connections; set => connections = value; }

        /// <summary> Was the task execution successfull? True by default </summary>
        public bool Succeed { get; protected set; } = true;

        public bool IsRunning { get; private set; }
        public Blackboard Blackboard { get => GetBlackboard(); set => SetBlackboard(value); }

        /// <summary> MonoBehaviour used to run coroutines from tasks </summary>
        protected CoroutineRunner CoroutineRunner { get; private set; }
        internal int LastReturnedOut { get; private set; }

        #endregion

        /// <summary> Ask this task to provide out index to go further </summary>
        public int GetCurrentOut()
        {
            int nextOutIndex = GetCurrentOutIndex();
            LastReturnedOut = nextOutIndex;
            return nextOutIndex;
        }

        /// <summary> What out index to choose after task completion </summary>
        protected virtual int GetCurrentOutIndex()
        {
            return 0;
        }

        /// <summary> Get actual task connected to out index. Returns null if index out of bounds </summary>
        public BehaviourTask GetConnectionSafe(int index)
        {
            if (Connections != null && index >= 0 && index < Connections.Count)
            {
                return Connections[index];
            }

            return null;
        }

        /// <summary> Provide description to show in behaviour tree </summary>
        public virtual string GetDescription()
        {
            return null;
        }

        public void StopImmediate(bool succeed)
        {
            CanRun = false;
            ForceSucceedOrFail = succeed;
        }

        /// <summary> Coroutine which will be executed when task is running </summary>
        protected virtual IEnumerator TaskRoutine()
        {
            Succeed = true;
            yield return null;
        }

        /// <summary> Get connection with index 0 </summary>
        protected BehaviourTask GetFirstConnection()
        {
            return GetConnectionSafe(0);
        }

        /// <summary> Start coroutine as MonoBehaviours do </summary>
        protected Coroutine StartCoroutine(IEnumerator coroutine)
        {
            return CoroutineRunner.StartCoroutine(coroutine);
        }

        /// <summary> Check for null and stop coroutine </summary>
        protected void StopCoroutineSafe(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                CoroutineRunner.StopCoroutine(coroutine);
            }
        }

        /// <summary> Stop all coroutines started on CoroutineRunner attached to this BehaviourPlayer </summary>
        protected void StopAllCoroutines()
        {
            CoroutineRunner.StopAllCoroutines();
        }

        /// <summary> Called if task was canceled externally </summary>
        protected virtual void StoppedExternaly(bool forceSucceed) { }

        private Blackboard GetBlackboard()
        {
            return blackboard;
        }

        private void SetBlackboard(Blackboard value)
        {
            if (Application.isPlaying)
            {
                blackboard = value;
            }
        }

        internal IEnumerator StartTask()
        {
            IsRunning = true;
            CanRun = true;
            var enumerator = TaskRoutine();

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;

                if (!CanRun)
                {
                    StoppedExternaly(ForceSucceedOrFail);
                    Succeed = ForceSucceedOrFail;
                    break;
                }
            }

            IsRunning = false;
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
    }
}
