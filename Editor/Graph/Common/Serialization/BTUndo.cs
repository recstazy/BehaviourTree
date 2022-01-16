using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Recstazy.BehaviourTree.EditorScripts
{
    internal static class BTUndo
    {
        public class BTUndoContainer : ScriptableObject, ISerializationCallbackReceiver
        {
            [SerializeField]
            private string _serializedNodeData;

            [System.NonSerialized]
            private bool _wasDeserialized;

            public bool WasDeserialized 
            { 
                get
                {
                    bool was = _wasDeserialized;
                    _wasDeserialized = false;
                    return was;
                }
                private set => _wasDeserialized = value;
            }

            public void Serialize(BehaviourTree tree)
            {
                _serializedNodeData = new TreeNodeDataDescription(tree.NodeData).Serialize();
            }

            public void Apply(BehaviourTree tree)
            {
                if (string.IsNullOrEmpty(_serializedNodeData)) return;
                var desc = JsonUtility.FromJson<TreeNodeDataDescription>(_serializedNodeData);
                tree.NodeData = desc.CreateNodeData();
                EditorUtility.SetDirty(tree);
            }

            public void OnBeforeSerialize() { }

            public void OnAfterDeserialize()
            {
                WasDeserialized = true;
            }
        }

        [System.Serializable]
        public struct TreeNodeDataDescription
        {
            [SerializeField]
            private NodeDescription[] _dataArray;

            public TreeNodeDataDescription(TreeNodeData nodeData)
            {
                _dataArray = new NodeDescription[nodeData.Data.Length];

                for (int i = 0; i < _dataArray.Length; i++)
                {
                    _dataArray[i] = new NodeDescription(nodeData.Data[i]);
                }
            }

            public TreeNodeData CreateNodeData()
            {
                var dataArray = _dataArray.Select(d => d.GenerateData(true)).ToArray();
                var data = new TreeNodeData(dataArray);
                return data;
            }

            public string Serialize()
            {
                return JsonUtility.ToJson(this);
            }
        }

        #region Fields

        private static BehaviourTree s_currentTree;
        private static BTUndoContainer s_container;

        #endregion

        #region Properties
	
        #endregion

        public static void Initialize(BehaviourTree tree)
        {
            if (s_currentTree != tree)
            {
                s_container = ScriptableObject.CreateInstance<BTUndoContainer>();
                s_currentTree = tree;
                s_container.Serialize(s_currentTree);
                EditorUtility.SetDirty(s_container);
            }
        }

        public static void RegisterUndo(BehaviourTree tree, string name)
        {
            Undo.RegisterCompleteObjectUndo(s_container, name);
            s_container.Serialize(tree);
            EditorUtility.SetDirty(s_container);
        }

        public static bool ApplyUndoRedo(BehaviourTree tree)
        {
            if (s_currentTree != tree || s_container == null || !s_container.WasDeserialized) return false;
            s_container.Apply(tree);
            return true;
        }
    }
}
