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

        #endregion

        public BTNode() { }

        public BTNode(NodeData data) : base()
        {
            Data = data;
            transform.position = data.Position;
            title = GetName();
            CreateOutputs();
            outputContainer.AddToClassList("portContainer");

            RefreshExpandedState();
        }

        protected string GetName()
        {
            return Data.TaskImplementation == null ? "Empty Task" : ObjectNames.NicifyVariableName(Data.TaskImplementation.GetType().Name);
        }

        private void CreateOutputs()
        {
            var outputs = Data.GetOuts();

            foreach (var o in outputs)
            {
                var port = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(BTNodePort));
                port.portName = o.Name;
                outputContainer.Add(port);
            }
        }
    }
}
