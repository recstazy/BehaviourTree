using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System;
using System.Runtime.Serialization;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class TreeValueNode : BTNode
    {
        #region Fields

        #endregion

        #region Properties

        public override bool IsEntry => false;
        public FuncNodeData FuncData { get; private set; }

        #endregion

        public TreeValueNode() : base() { }

        public TreeValueNode(FuncNodeData data) : base(data)
        {
            FuncData = data;
            ImportLayout();
            CreateOuts();
        }

        private void ImportLayout()
        {
            titleContainer.Clear();
            titleContainer.RemoveFromHierarchy();
        }

        public override void EdgesChangedExternally()
        {
            base.EdgesChangedExternally();
            var outs = Data.GetOuts();

            if (outs[0].PortType == typeof(AnyValueType))
            {
                var connection = Data.Connections.FirstOrDefault();

                if (connection.IsValid)
                {
                    var connectedNode = Owner.nodes.ToList()
                        .Select(n => (BTNode)n)
                        .First(n => n.Data.Index == connection.InNode);

                    var input = connectedNode.inputContainer.Query<Port>().ToList()
                        .First(p => p.GetInputDescription().IdName == connection.InName);

                    var valueType = input.portType;
                    var valueInstance = FormatterServices.GetUninitializedObject(valueType);
                    FuncData.FuncImplementation = new TreeValueFunc(valueInstance, valueType);

                    var newOuts = Data.GetOuts().Select(o => (IConnectionDescription)o).ToArray();
                    UpdatePorts(outputContainer, newOuts, (desc) => CreateOutputPort((OutputDescription)desc));
                    Reconnect();
                }
            }
        }

        private void CreateOuts()
        {
            var outs = Data.GetOuts();

            foreach (var o in outs)
            {
                var port = CreateOutputPort(o);
                port.portName = "Connect Me";
            }
        }
    }
}
