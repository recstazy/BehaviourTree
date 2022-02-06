using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Recstazy.BehaviourTree
{
    public static class JsonHelper
    {
        public static bool Serialize(object obj, out string json, out string typeString)
        {
            if (obj == null)
            {
                json = string.Empty;
                typeString = string.Empty;
                return false;
            }

            typeString = GetTypeAsString(obj);
            json = JsonUtility.ToJson(obj);
            return true;
        }

        public static object Deserialize(string json, string typeString)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(typeString)) return null;

            var type = StringToType(typeString);
            if (type == null) return null;

            return JsonUtility.FromJson(json, type);
        }

        public static string GetTypeAsString(object obj)
        {
            return GetTypeString(obj.GetType());
        }

        public static string GetTypeString(Type type)
        {
            var asmName = type.Assembly.FullName.Split(',').FirstOrDefault();
            var typeName = type.FullName;
            return $"{typeName}, {asmName}";
        }

        public static Type StringToType(string typeString)
        {
            return Type.GetType(typeString);
        }
    }
}
