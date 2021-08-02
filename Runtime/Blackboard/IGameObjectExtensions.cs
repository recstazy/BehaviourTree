using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.BehaviourTree
{
    public static class IGameObjectExtensions
    {
        public static Transform GetTransform(this IGameObjectProvider goProvider)
        {
            return CheckNullGameObject(goProvider) ? goProvider.gameObject.transform : null;
        }

        public static T GetComponent<T>(this IGameObjectProvider goProvider) where T : Component
        {
            if (CheckNullGameObject(goProvider))
            {
                return goProvider.gameObject.GetComponent<T>();
            }

            return null;
        }

        public static T GetComponentInChildren<T>(this IGameObjectProvider goProvider) where T : Component
        {
            if (CheckNullGameObject(goProvider))
            {
                return goProvider.GetComponentInChildren<T>();
            }

            return null;
        }

        public static T GetComponentInParent<T>(this IGameObjectProvider goProvider) where T : Component
        {
            if (CheckNullGameObject(goProvider))
            {
                return goProvider.GetComponentInParent<T>();
            }

            return null;
        }

        public static T[] GetComponents<T>(this IGameObjectProvider goProvider) where T : Component
        {
            if (CheckNullGameObject(goProvider))
            {
                return goProvider.GetComponents<T>();
            }

            return new T[0];
        }

        public static T[] GetComponentsInChildren<T>(this IGameObjectProvider goProvider) where T : Component
        {
            if (CheckNullGameObject(goProvider))
            {
                return goProvider.GetComponentsInChildren<T>();
            }

            return new T[0];
        }

        public static T[] GetComponentsInParent<T>(this IGameObjectProvider goProvider) where T : Component
        {
            if (CheckNullGameObject(goProvider))
            {
                return goProvider.GetComponentsInParent<T>();
            }

            return new T[0];
        }

        private static bool CheckNullGameObject(IGameObjectProvider goProvider)
        {
            return goProvider != null && goProvider.gameObject != null;
        }
    }
}
