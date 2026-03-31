using Audio;
using System.Collections;
using TMPro;
using UI.Narration;
using UnityEngine;
using UnityEngine.UI;

public class Fading : MonoBehaviour
{
    [SerializeField, Tooltip("Refers to wether the square should fade out at start or just be instantly transparent")]
    private bool _fadeOut = true;
    [SerializeField]
    private Image _image;
    [SerializeField]
    private TMP_Text _text;
    [SerializeField]
    private float speedIn = 1f;
    [SerializeField]
    private float _speedOut = 1f;
    [SerializeField]
    private float _textWriteDelay = 0.2f;

    [SerializeField] private bool _fadeAudio = true;
    
    private float _t;
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

    private bool _textWritten = false;
    public IEnumerator FadeIn(bool died)
    {
        _image.gameObject.SetActive(true);
        _t = 0;

        while (_t <= 1)
        {
            _t += Time.deltaTime * speedIn;
            yield return new WaitForEndOfFrame();
            _image.color = Color.Lerp(Color.clear, Color.black, _t);
            //_text.color = Color.Lerp(Color.clear, _textColor, _t);
            
            if (!_fadeAudio)
                continue;
            
            float volume = (1 - _t) * SettingsMenu.INSTANCE.MasterVolumeSlider.value;
            FMODEvents.INSTANCE.SetMasterVolume(volume);
        }
        _image.color = Color.black;

        //while (_t >= 0)
        //{
        //    _t += Time.deltaTime * speedIn;
        //    yield return new WaitForEndOfFrame();
        //    _text.color = Color.Lerp(Color.clear, _textColor, _t);
        //}
        //_text.color = Color.clear;

        if (!died) 
            yield break;
        
        _slowWriteRoutine = StartCoroutine(SlowWriteText());

        while (_slowWriteRoutine != null)
            yield return null;

        _image.gameObject.SetActive(true);
        _t = 1;
        yield return new WaitForSecondsRealtime(_textWriteDelay*3);
        while (_t >= 0)
        {
            _t -= Time.deltaTime * _speedOut;
            yield return new WaitForEndOfFrame();
            _text.color = Color.Lerp(Color.clear, _textColor, _t);
        }
        _text.color = Color.clear;
        _text.gameObject.SetActive(false);
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

    public IEnumerator FadeOut()
    {
        _image.gameObject.SetActive(true);
        _t = 1;

        while (_t >= 0)
        {
            _t -= Time.deltaTime * _speedOut;
            yield return new WaitForEndOfFrame();
            _image.color = Color.Lerp(Color.clear, Color.black, _t);
            _text.color = Color.Lerp(Color.clear, _textColor, _t);

            if (!_fadeAudio)
                continue;
            
            float volume = (1 - _t) * SettingsMenu.INSTANCE.MasterVolumeSlider.value;
            FMODEvents.INSTANCE.SetMasterVolume(volume);
        }
        _image.color = Color.clear;
        _image.gameObject.SetActive(false);
    }
}