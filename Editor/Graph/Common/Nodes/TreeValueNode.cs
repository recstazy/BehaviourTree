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

        private PropertyFieldElement _field;
        private const string FuncImplName = "_funcImplementation";

        #endregion

        #region Properties

        public override bool IsEntry => false;
        public FuncNodeData FuncData { get; private set; }
        private BehaviourTree Tree => BTWindow.SharedTree;
        
        #endregion

        public TreeValueNode() : base() { }

        public TreeValueNode(FuncNodeData data) : base(data)
        {
            FuncData = data;
            ImportLayout();
            CreateOuts();
            
            if (FuncData.FuncImplementation is VariableFunc varFunc)
            {
                if (varFunc.VariableType != typeof(AnyValueType))
                {
                    CreatePropertyField();
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_field != null)
            {
                _field.OnChanged -= SetDirty;
            }
        }

        private void SetDirty()
        {
            BTWindow.SetDirty("Change Tree Value");
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
                    var valueInstance = JsonUtility.FromJson("{}", valueType);
                    FuncData.FuncImplementation = new TreeValueFunc(valueInstance, valueType);
                    BTWindow.SetDirty("Set Tree Value Type");

                    var newOuts = Data.GetOuts().Select(o => (IConnectionDescription)o).ToArray();
                    UpdatePorts(outputContainer, newOuts, (desc) => CreateOutputPort((OutputDescription)desc));
                    Reconnect();
                    CreatePropertyField();
                }
            }
        }

        private void CreateOuts()
        {
            var outs = Data.GetOuts();

            foreach (var o in outs)
            {
                var port = CreateOutputPort(o, Port.Capacity.Multi);
                port.portName = "Connect Me";
            }
        }

        private void CreatePropertyField()
        {
            using(var sObject = new SerializedObject(Tree))
            {
                using(var sProp = GetValueProperty(sObject))
                {
                    if (sProp != null)
                    {
                        _field = new PropertyFieldElement();
                        _field.SetProperty(sProp, true);

                        var port = outputContainer.Q<Port>();
                        var container = port.Q<Label>().parent;
                        container.Add(_field);
                        port.style.height = new StyleLength(StyleKeyword.Auto);
                        port.portName = " ";
                        _field.OnChanged += SetDirty;
                    }
                }
            }
        }

        private SerializedProperty GetValueProperty(SerializedObject sObject)
        {
            int dataIndex = -1;

            for (int i = 0; i < Tree.NodeData.FuncData.Length; i++)
            {
                if (Tree.NodeData.FuncData[i].Index == Data.Index)
                {
                    dataIndex = i;
                    break;
                }
            }

            if (dataIndex >= 0)
            {
                var datasArray = sObject.FindProperty("_nodeData._funcData");
                var valueProp = datasArray.GetArrayElementAtIndex(dataIndex).FindPropertyRelative($"{FuncImplName}._value");
                var serValue = (FuncData.FuncImplementation as TreeValueFunc).Value;

                switch (serValue.EnumType)
                {
                    case SerializedValue.ValueType.Bool:
                        return valueProp.FindPropertyRelative("_boolValue");
                    case SerializedValue.ValueType.Float:
                        return valueProp.FindPropertyRelative("_floatValue");
                    case SerializedValue.ValueType.Int:
                        return valueProp.FindPropertyRelative("_intValue");
                    case SerializedValue.ValueType.String:
                        return valueProp.FindPropertyRelative("_stringValue");
                    default:
                        return valueProp.FindPropertyRelative("_complexValue");
                }
            }

            return null;
        }
    }
}
