using TMPro;
using UnityEngine;

public class DebugText : MonoBehaviour
{
    private static TextMeshProUGUI DEBUG_TEXT;
    
    private void Awake()
    {
        DEBUG_TEXT = GetComponent<TextMeshProUGUI>();
    }

    public static void Log(string text)
    {
        if (!DEBUG_TEXT) return;
        DEBUG_TEXT.text = text;
    }
}
