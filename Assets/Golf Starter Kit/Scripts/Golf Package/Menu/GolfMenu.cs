#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MyApp.Golf
{
    public class GolfMenu : MainMenu
    {
        #region GameManager
        [MenuItem(Globals.PROJECT_NAME + "/Game Manager/" + MainMenuItem.Add)]
        public static void AddGameManager()
        {
            var node = FindObjectOfType<GolfGameManager>();
            if (node == null)
            {
                if (Selection.objects != null && Selection.objects.Length > 0)
                {
                    Selection.objects = new Object[] { Selection.objects[0] };
                    AttachComponentToSelection<GolfGameManager>();
                }
                else
                {
                    AddComponentToObject<GolfGameManager>("Golf GameManager");
                }
            }
            else
            {
                Selection.objects = new Object[] { node };
            }
        }
        [MenuItem(Globals.PROJECT_NAME + "/Game Manager/" + MainMenuItem.RemoveAll)]
        public static void RemoveGameManager()
        {
            RemoveComponentFromSelection<GolfGameManager>();
        }
        #endregion
        #region Golf Ball
        [MenuItem(Globals.PROJECT_NAME + "/Ball/" + MainMenuItem.Add)]
        public static void AddBall()
        {
            var node = FindObjectOfType<GolfBall>();
            if (node == null)
            {
                if (Selection.objects != null && Selection.objects.Length > 0)
                {
                    Selection.objects = new Object[] { Selection.objects[0] };
                    AttachComponentToSelection<GolfBall>();
                }
                else
                {
                    AddComponentToObject<GolfBall>("Golf GameManager");
                }
            }
            else
            {
                Selection.objects = new Object[] { node };
            }
        }
        [MenuItem(Globals.PROJECT_NAME + "/Ball/" + MainMenuItem.RemoveAll)]
        public static void RemoveBall()
        {
            RemoveComponentFromSelection<GolfBall>();
        }
        #endregion
        #region Golf Hole
        [MenuItem(Globals.PROJECT_NAME + "/Hole/" + MainMenuItem.Attach)]
        public static void AddHole()
        {
            AttachComponentToSelection<GolfHole>();
        }
        [MenuItem(Globals.PROJECT_NAME + "/Hole/" + MainMenuItem.SelectAll)]
        public static void SelectHole()
        {
            SelectAllObjectsByComponent<GolfHole>();
        }
        [MenuItem(Globals.PROJECT_NAME + "/Hole/" + MainMenuItem.RemoveAll)]
        public static void RemoveHole()
        {
            RemoveComponentFromSelection<GolfHole>();
        }
        #endregion
    }
}
#endif