using UnityEngine;

namespace UI.Narration
{
    [CreateAssetMenu(fileName = "Speaker", menuName = "Dialogue/Speaker")]
    public class DialogueSpeaker : ScriptableObject
    {
        public Color  TextColor;
        public Sprite Sprite;
    }
}