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
                topContainer.Insert(1, _taskContainer);
            }

            UpdateTaskDependencies();
        }

        protected string GetName()
        {
            return Data.TaskImplementation == null ? "Empty Task" : ObjectNames.NicifyVariableName(Data.TaskImplementation.GetType().Name);
        }

        private void UpdateTaskDependencies()
        {
            if (outputContainer.childCount > 0) outputContainer.Clear();
            title = GetName();
            CreateOutputs();
            _taskContainer?.SetData(Data);
            RefreshExpandedState();
        }

        private void CreateInput()
        {
            if (IsEntry) return;
            var port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            port.portName = string.Empty;
            inputContainer.Add(port);
        }

        private void CreateOutputs()
        {
            var outputs = Data.GetOuts();
            if (outputs == null || outputs.Length == 0) return;

            foreach (var o in outputs)
            {
                bool isMultiout = Data.TaskImplementation != null && Data.TaskImplementation is MultioutTask;
                var port = InstantiatePort(Orientation.Horizontal, Direction.Output, isMultiout ? Port.Capacity.Multi : Port.Capacity.Single, typeof(float));
                port.portName = o.Name;
                outputContainer.Add(port);
            }
        }

        private void TaskChanged()
        {
            Data.TaskImplementation = TaskFactory.CreateTaskImplementationEditor(_taskProvider.CurrentIndex);
            BTWindow.SetDirty("Change Task");
            UpdateTaskDependencies();
        }
    }
}
