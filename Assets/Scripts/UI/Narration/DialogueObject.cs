using System.Collections.Generic;
using UnityEngine;

namespace UI.Narration
{
    public class DialogueObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject _indicator;
    
        public bool Highlighted { get; set; }
    
        public Vector3 Position { get; private set; }
    
        [SerializeField] protected List<DialogueSequence> _dialogueQueue;

        private void Update()
        {
            Position = transform.position;
        
            if (_indicator)
                _indicator.SetActive(Highlighted);
        }

        public void AddDialogue(DialogueSequence dialogue) => _dialogueQueue.Add(dialogue);
        
        public void Interacted()
        {
            if (_dialogueQueue.Count == 0)
                return;
            
            var dialogue = _dialogueQueue[0];
            DialogueManager.StartDialogue(dialogue);
            
            if (!dialogue.Repeatable)
                _dialogueQueue.RemoveAt(0);
        }
    }
}
