using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal static class InputExtensions 
    {
        public static Type GetValueType(this InputBase input)
        {
            var type = input.GetType();

            if (type.IsGenericType)
            {
                var typeArgument = type.GetGenericArguments()[0];
                return typeArgument;
            }

            return null;
        }

        public static List<InputDescription> GetInputs(this BehaviourTask task)
        {
            var inputs = new List<InputDescription>();
            if (task == null) return inputs;

            var type = task.GetType();
            var serializedFields = type.GetSerializedFieldsUpToBase();

            foreach (var f in serializedFields)
            {
                if (f.FieldType.IsSubclassOf(typeof(InputBase)))
                {
                    var inputObject = (InputBase)f.GetValue(task);
                    var definition = f.FieldType.GetGenericTypeDefinition();
                    var description = new InputDescription(inputObject.ValueType, definition == typeof(InputGet<>), f.Name);
                    inputs.Add(description);
                }
            }

            return inputs;
        }
    }
}
