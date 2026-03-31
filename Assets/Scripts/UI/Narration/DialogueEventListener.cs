using UnityEngine;
using UnityEngine.Events;

namespace UI.Narration
{
    public class DialogueEventListener : MonoBehaviour
    {
        [SerializeField] private DialogueSequence _target;

        public UnityEvent OnDialogueSequenceFinished;
        
        private void Start()
        {
            _target.OnFinished.AddListener(OnDialogueSequenceFinished.Invoke);
        }
    }
}