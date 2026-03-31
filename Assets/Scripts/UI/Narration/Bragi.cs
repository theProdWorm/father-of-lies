using System.Collections.Generic;
using UnityEngine;

namespace UI.Narration
{
    public class Bragi : DialogueObject
    {
        [SerializeField] private List<DialogueSequence> _firstMeeting;
        [SerializeField] private List<DialogueSequence> _afterFirstBranch;
        [SerializeField] private List<DialogueSequence> _afterSecondBranch;
        
        private void Start()
        {
            Debug.Log("Bragi");
            
            if (ProgressPersistence.SecondBranchDone)
            {
                Debug.Log("Second branch done");
                _dialogueQueue = _afterSecondBranch;
            }
            else if (ProgressPersistence.FirstBranchDone)
            {
                Debug.Log("First branch done");
                _dialogueQueue = _afterFirstBranch;
            }
            else
            {
                Debug.Log("hub meeting");
                _dialogueQueue = _firstMeeting;
            }
        }
    }
}