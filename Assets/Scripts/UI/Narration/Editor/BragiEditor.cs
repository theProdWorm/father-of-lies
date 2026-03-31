using UnityEditor;

namespace UI.Narration.Editor
{
    [CustomEditor(typeof(Bragi))]
    public class BragiEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var indicator = serializedObject.FindProperty("_indicator");
            
            var firstMeeting = serializedObject.FindProperty("_firstMeeting");
            var afterFirstBranch = serializedObject.FindProperty("_afterFirstBranch");
            var afterSecondBranch = serializedObject.FindProperty("_afterSecondBranch");
            
            EditorGUILayout.PropertyField(indicator);
            
            EditorGUILayout.PropertyField(firstMeeting);
            EditorGUILayout.PropertyField(afterFirstBranch);
            EditorGUILayout.PropertyField(afterSecondBranch);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}