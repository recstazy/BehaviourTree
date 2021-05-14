using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal class BTHotkeys
    {
        #region Fields

        private const KeyCode altModifier = KeyCode.LeftAlt;
        private const KeyCode deleteNodesKey = KeyCode.Delete;
        private const KeyCode controlModifier = KeyCode.LeftControl;
        private const KeyCode copyKey = KeyCode.C;
        private const KeyCode pasteKey = KeyCode.V;
        private const KeyCode duplicateKey = KeyCode.D;

        private static Dictionary<KeyCode, bool> pressStates;
        private Dictionary<Func<bool>, Action> actions;
        private Func<bool> currentHotkey;

        private Func<bool> copyHotkey => () => ControlModifierPressed && pressStates[copyKey];
        private Func<bool> pasteHotkey => () => ControlModifierPressed && pressStates[pasteKey];
        private Func<bool> duplicateHotkey => () => ControlModifierPressed && pressStates[duplicateKey];
        private Func<bool> deleteHotkey => () => pressStates[deleteNodesKey];

        #endregion

        #region Properties

        public static bool ControlModifierPressed => pressStates[controlModifier];
        public static bool AltModifierPressed => pressStates[altModifier];
        public static bool DeleteConnectionPressed => AltModifierPressed;

        public static Action OnDeleteNodes { get; set; }
        public static Action OnCopy { get; set; }
        public static Action OnPaste { get; set; }
        public static Action OnDuplicate { get; set; }

        private bool HotkeyPressedNow => currentHotkey != null;

        #endregion

        public BTHotkeys()
        {
            pressStates = new Dictionary<KeyCode, bool>
            {
                { controlModifier, false },
                { altModifier, false },
                { deleteNodesKey, false },
                { copyKey, false },
                { pasteKey, false },
                { duplicateKey, false },
            };

            actions = new Dictionary<Func<bool>, Action>
            {
                { deleteHotkey, OnDeleteNodes },
                { copyHotkey, OnCopy },  
                { pasteHotkey, OnPaste },
                { duplicateHotkey, OnDuplicate },
            };
        }

        public void Dispose()
        {
            OnDeleteNodes = null;
            OnCopy = null;
            OnPaste = null;
            OnDuplicate = null;
            actions = null;
            pressStates = null;
            currentHotkey = null;
        }

        public bool Process(Event e)
        {
            switch(e.type)
            {
                case EventType.KeyDown:
                    {
                        if (!HotkeyPressedNow)
                        {
                            if (pressStates.ContainsKey(e.keyCode))
                            {
                                pressStates[e.keyCode] = true;
                                e.Use();
                                return CallActionIfPressed();
                            }
                        }
                        
                        break;
                    }
                case EventType.KeyUp:
                    {
                        if (pressStates.ContainsKey(e.keyCode))
                        {
                            pressStates[e.keyCode] = false;
                            e.Use();
                            CheckLastActionReleased();
                        }

                        break;
                    }
            }

            return false;
        }

        private bool CallActionIfPressed()
        {
            foreach (var a in actions)
            {
                if (a.Key.Invoke())
                {
                    currentHotkey = a.Key;
                    a.Value?.Invoke();
                    return true;
                }
            }

            return false;
        }

        private void CheckLastActionReleased()
        {
            if (HotkeyPressedNow)
            {
                if (!currentHotkey.Invoke())
                {
                    currentHotkey = null;
                    return;
                }
            }
        }
    }
}
