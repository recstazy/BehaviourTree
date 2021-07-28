using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    [TaskOut(0)]
    [TaskMenu("Value/Math")]
    public class Math : BehaviourTask
    {
        public enum Operation { Add, Subtract, Multiply, Divide }

        #region Fields

        [SerializeField]
        [ValueType(typeof(FloatValue))]
        private BlackboardName a;

        [SerializeField]
        private Operation operation;

        [SerializeField]
        private float b;

        [SerializeField]
        [ValueType(typeof(FloatValue))]
        private BlackboardName output;

        [SerializeField]
        private bool swapOrder;

        private const string plus = "+";
        private const string minus = "-";
        private const string mul = "*";
        private const string div = "/";

        #endregion

        public override string GetDescription()
        {
            var aString = a.ToString();
            var bString = b.ToString();
            return $"{output} = {(swapOrder ? bString : aString)} {GetOperationString()} {(swapOrder ? aString : bString)}";
        }

        protected override IEnumerator TaskRoutine()
        {
            if (Blackboard.TryGetValue(a, out FloatValue value))
            {
                float aValue = value.Value;

                switch (operation)
                {
                    case Operation.Add:
                        aValue += b;
                        break;
                    case Operation.Subtract:
                        aValue = swapOrder ? b - aValue : aValue - b;
                        break;
                    case Operation.Multiply:
                        aValue *= b;
                        break;
                    case Operation.Divide:
                        aValue = swapOrder ? b / aValue : aValue / b;
                        break;
                    default:
                        break;
                }

                Blackboard.SetValue(output, (FloatValue)aValue);
            }

            yield break;
        }

        private string GetOperationString()
        {
            switch (operation)
            {
                case Operation.Add:
                    return plus;
                case Operation.Subtract:
                    return minus;
                case Operation.Multiply:
                    return mul;
                case Operation.Divide:
                    return div;
                default:
                    return "[Get Operation Error]";
            }
        }
    }
}
