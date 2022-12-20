#if UNITY_EDITOR
using UnityEditor;

namespace MyApp.Golf.Editors
{
    [CustomEditor(typeof(GolfHole))]
    public class GolfHoleEditor : Editor
    {

        #region variable
        protected GolfHole Target;
        #endregion
        private void OnEnable()
        {
            Target = (GolfHole)target;
        }
        #region Inspector
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            basic();
            if (Target.enable)
            {
                Parameter();
                EditorTools.Line();
                Info();
            }
            serializedObject.ApplyModifiedProperties();
            Target._sphereCollider.radius = Target.vortexRadius;
        }
        private void basic()
        {
            EditorTools.Box_Open();
            EditorTools.PropertyField(serializedObject, "enable", "Enable", "Total system enable.");
            EditorTools.Box_Close();
        }
        private void Parameter()
        {
            if (!EditorTools.Foldout(ref Target.showParametersParts, "Parameter(s)")) return;
            EditorTools.Box_Open();
            EditorTools.PropertyField(serializedObject, "gravityEnable", "Gravity enable", "Hole gravity.");
            EditorTools.PropertyField(serializedObject, "maxContactTime", "Max contact time", "The Maximum cantact time that means golf ball is in hole.");
            if (Target.gravityEnable)
            {
                EditorTools.PropertyField(serializedObject, "gravityForce", "Gravity force", "The gravity force into center of hole.");
                EditorTools.PropertyField(serializedObject, "convergenceCoefficient", "Convergence coefficient");
                EditorTools.PropertyField(serializedObject, "vortexStrength", "Vortex strength");
                EditorTools.PropertyField(serializedObject, "vortexRadius", "Vortex radius");
                EditorTools.PropertyField(serializedObject, "safeRadius", "Safe radius", "The radius that ball is in hole.");
            }
            EditorTools.Box_Close();
        }
        private void Info()
        {
            if (!EditorTools.Foldout(ref Target.showInfoParts, "Infomation")) return;
            EditorTools.Box_Open();
            EditorTools.Info("Contact time", Target.ContactTime.ToString());
            EditorTools.Info("Vortex intensity", "<" + Target.vortexIntensity.ToString() + " ," + Target.vortexStrength + ">");
            EditorTools.Box_Close();
        }
        #endregion
    }
}
#endif