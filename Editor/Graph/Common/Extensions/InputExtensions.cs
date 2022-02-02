using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal static class InputExtensions 
    {
        public static List<InputDescription> GetInputs(this NodeData nodeData)
        {
            if (nodeData is TaskNodeData taskData)
            {
                return taskData.TaskImplementation.GetInputs();
            }
            else return new List<InputDescription>();
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
                    var description = new InputDescription(inputObject.ValueType, definition == typeof(Getter<>), f.Name, f.Name);
                    inputs.Add(description);
                }
            }

            return inputs;
        }
    }
}
