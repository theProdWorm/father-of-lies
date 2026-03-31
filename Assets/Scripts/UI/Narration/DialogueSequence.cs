using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Narration
{
    [CreateAssetMenu(fileName = "Dialogue Sequence", menuName = "Dialogue/Dialogue Sequence")]
    public class DialogueSequence : ScriptableObject
    {
        public bool Repeatable;
        public DialogueLine[] Lines;

        public bool IsVoiced;
        public string VoiceEventName;
        public string VoiceParameterName;
        
        public UnityEvent OnFinished;
        
        private void OnValidate()
        {
            OnFinished.RemoveAllListeners();
        }
    }
}