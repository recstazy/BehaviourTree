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
        private BlackboardName _a;

        [SerializeField]
        private Operation _operation;

        [SerializeField]
        private float _b;

        [SerializeField]
        [ValueType(typeof(FloatValue))]
        private BlackboardName _output;

        [SerializeField]
        private bool _swapOrder;

        private const string Plus = "+";
        private const string Minus = "-";
        private const string Mul = "*";
        private const string Div = "/";

        #endregion

        public override string GetDescription()
        {
            var aString = _a.ToString();
            var bString = _b.ToString();
            return $"{_output} = {(_swapOrder ? bString : aString)} {GetOperationString()} {(_swapOrder ? aString : bString)}";
        }

        protected override IEnumerator TaskRoutine()
        {
            if (Blackboard.TryGetValue(_a, out FloatValue value))
            {
                float aValue = value.Value;

                switch (_operation)
                {
                    case Operation.Add:
                        aValue += _b;
                        break;
                    case Operation.Subtract:
                        aValue = _swapOrder ? _b - aValue : aValue - _b;
                        break;
                    case Operation.Multiply:
                        aValue *= _b;
                        break;
                    case Operation.Divide:
                        aValue = _swapOrder ? _b / aValue : aValue / _b;
                        break;
                    default:
                        break;
                }

                Blackboard.SetValue(_output, (FloatValue)aValue);
            }

            yield break;
        }

        private string GetOperationString()
        {
            switch (_operation)
            {
                case Operation.Add:
                    return Plus;
                case Operation.Subtract:
                    return Minus;
                case Operation.Multiply:
                    return Mul;
                case Operation.Divide:
                    return Div;
                default:
                    return "[Get Operation Error]";
            }
        }
    }
}
