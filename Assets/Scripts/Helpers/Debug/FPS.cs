///FPS COUNTER
///Written by: Annop "Nargus" Prapasapong
///Created: 7 June 2012


using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour
{
    public int FramesPerSec { get; protected set; }

    [SerializeField] private float frequency = 0.5f;


    private TextMeshProUGUI counter;

    private void Start()
    {
        counter = GetComponent<TextMeshProUGUI>();
        counter.text = "";
        StartCoroutine(FPSRoutine());
    }

    private IEnumerator FPSRoutine()
    {
        for (; ; )
        {
            int lastFrameCount = Time.frameCount;
            float lastTime = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(frequency);

            float timeSpan = Time.realtimeSinceStartup - lastTime;
            int frameCount = Time.frameCount - lastFrameCount;

            FramesPerSec = Mathf.RoundToInt(frameCount / timeSpan);
            counter.text = "FPS: " + FramesPerSec.ToString();
        }
    }
}
