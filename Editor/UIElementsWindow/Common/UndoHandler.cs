using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class UndoHandler
    {
        private const string UndoCommandName = "UndoRedoPerformed";

        public void OnGUI()
        {
            if (Event.current.type == EventType.ValidateCommand)
            {
                if (Event.current.commandName == UndoCommandName)
                {
                    BTWindow.UndoRedoPreformed();
                    Event.current.Use();
                }
            }
        }
    }
}
