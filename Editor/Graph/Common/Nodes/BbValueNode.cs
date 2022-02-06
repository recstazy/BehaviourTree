using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BbValueNode : BTNode
    {
        #region Fields

        #endregion

        #region Properties

        public override bool IsEntry => false;

        #endregion

        public BbValueNode() : base() { }

        public BbValueNode(FuncNodeData data) : base(data)
        {
            ImportLayout();
            CreateOuts();
            ValidateBbValue(data);
        }

        private void ImportLayout()
        {
            titleContainer.Clear();
            titleContainer.RemoveFromHierarchy();
        }

        private void CreateOuts()
        {
            var outs = Data.GetOuts();

            foreach (var o in outs)
            {
                CreateOutputPort(o);
            }
        }

        private void ValidateBbValue(FuncNodeData data)
        {
            var func = data.FuncImplementation as BbValueFunc;

            if (func != null && !string.IsNullOrEmpty(func.VariableName))
            {
                bool? isValidName = BTWindow.SharedTree.Blackboard?.GetterValues.TryGetValue(func?.VariableName, out var accessor);

                if (isValidName.HasValue && isValidName.Value == true) return;
            }

            AddErrorHighlight(this);
        }
    }
}
