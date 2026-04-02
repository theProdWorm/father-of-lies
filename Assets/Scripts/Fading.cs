using Audio;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Fading : MonoBehaviour
{
    [SerializeField, Tooltip("Refers to whether the square should fade out at start or just be instantly transparent")]
    private bool _fadeOut = true;
    [SerializeField]
    private Image _image;
    [SerializeField]
    private TMP_Text _text;
    [SerializeField]
    private float _fadeInDuration = 1f;
    [SerializeField]
    private float _fadeOutDuration = 1f;
    [SerializeField]
    private float _deathFadeInDuration = 3f;
    [SerializeField]
    private float _deathFadeOutDuration = 2f;
    [SerializeField]
    private float _textWriteDelay = 0.1f;

    [SerializeField] private bool _fadeAudio = true;
    
    private Color _textColor;
    private Coroutine _slowWriteRoutine;

    private void Awake()
    {
        _textColor = _text.color;
        if (_fadeOut)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            _image.color = Color.clear;
            _text.color = Color.clear;
        }
    }

    public IEnumerator FadeIn(bool died)
    {
        Debug.Log($"Died: {died}");
        
        _image.gameObject.SetActive(true);
        float fadeDuration = died ? _deathFadeInDuration : _fadeInDuration;
        float elapsedTime = 0;
        while (elapsedTime <= fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
            
            float t = elapsedTime / fadeDuration;
            
            _image.color = Color.Lerp(Color.clear, Color.black, t);
            
            if (!_fadeAudio)
                continue;
            
            float volume = (1 - t) * SettingsMenu.INSTANCE.MasterVolumeSlider.value;
            FMODEvents.INSTANCE.SetMasterVolume(volume);
        }
        _image.color = Color.black;

        if (!died) 
            yield break;
        
        _slowWriteRoutine = StartCoroutine(SlowWriteText());
        while (_slowWriteRoutine != null)
            yield return null;
        
        yield return new WaitForSecondsRealtime(_textWriteDelay * 3);
        
        elapsedTime = 0;
        while (elapsedTime <= _deathFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
            
            float t = elapsedTime / _deathFadeOutDuration;
            
            _text.color = Color.Lerp(_textColor, Color.clear, t);
        }
        _text.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut()
    {
        _image.gameObject.SetActive(true);
        float elapsedTime = 0;

        while (elapsedTime <= _fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
            
            float t = elapsedTime / _fadeOutDuration;
            
            _image.color = Color.Lerp(Color.black, Color.clear, t);
            _text.color = Color.Lerp(_textColor, Color.clear, t);

            if (!_fadeAudio) 
                continue;
            
            float volume = t * SettingsMenu.INSTANCE.MasterVolumeSlider.value;
            FMODEvents.INSTANCE.SetMasterVolume(volume);
        }
        _image.color = Color.clear;
        _image.gameObject.SetActive(false);
    }
    
    private IEnumerator SlowWriteText()
    {
        string textToWrite = _text.text;
        string text = "";
        _text.text = text;
        _text.gameObject.SetActive(true);
        _text.color = _textColor;

        float delay = _textWriteDelay;
        foreach (var c in textToWrite)
        {
            text += c;
            _text.text = text;
            yield return new WaitForSeconds(delay);
        }
        
        _slowWriteRoutine = null;
    }
}