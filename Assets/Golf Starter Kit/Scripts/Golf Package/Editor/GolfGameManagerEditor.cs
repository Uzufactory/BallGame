#if UNITY_EDITOR
using UnityEditor;

namespace MyApp.Golf.Editors
{
    [CustomEditor(typeof(GolfGameManager))]
    public class GolfGameManagerEditor : Editor
    {

        #region variable
        protected GolfGameManager Target;
        #endregion
        private void OnEnable()
        {
            Target = (GolfGameManager)target;
        }
        #region Inspector
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Parameter(); GameEvents();
            Info();
            serializedObject.ApplyModifiedProperties();
        }
        private void Parameter()
        {
            if (!EditorTools.Foldout(ref Target.showParametersParts, "Parameter(s)")) return;
            EditorTools.Box_Open();
            EditorTools.PropertyField(serializedObject, "infiniteShoot", "Infinite shoot(s)");
            if (!Target.infiniteShoot)
            {
                EditorTools.PropertyField(serializedObject, "maxShoots", "Max Shoots");
            }
            EditorTools.Box_Close();
        }
        private void GameEvents()
        {
            if (!EditorTools.Foldout(ref Target.showGameEventsParts, "Game Event(s)")) return;
            
            EditorTools.Box_Open();
            EditorTools.PropertyField(serializedObject, "gameOverByBallIsInHoleEventDelay", "Delay for GameOver - Ball is in hole");
            EditorTools.PropertyField(serializedObject, "gameOverByBallIsInHoleEvent", "GameOver - Ball is in hole");
            EditorTools.Line();
            EditorTools.PropertyField(serializedObject, "gameOverByShootsIsOverEventDelay", "Delay for GameOver - Shoot(s) over");
            EditorTools.PropertyField(serializedObject, "gameOverByShootsIsOverEvent", "GameOver - Shoot(s) over");
            EditorTools.Box_Close();
        }
        private void Info()
        {
            if (!EditorTools.Foldout(ref Target.showInfoParts, "Infomation")) return;
            EditorTools.Box_Open();
            EditorTools.Info("Game Is Over", Target.GameIsOver.ToString());
            EditorTools.Info("Shoot(s)", Target.currentShoots.ToString());
            EditorTools.Info("GameState", Target.GameState.ToString());
            EditorTools.Box_Close();
        }
        #endregion
    }
}
#endif