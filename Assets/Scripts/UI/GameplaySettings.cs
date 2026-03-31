using UI;
using UnityEngine;
using UnityEngine.UI;

public class GameplaySettings : MonoBehaviour
{
    private static bool HEALTHBARSENABLED = true;
    public Toggle EnemyHealthBarToggle;
    

    private void Awake()
    {
        EnemyHealthBarToggle.isOn = HEALTHBARSENABLED;
    }

    private void OnEnable()
    {
        EnemyHealthBarToggle.onValueChanged.AddListener(EnemyHealthBarsEnabled);
    }

    private void OnDisable()
    {
        EnemyHealthBarToggle.onValueChanged.RemoveListener(EnemyHealthBarsEnabled);
    }

    public void EnemyHealthBarsEnabled(bool value)
    {
        UIEnemyHealth.SlidersEnabled(value);
        HEALTHBARSENABLED = value;
    }
}
