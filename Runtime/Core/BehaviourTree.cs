using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Asset containing all the behaviour tree logic
    /// </summary>
    [CreateAssetMenu(menuName = "Behaviour Tree/New Behaviour Tree", fileName = "NewBehaviourTree", order = 131)]
    public sealed class BehaviourTree : ScriptableObject, IBlackboardProvider
    {
        #region Fields

        [SerializeField]
        private Blackboard blackboard;

        [SerializeField]
        private Vector2 graphPosition;

        [SerializeField]
        private float zoom = 1f;

        [SerializeField]
        private TreeNodeData nodeData;

        #endregion

        #region Properties

        /// <summary> Is this tree a runtime instance </summary>
        public bool IsRuntime { get; private set; } = false;
        public Blackboard Blackboard => blackboard;

        internal TreeNodeData NodeData { get => nodeData; set => nodeData = value; }
        internal Vector2 GraphPosition { get => graphPosition; set => graphPosition = value; }
        internal float Zoom { get => zoom; set => zoom = value; }
        internal NodeData EntryNode => nodeData?.Data is null || nodeData.Data.Length == 0 ? null : nodeData.Data[0];

        #endregion

        internal void CreateEntry()
        {
            if (nodeData != null && nodeData.Data != null && nodeData.Data.Length == 0)
            {
                nodeData.Data = new NodeData[] { new NodeData(0, new EntryTask()) };
            }
        }

        [RuntimeInstanced]
        internal BehaviourTree CreateRuntimeImplementation(CoroutineRunner coroutineRunner)
        {
            if (Blackboard is null)
            {
                Debug.LogError($"No blackboard set for \"{name}\" tree");
            }

            var instance = Instantiate(this);
            instance.InitializeRuntime(coroutineRunner);
            instance.CreateRuntimeConnections();
            instance.IsRuntime = true;
            return instance;
        }

        [RuntimeInstanced]
        private void InitializeRuntime(CoroutineRunner coroutineRunner)
        {
            blackboard = Instantiate(blackboard);
            blackboard.InitializeAtRuntime(coroutineRunner.gameObject);
            var runtimeNodeData = nodeData.Data;

            for (int i = 0; i < runtimeNodeData.Length; i++)
            {
                if (runtimeNodeData[i].TaskImplementation != null)
                {
                    runtimeNodeData[i].TaskImplementation.Blackboard = Blackboard;
                    runtimeNodeData[i].TaskImplementation.SetCoroutineRunner(coroutineRunner);
                }
            }
        }

        [RuntimeInstanced]
        private void CreateRuntimeConnections()
        {
            foreach (var d in nodeData.Data)
            {
                d.CreateRuntimeConnections(nodeData.Data);
            }
        }
    }
}
