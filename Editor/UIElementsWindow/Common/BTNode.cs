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
    internal class BTNode : Node
    {
        private static event Action OnAnyDeleted;

        #region Fields

        private NodeTaskProvider _taskProvider;
        private TaskContainer _taskContainer;

        #endregion

        #region Properties

        public int TaskTypeIndex => _taskProvider != null ? _taskProvider.CurrentIndex : -1;
        public NodeData Data { get; private set; }
        public bool IsEntry { get; private set; }

        #endregion

        public static void AnyNodeDeleted()
        {
            OnAnyDeleted?.Invoke();
        }

        public BTNode() { }

        public BTNode(NodeData data) : base()
        {
            Data = data;
            IsEntry = Data.TaskImplementation is EntryTask;
            CreateInput();

            if (!IsEntry)
            {
                _taskProvider = new NodeTaskProvider(Data);
                _taskProvider.OnTaskChanged += TaskChanged;
                var titleElement = titleContainer.Children().First();
                titleContainer.Remove(titleElement);
                titleContainer.Insert(0, _taskProvider);
                titleContainer.AddToClassList("title-container");
                titleContainer.style.height = 25;

                _taskContainer = new TaskContainer();
                UpdateTaskContainer();
                extensionContainer.Add(_taskContainer);
            }

            UpdateTaskDependencies();
            RegisterCallback<DetachFromPanelEvent>(Detached);
            OnAnyDeleted += UpdateTaskContainer;
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

        public void EdgesChanged()
        {
            var newOuts = Data.GetOuts();
            if (newOuts == null) return;

            if (Data.Connections.Length != newOuts.Length)
            {
                Data.SetConnections(Data.Connections.Take(Mathf.Min(Data.Connections.Length, newOuts.Length)).ToArray());
            }

            OutsOrderChanged();
            var sortedPorts = outputContainer.Query<Port>().Build().ToList();
            if (sortedPorts == null) sortedPorts = new List<Port>();

            // Remove extra ports
            for (int i = sortedPorts.Count - 1; i >= newOuts.Length; i--)
            {
                var edges = sortedPorts[i].connections.ToArray();

                for (int j = 0; j < edges.Length; j++)
                {
                    var edge = edges[j];
                    edge.input.Disconnect(edge);
                    edge.output.Disconnect(edge);
                    edge.parent.Remove(edge);
                }

                outputContainer.Remove(sortedPorts[i]);
            }

            // Add ports wich are lack
            for (int i = sortedPorts.Count; i < newOuts.Length; i++)
            {
                CreateOutputPort(newOuts[i]);
            }

            sortedPorts = sortedPorts.Where(p => p.parent == outputContainer).ToList();
            // Update ports info
            for (int i = 0; i < sortedPorts.Count; i++)
            {
                sortedPorts[i].userData = newOuts[i].Index;
                sortedPorts[i].portName = newOuts[i].Name;
            }
        }

        public void OutsOrderChanged()
        {
            var sortedPorts = outputContainer.Query<Port>().Build().ToList().OrderBy(p => (int)p.userData).ToArray();

            foreach (var p in sortedPorts)
            {
                p.BringToFront();
            }
        }

        public void WasDeleted()
        {
            Dispose();
        }

        protected string GetName()
        {
            return Data.TaskImplementation == null ? "Empty Task" : ObjectNames.NicifyVariableName(Data.TaskImplementation.GetType().Name);
        }

        private void Detached(DetachFromPanelEvent evt)
        {
            Dispose();
        }

        private void Dispose()
        {
            UnregisterCallback<DetachFromPanelEvent>(Detached);
            OnAnyDeleted -= UpdateTaskContainer;
            _taskContainer?.Dispose();
            if (_taskProvider != null) _taskProvider.OnTaskChanged -= TaskChanged;
            _taskProvider = null;
        }

        private void UpdateTaskContainer()
        {
            _taskContainer?.SetData(Data);
        }

        private void UpdateTaskDependencies()
        {
            EdgesChanged();
            title = GetName();
            UpdateTaskContainer();
            RefreshExpandedState();
        }

        private void CreateInput()
        {
            if (IsEntry) return;
            var port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            port.portName = string.Empty;
            port.userData = 0;
            inputContainer.Add(port);
        }

        private void TaskChanged()
        {
            Data.TaskImplementation = TaskFactory.CreateTaskImplementationEditor(_taskProvider.CurrentIndex);
            BTWindow.SetDirty("Change Task");
            UpdateTaskDependencies();
        }

        private void CreateOutputPort(TaskOutAttribute outAttribute)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            port.portName = outAttribute.Name;
            port.userData = outAttribute.Index;
            outputContainer.Add(port);
        }
    }
}
