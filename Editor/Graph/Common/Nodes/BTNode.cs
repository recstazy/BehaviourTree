using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal abstract class BTNode : Node, IPlaymodeDependent
    {
        public event Action<BTNode> OnReconnect;
        protected static event Action OnAnyDeleted;

        #region Fields

        #endregion

        #region Properties

        public NodeData Data { get; private set; }
        public abstract bool IsEntry { get; }

        #endregion

        public static void AnyNodeDeleted()
        {
            OnAnyDeleted?.Invoke();
        }

        public BTNode() : base() { }

        public BTNode(NodeData data) : base()
        {
            Data = data;
            RegisterCallback<DetachFromPanelEvent>(Detached);
        }

        public void ApplyPositionFromData()
        {
            var currentRect = GetPosition();
            currentRect.position = Data.Position;
            SetPosition(currentRect);
        }

        public Vector2 GetWorldPosition()
        {
            var currentRect = GetPosition();
            return currentRect.position;
        }

        public virtual void PlaymodeChanged(bool isPlaymode)
        {
            SetEnabled(!isPlaymode);
        }

        public virtual void WasDeleted()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            UnregisterCallback<DetachFromPanelEvent>(Detached);
        }

        public virtual void UpdateEdges() { }
        public virtual void UpdateOutPorts() { }

        protected void Reconnect()
        {
            OnReconnect?.Invoke(this);
        }

        private void Detached(DetachFromPanelEvent evt)
        {
            Dispose();
        }

        protected Port CreateOutputPort(TaskOutDescription outDesc)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, outDesc.OutType);
            port.portName = outDesc.Name;
            port.userData = outDesc.Index;
            outputContainer.Add(port);
            return port;
        }
    }
}
