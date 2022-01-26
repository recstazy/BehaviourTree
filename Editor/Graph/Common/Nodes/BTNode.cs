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
        private static event Action OnValidateConnections;

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

        public static void ValidateOuts()
        {
            OnValidateConnections?.Invoke();
        }

        public BTNode() : base() { }

        public BTNode(NodeData data) : base()
        {
            Data = data;
            RegisterCallback<DetachFromPanelEvent>(Detached);
            OnValidateConnections += ValidateConnections;
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
            OnValidateConnections -= ValidateConnections;
        }

        public virtual void UpdateEdges() { }
        public virtual void UpdateAllPorts() { }

        protected void UpdatePorts(VisualElement container, IConnectionDescription[] newPorts, Func<IConnectionDescription, Port> createPortAction)
        {
            var ports = container.Query<Port>().Build().ToList();
            if (ports == null) ports = new List<Port>();

            // Remove extra ports
            for (int i = ports.Count - 1; i >= newPorts.Length; i--)
            {
                container.Remove(ports[i]);
            }

            // Add ports wich are lack
            for (int i = ports.Count; i < newPorts.Length; i++)
            {
                var port = createPortAction(newPorts[i]);
                ports.Add(port);
            }

            // Remove all removed from hierarchy ports from list
            ports = ports.Where(p => p.parent == container).OrderBy(p => p.parent.IndexOf(p)).ToList();

            // Update ports info
            for (int i = 0; i < ports.Count; i++)
            {
                ports[i].portType = newPorts[i].PortType;
                ports[i].userData = newPorts[i];
                ports[i].portName = ObjectNames.NicifyVariableName(newPorts[i].PortName);
            }
        }

        protected void Reconnect()
        {
            OnReconnect?.Invoke(this);
        }

        private void Detached(DetachFromPanelEvent evt)
        {
            Dispose();
        }

        protected Port CreateOutputPort(OutputDescription outDesc)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, outDesc.OutType);
            port.portName = ObjectNames.NicifyVariableName(outDesc.Name);
            port.userData = outDesc;
            outputContainer.Add(port);
            return port;
        }

        protected Port CreateInputPort(InputDescription description)
        {
            if (IsEntry) return null;
            bool isExecution = description.ValueType == typeof(ExecutionPin);
            bool isMulti = isExecution;
            var capacity = isMulti ? Port.Capacity.Multi : Port.Capacity.Single;
            var port = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, description.ValueType);
            port.portName = isExecution ? string.Empty : ObjectNames.NicifyVariableName(description.IdName);
            port.userData = description;
            inputContainer.Add(port);

            return port;
        }

        private void ValidateConnections()
        {
            bool shouldReconnect = false;
            var outPorts = outputContainer.Query<Port>().Build().ToList();

            foreach (var outPort in outPorts)
            {
                if (outPort.connected)
                {
                    foreach (var edge in outPort.connections)
                    {
                        bool shouldRemoveConnectionFromData = false;
                        var outDesc = outPort.GetOutDescription();

                        if (edge.input == null || edge.input.parent == null)
                        {
                            shouldRemoveConnectionFromData = true;
                        }
                        else
                        {
                            var inDescription = edge.input.GetInputDescription();
                            bool validNameBinding = inDescription.PortName == outDesc.LastConnectedInput.PortName;
                            bool validTyping = inDescription.PortType == outDesc.LastConnectedInput.PortType;

                            if (!validNameBinding || !validTyping) shouldRemoveConnectionFromData = true;
                        }

                        if (shouldRemoveConnectionFromData)
                        {
                            Data.RemoveConnectionByLastInput(outDesc);
                            shouldReconnect = true;
                        }
                    }
                }
            }

            if (shouldReconnect)
            {
                Reconnect();
            }
        }
    }
}
