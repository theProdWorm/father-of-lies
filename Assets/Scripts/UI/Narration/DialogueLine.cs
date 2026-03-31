using System;
using UnityEngine;

namespace UI.Narration
{
    [Serializable]
    public class DialogueLine
    {
        [TextArea]
        public string Text;
        public DialogueSpeaker Speaker;
    }
}