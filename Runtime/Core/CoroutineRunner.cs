using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    /// <summary>
    /// Dummy class to run tasks' coroutines on
    /// </summary>
    public class CoroutineRunner : MonoBehaviour 
    {
        public event System.Action OnInstanced;
        public event System.Action OnDestroyed;

        private void Awake()
        {
            OnInstanced?.Invoke();
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }
}
