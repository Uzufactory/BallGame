#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace MyApp
{
    public class MainMenu : MonoBehaviour
    {
        #region Component
        public static void CreateComponentIfNotExits<T>(string name) where T : Component
        {
            var node = FindObjectOfType<T>();
            if (node == null)
            {
                AddComponentToObject<T>(name);
            }
            else
            {
                Selection.objects = new Object[] { node };
            }
        }
        public static void AddComponentToObject<T>(string name) where T : Component
        {
            GameObject node = new GameObject(name);
            node.transform.SetPositionAndRotation(getPosition(), Quaternion.identity);
            node.AddComponent<T>();
            Undo.RegisterCreatedObjectUndo(node, "Create object");
            Selection.objects = new Object[] { node };
        }
        public static void AttachComponentToSelection<T>() where T : Component
        {
            if (Selection.objects == null) return;
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                var node = (GameObject)Selection.objects[i];
                if (node == null || node.GetComponent<T>() != null) continue;
                Undo.AddComponent<T>(node);
            }
        }
        public static void RemoveComponentFromSelection<T>() where T : Component
        {
            if (Selection.objects == null) return;
            T function;
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                var node = (GameObject)Selection.objects[i];
                if (node == null || (function = node.GetComponent<T>()) == null) continue;
                Undo.DestroyObjectImmediate(function);
            }
        }
        public static void SelectAllObjectsByComponent<T>() where T : Component
        {
            var nodes = FindObjectsOfType<T>();
            List<Object> result = new List<Object>();
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node == null) continue;
                    result.Add(node.gameObject);
                }
            }

            Selection.objects = result.ToArray();
        }
        #endregion
        #region About us
        [MenuItem(Globals.PROJECT_NAME + "/Publisher Page")]
        public static void PublisherPage()
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/48757");
        }
        #endregion
        private static Vector2 getPosition()
        {
            return Vector2.zero;
        }
    }
}
#endif