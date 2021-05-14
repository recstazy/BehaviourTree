using UnityEngine;
using System;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Use this to show blackboard name selector in editor
    /// </summary>
    [Serializable]
    public class BlackboardName
    {
        #region Fields

        [SerializeField]
        private string name;

        #endregion

        #region Properties

        public string Name => name;

        #endregion

        public override string ToString()
        {
            return name;
        }

        public static implicit operator string(BlackboardName value) => value is null || value.name is null ? string.Empty : value.name;
        public static implicit operator BlackboardName(string value) => new BlackboardName() { name = value };
    }
}
