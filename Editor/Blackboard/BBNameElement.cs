using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Recstazy.BehaviourTree.EditorScripts
{
    [CustomPropertyElement(typeof(BlackboardGetter))]
    public class BBNameElement : BasePropertyFieldElement
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        protected override void CreateVisualElements(SerializedProperty property)
        {
            FieldsContainer.Add(new Label("Your BB Name Here"));
        }
    }
}
