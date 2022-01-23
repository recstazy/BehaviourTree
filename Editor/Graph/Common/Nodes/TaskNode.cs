using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class TaskNode : BTNode
    {
        #region Fields

        private NodeTaskProvider _taskProvider;
        private TaskContainer _taskContainer;
        private bool _isEntry;

        #endregion

        #region Properties

        public int TaskTypeIndex => _taskProvider != null ? _taskProvider.CurrentIndex : -1;
        public override bool IsEntry => _isEntry;
        public TaskNodeData TaskData { get; private set; }

        #endregion

        public TaskNode() : base() { }

        public TaskNode(TaskNodeData data) : base(data)
        {
            TaskData = data;
            _isEntry = TaskData.TaskImplementation is EntryTask;
            CreateInputs();

            if (!IsEntry)
            {
                _taskProvider = new NodeTaskProvider(TaskData);
                _taskProvider.OnTaskChanged += TaskChanged;
                var titleElement = titleContainer.Children().First();
                titleContainer.Remove(titleElement);
                titleContainer.Insert(0, _taskProvider);
                titleContainer.AddToClassList("node-title-container");
                titleContainer.style.height = 25;

                _taskContainer = new TaskContainer();
                UpdateTaskContainer();
                extensionContainer.Add(_taskContainer);
            }

            UpdateTaskDependencies();
            OnAnyDeleted += UpdateTaskContainer;
        }

        public override void Dispose()
        {
            base.Dispose();
            OnAnyDeleted -= UpdateTaskContainer;
            _taskContainer?.Dispose();
            extensionContainer.Clear();
            if (_taskProvider != null) _taskProvider.OnTaskChanged -= TaskChanged;
            _taskProvider = null;
            titleContainer.Clear();
        }

        public override void UpdateEdges()
        {
            base.UpdateEdges();
            Reconnect();
        }

        public override void UpdateOutPorts()
        {
            base.UpdateOutPorts();
            var newOuts = Data.GetOuts();
            if (newOuts == null) return;

            if (Data.Connections.Length != newOuts.Length)
            {
                Data.SetConnections(Data.Connections.Take(Mathf.Min(Data.Connections.Length, newOuts.Length)).ToArray());
                newOuts = Data.GetOuts();
            }

            var ports = outputContainer.Query<Port>().Build().ToList();
            if (ports == null) ports = new List<Port>();

            // Remove extra ports
            for (int i = ports.Count - 1; i >= newOuts.Length; i--)
            {
                outputContainer.Remove(ports[i]);
            }

            // Add ports wich are lack
            for (int i = ports.Count; i < newOuts.Length; i++)
            {
                var port = CreateOutputPort(newOuts[i]);
                ports.Add(port);
            }

            // Remove all removed from hierarchy ports from list
            ports = ports.Where(p => p.parent == outputContainer).OrderBy(p => p.parent.IndexOf(p)).ToList();

            // Update ports info
            for (int i = 0; i < ports.Count; i++)
            {
                ports[i].userData = newOuts[i].Index;
                ports[i].portName = newOuts[i].Name;
            }

            RefreshPorts();
        }

        private string GetName()
        {
            return TaskData.TaskImplementation == null ? "Empty Task" : ObjectNames.NicifyVariableName(TaskData.TaskImplementation.GetType().Name);
        }

        private void UpdateTaskContainer()
        {
            _taskContainer?.SetData(TaskData);
        }

        private void UpdateTaskDependencies()
        {
            title = GetName();
            UpdateTaskContainer();
            RefreshExpandedState();
        }

        private void CreateInputs()
        {
            if (IsEntry) return;
            // Create Execution input
            CreateInputPort(InputDescription.ExecutionInput);

            // Create Value inputs
            var inputs = TaskData.TaskImplementation.GetInputs();

            foreach (var i in inputs)
            {
                CreateInputPort(i);
            }

            RefreshPorts();
        }

        private void TaskChanged()
        {
            TaskData.TaskImplementation = TaskFactory.CreateTaskImplementationEditor(_taskProvider.CurrentIndex);
            BTWindow.SetDirty("Change Task");
            UpdateTaskDependencies();
            UpdateEdges();
        }
    }
}
