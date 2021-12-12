using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTNode : Node
    {
        private struct BTNodePort { }

        #region Fields

        private NodeTaskProvider _taskProvider;

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
            outputContainer.AddToClassList("portContainer");

            if (!IsEntry)
            {
                _taskProvider = new NodeTaskProvider(Data);
                _taskProvider.OnTaskChanged += TaskChanged;
                mainContainer.Insert(1, _taskProvider);
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
            RefreshExpandedState();
        }

        private void CreateInput()
        {
            if (IsEntry) return;
            var port = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(float));
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
                var port = InstantiatePort(Orientation.Vertical, Direction.Output, isMultiout ? Port.Capacity.Multi : Port.Capacity.Single, typeof(float));
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
