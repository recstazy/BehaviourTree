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

            UpdateAllPorts();
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

        public override void UpdateAllPorts()
        {
            base.UpdateAllPorts();
            var newOuts = Data.GetOuts();
            if (newOuts == null) return;

            if (Data.Connections.Length != newOuts.Length)
            {
                Data.SetConnections(Data.Connections.Take(Mathf.Min(Data.Connections.Length, newOuts.Length)).ToArray());
                newOuts = Data.GetOuts();
            }

            UpdatePorts(outputContainer, newOuts.Select(o => (IConnectionDescription)o).ToArray(), (desc) => CreateOutputPort((OutputDescription)desc));

            if (!IsEntry)
            {
                var newInputs = new List<IConnectionDescription> { InputDescription.ExecutionInput }
                .Concat(TaskData.TaskImplementation.GetInputs().Select(i => (IConnectionDescription)i))
                .ToArray();

                UpdatePorts(inputContainer, newInputs, (desc) => CreateInputPort((InputDescription)desc));
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

        private void TaskChanged()
        {
            TaskData.TaskImplementation = TaskFactory.CreateTaskImplementationEditor(_taskProvider.CurrentIndex);
            BTWindow.SetDirty("Change Task");
            UpdateTaskDependencies();
            UpdateEdges();
            ValidateOuts();
        }
    }
}
