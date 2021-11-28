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

        private const KeyCode AltModifier = KeyCode.LeftAlt;
        private const KeyCode DeleteNodesKey = KeyCode.Delete;
        private const KeyCode DeleteNodesKeyAlternative = KeyCode.X;
        private const KeyCode ControlModifier = KeyCode.LeftControl;
        private const KeyCode CopyKey = KeyCode.C;
        private const KeyCode PasteKey = KeyCode.V;
        private const KeyCode DuplicateKey = KeyCode.D;

        private static Dictionary<KeyCode, bool> s_pressStates;
        private Dictionary<Func<bool>, Action> _actions;
        private Func<bool> _currentHotkey;

        private Func<bool> CopyHotkey => () => ControlModifierPressed && s_pressStates[CopyKey];
        private Func<bool> PasteHotkey => () => ControlModifierPressed && s_pressStates[PasteKey];
        private Func<bool> DuplicateHotkey => () => ControlModifierPressed && s_pressStates[DuplicateKey];
        private Func<bool> DeleteHotkey => () => s_pressStates[DeleteNodesKey] || s_pressStates[DeleteNodesKeyAlternative];

        #endregion

        #region Properties

        public static bool ControlModifierPressed => s_pressStates[ControlModifier];
        public static bool AltModifierPressed => s_pressStates[AltModifier];
        public static bool DeleteConnectionPressed => AltModifierPressed;

        public static Action OnDeleteNodes { get; set; }
        public static Action OnCopy { get; set; }
        public static Action OnPaste { get; set; }
        public static Action OnDuplicate { get; set; }

        private bool HotkeyPressedNow => _currentHotkey != null;

        #endregion

        public BTHotkeys()
        {
            s_pressStates = new Dictionary<KeyCode, bool>
            {
                { ControlModifier, false },
                { AltModifier, false },
                { DeleteNodesKey, false },
                { DeleteNodesKeyAlternative, false },
                { CopyKey, false },
                { PasteKey, false },
                { DuplicateKey, false },
            };

            _actions = new Dictionary<Func<bool>, Action>
            {
                { DeleteHotkey, OnDeleteNodes },
                { CopyHotkey, OnCopy },  
                { PasteHotkey, OnPaste },
                { DuplicateHotkey, OnDuplicate },
            };
        }

        public void Dispose()
        {
            OnDeleteNodes = null;
            OnCopy = null;
            OnPaste = null;
            OnDuplicate = null;
            _actions = null;
            s_pressStates = null;
            _currentHotkey = null;
        }

        public bool Process(Event e)
        {
            switch(e.type)
            {
                case EventType.KeyDown:
                    {
                        if (!HotkeyPressedNow)
                        {
                            if (s_pressStates.ContainsKey(e.keyCode))
                            {
                                s_pressStates[e.keyCode] = true;
                                e.Use();
                                return CallActionIfPressed();
                            }
                        }
                        
                        break;
                    }
                case EventType.KeyUp:
                    {
                        if (s_pressStates.ContainsKey(e.keyCode))
                        {
                            s_pressStates[e.keyCode] = false;
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
            foreach (var a in _actions)
            {
                if (a.Key.Invoke())
                {
                    _currentHotkey = a.Key;
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
                if (!_currentHotkey.Invoke())
                {
                    _currentHotkey = null;
                    return;
                }
            }
        }
    }
}
