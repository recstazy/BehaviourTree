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

        #endregion

        #region Properties
	
        public NodeData Data { get; private set; }
        public bool IsEntry { get; private set; }

        #endregion

        public BTNode() { }

        public BTNode(NodeData data) : base()
        {
            Data = data;
            IsEntry = Data?.TaskImplementation is EntryTask;

            var currentRect = GetPosition();
            currentRect.position = data.Position;
            SetPosition(currentRect);
            title = GetName();
            CreateInput();
            CreateOutputs();
            outputContainer.AddToClassList("portContainer");

            if (!IsEntry)
            {
                var taskProvider = new NodeTaskProvider(Data);
                mainContainer.Insert(1, taskProvider);
            }

            RefreshExpandedState();
        }

        protected string GetName()
        {
            return Data.TaskImplementation == null ? "Empty Task" : ObjectNames.NicifyVariableName(Data.TaskImplementation.GetType().Name);
        }

        private void CreateInput()
        {
            if (IsEntry) return;

            var port = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(float));
            port.portName = string.Empty;
            inputContainer.Add(port);
        }

        private void CreateOutputs()
        {
            var outputs = Data.GetOuts();
            if (outputs == null || outputs.Length == 0) return;

            foreach (var o in outputs)
            {
                var port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(float));
                port.portName = o.Name;
                outputContainer.Add(port);
            }
        }
    }
}
