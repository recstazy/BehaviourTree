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
                var sumData = ((NodeData[])nodeData.TaskData).Concat(nodeData.VarData).ToArray();
                _dataArray = new NodeDescription[sumData.Length];

                for (int i = 0; i < _dataArray.Length; i++)
                {
                    _dataArray[i] = new NodeDescription(sumData[i]);
                }
            }

            public TreeNodeData CreateNodeData()
            {
                var dataArray = _dataArray.Select(d => d.GenerateData(true)).ToArray();
                var taskData = dataArray.Select(d => d as TaskNodeData).Where(d => d != null).ToArray();
                var varData = dataArray.Select(d => d as VarNodeData).Where(d => d != null).ToArray();
                var data = new TreeNodeData(taskData, varData);
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
            if (s_currentTree != tree || s_container == null)
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

        internal static void LogUndo()
        {
            Debug.Log($"{s_container}, {s_currentTree?.name}");
        }
    }
}
