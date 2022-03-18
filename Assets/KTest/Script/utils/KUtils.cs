using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ktest
{
    public static class KUtils
    {
        public static void Swap<T>(ref T a, ref T b){
            T tmp = a; a = b; b = tmp;
        }
        
        public static void SetLocalPos(Transform transform, Vector3 pos = default(Vector3), bool keepZ = false){
            if(keepZ) pos.z = transform.localPosition.z;
            transform.localPosition = pos;
        }
        
        public static void SetPos(Transform transform, Vector3 pos = default(Vector3), bool keepZ = false){
            if(keepZ) pos.z = transform.position.z;
            transform.position = pos;
        }
        
        public static void SetLocalPosX(Transform transform, float px){
            var pos = transform.localPosition;
            pos.x = px;
            transform.localPosition = pos;
        }

        public static void SetLocalPosY(Transform transform, float py){
            var pos = transform.localPosition;
            pos.y = py;
            transform.localPosition = pos;
        }
        
        public static void SetPosX(Transform transform, float px){
            var pos = transform.position;
            pos.x = px;
            transform.position = pos;
        }

        public static void SetPosY(Transform transform, float py){
            var pos = transform.position;
            pos.y = py;
            transform.position = pos;
        }

        public static void SetPosZ(Transform transform, float pz){
            var pos = transform.position;
            pos.z = pz;
            transform.position = pos;
        }

        public static void SetLocalScale(Transform transform, float sx, float sy = 0f){
            var s = transform.localScale;
            if(sx != 0) s.x = sx;
            if(sy != 0) s.y = sy;
            transform.localScale = s;
        }
        
        public static void MultyLocalScale(Transform transform, float sx, float sy = 0f){
            var s = transform.localScale;
            if(sx != 0) s.x *= sx;
            if(sy != 0) s.y *= sy;
            transform.localScale = s;
        }
        
        public static void FixOddScaleWithZ(Transform transform){
            if(CheckIsOddScale(transform)){
                var s = transform.localScale;
                s.z *= -1;
                transform.localScale = s;
            }
        }
        
        public static bool CheckIsOddScale(Transform transform){
            return CheckIsOddScale(transform.lossyScale);
        }

        public static bool CheckIsOddScale(Vector3 scale){
            return scale.x * scale.y * scale.z < 0;    
        }

        public static void SetEditorPause(bool value){
            #if UNITY_EDITOR
            EditorApplication.isPaused = value;
            #endif
        }
        
        public static bool IsListEmpty(IList list){
            return list == null || list.Count == 0;
        }
        
        public static bool IsCollectionEmpty(ICollection list){
            return list == null || list.Count == 0;
        }
        
        public static bool IsTCollectionEmpty<T>(ICollection<T> list){
            return list == null || list.Count == 0;
        }
        
        public static T GetOrAddComponent<T>(GameObject obj) where T : Component{
            if(obj == null) return null;
            
            T com = obj.GetComponent<T>();
            if(com == null)
                com = obj.AddComponent<T>();
            return com;
        }
        
        public static void DestroyObj(UnityEngine.Object obj)
        {
            if(obj == null) return;
            
#if UNITY_EDITOR
            if (Application.isPlaying) UnityEngine.Object.Destroy(obj);
            else UnityEngine.Object.DestroyImmediate(obj);
#else
			UnityEngine.Object.Destroy (obj);
#endif
        }
        
        public static List<T2> GetListInDict<T1, T2>(Dictionary<T1, List<T2>> dict, T1 key){
            if(!dict.ContainsKey(key))
                dict.Add(key, new List<T2>());
            return dict[key];
        }
    }
}
