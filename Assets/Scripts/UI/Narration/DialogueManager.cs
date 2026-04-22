using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using FMOD.Studio;
using Audio;
using Entities;
using Entities.Player;

namespace UI.Narration
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private PlayerInput _playerInput;
        
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private TextMeshProUGUI _dialogueTMP;
        [SerializeField] private Image _speakerImage;
        [SerializeField] private Image _speakerBackground;
        [SerializeField] private GameObject _nextIndicator;
	    [SerializeField] private FMODEvents _fmodEvents;

        [SerializeField] private GameObject _hud;
    
        [SerializeField] private float _defaultLetterDelay;
        [SerializeField] private LetterDelay[] _customLetterDelays;

        public static int DIALOGUE_PAGE;
        private DialogueSequence _dialogue;

        public static bool PLAYING_VOICED_DIALOGUE => INSTANCE && INSTANCE._dialogue && INSTANCE._dialogue.IsVoiced;
        public static string VOICE_PARAMETER_NAME => INSTANCE._dialogue.VoiceParameterName;

        private Coroutine _slowWriteCoroutine;
        
        private DialogueLine CurrentLine => _dialogue.Lines[DIALOGUE_PAGE];
        
        public static DialogueManager INSTANCE;
        
        private void Awake()
        {
            if (INSTANCE)
            {
                Destroy(gameObject);
                return;
            }

            INSTANCE = this;
            _dialoguePanel.SetActive(false);
        }

        public static void StartDialogue(DialogueSequence dialogue)
        {
            INSTANCE._playerInput.SwitchCurrentActionMap("Dialogue");
            
            INSTANCE._dialogue = dialogue;
            DIALOGUE_PAGE = 0;
            
            INSTANCE._dialoguePanel.SetActive(true);
            
            if (INSTANCE._hud) 
                INSTANCE._hud.SetActive(false);
            
            INSTANCE._nextIndicator.SetActive(INSTANCE._dialogue.Lines.Length > 1);
            INSTANCE._slowWriteCoroutine = INSTANCE.StartCoroutine(SlowWriteText(dialogue.Lines[0]));

            if (!INSTANCE._dialogue.IsVoiced)
                return;

            INSTANCE._fmodEvents.PlayEvent(INSTANCE._dialogue.VoiceEventName);
        }

        private static void EndDialogue()
        {
            INSTANCE._playerInput.SwitchCurrentActionMap("Player");
            
            INSTANCE._dialoguePanel.SetActive(false);
            
            if (INSTANCE._hud)
                INSTANCE._hud.SetActive(true);

            INSTANCE._dialogue.OnFinished?.Invoke();
        }

        public static void AdvanceDialogue(InputAction.CallbackContext context)
        {
            if (!context.started)
                return;
            
            if (INSTANCE._slowWriteCoroutine != null)
            { // Skip slow write
                INSTANCE.StopCoroutine(INSTANCE._slowWriteCoroutine);
                INSTANCE._slowWriteCoroutine = null;
                
                INSTANCE._dialogueTMP.text = INSTANCE.CurrentLine.Text;
            }
            else if (DIALOGUE_PAGE < INSTANCE._dialogue.Lines.Length - 1)
            { // Advance to next line
                INSTANCE._nextIndicator.SetActive(DIALOGUE_PAGE < INSTANCE._dialogue.Lines.Length - 2);
                
                var line = INSTANCE._dialogue.Lines[++DIALOGUE_PAGE];
                INSTANCE._slowWriteCoroutine = INSTANCE.StartCoroutine(SlowWriteText(line));
            }
            else
                EndDialogue();
        }

        private static void SetSpeaker(DialogueSpeaker speaker)
        {
            var textColor = speaker.TextColor;
            textColor.a = 1f;
            
            INSTANCE._speakerImage.color = textColor;
            INSTANCE._speakerImage.sprite = speaker.Sprite;

            if (speaker.name == "Daughter of Lies")
                PlayerController.SetActiveCharacter(false);
            else if (speaker.name == "Son of Lies")
                PlayerController.SetActiveCharacter(true);
        }

        private static float GetLetterDelay(char targetChar)
        {
            foreach (var c in INSTANCE._customLetterDelays)
            {
                if (c.Letter == targetChar)
                    return c.Delay;
            }

            return INSTANCE._defaultLetterDelay;
        }
        
        private static IEnumerator SlowWriteText(DialogueLine line)
        {
            SetSpeaker(line.Speaker);
            
            string text = "";

            foreach (var c in line.Text)
            {
                float delay = GetLetterDelay(c);

                text += c;
                INSTANCE._dialogueTMP.text = text;
                yield return new WaitForSeconds(delay);
            }
            
            INSTANCE._slowWriteCoroutine = null;
        }
    }
}
