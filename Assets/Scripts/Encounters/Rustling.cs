using System.Collections;
using UnityEngine;

public class Rustling : MonoBehaviour
{
    [SerializeField]
    private float _rustleAmount = 1.2f;
    [SerializeField]
    private float _rustleSpeed = 5f;
    [SerializeField, Tooltip("Multiplies how far the object can rustle in a certain axis, use values between 0-1")]
    private Vector3 _axleLimitators;

    public IEnumerator Rustle(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            float t = 0;
            Vector3 originPosition = transform.position;
            Vector3 targetPosition = transform.position + Random.insideUnitSphere * _rustleAmount;
            targetPosition = Vector3.Scale(targetPosition, _axleLimitators);

            float f = 0;
            while (f >= 0)
            {
                t += Time.deltaTime * _rustleSpeed;
                f = Mathf.Sin(t);
                yield return new WaitForFixedUpdate();
                transform.position = Vector3.Lerp(originPosition, targetPosition, f);
            }
        }
    }
}
