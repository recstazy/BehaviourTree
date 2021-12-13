using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTNode : Node
    {
        private struct BTNodePort { }

        #region Fields

        private NodeTaskProvider _taskProvider;
        private TaskContainer _taskContainer;

        #endregion

        #region Properties
	
        public NodeData Data { get; private set; }
        public bool IsEntry { get; private set; }

        #endregion

        public BTNode() { }

        public BTNode(NodeData data) : base()
        {
            Data = data;
            IsEntry = Data.TaskImplementation is EntryTask;

            var currentRect = GetPosition();
            currentRect.position = data.Position;
            SetPosition(currentRect);
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
                _taskContainer.SetData(Data);
                extensionContainer.Add(_taskContainer);
            }

            UpdateTaskDependencies();
        }

        public void EdgesChanged()
        {
            var newOuts = Data.GetOuts();
            if (newOuts == null || newOuts.Length == 0) return;
            OutsOrderChanged();
            var sortedPorts = outputContainer.Query<Port>().Build().ToList();
            if (sortedPorts == null) sortedPorts = new List<Port>();

            // Remove extra ports
            for (int i = sortedPorts.Count - 1; i >= newOuts.Length; i--)
            {
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

        protected string GetName()
        {
            return Data.TaskImplementation == null ? "Empty Task" : ObjectNames.NicifyVariableName(Data.TaskImplementation.GetType().Name);
        }

        private void UpdateTaskDependencies()
        {
            EdgesChanged();
            title = GetName();
            _taskContainer?.SetData(Data);
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

        private void CreateOutputs()
        {
            var outputs = Data.GetOuts();
            if (outputs == null || outputs.Length == 0) return;

            foreach (var o in outputs)
            {
                CreateOutputPort(o);
            }
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
