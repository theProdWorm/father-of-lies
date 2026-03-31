using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace UI
{
    public class DamageNumbers : MonoBehaviour
    {
        [Tooltip("Parent object of the numbers, has to be under a canvas")]
        [SerializeField]private GameObject parentObject;
        [SerializeField]private TextMeshProUGUI textPrefab;
        [Tooltip("Static offset for damage numbers")]
        [SerializeField]private Vector3 offset;
        [Tooltip("How far the random offset can be")]
        [SerializeField]private float randomOffsetStrength;
        [Tooltip("Curve controlling how the points float up or down")]
        [SerializeField]private AnimationCurve floatCurve;
        [Tooltip("The amplitude of the Float Curve")]
        [SerializeField]private float floatStrength;

        [SerializeField]private float numberLifetime;
    
        private Camera cam;
    
        private static ObjectPool<TextMeshProUGUI> textPool;
        private static List<numberInfo> numbers;

    
        private static float Lifetime;
        private static float randOffset;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            textPool = new ObjectPool<TextMeshProUGUI>(
                createFunc: CreateText,
                actionOnGet: GetText,
                actionOnRelease: ReleaseText,
                actionOnDestroy: DestroyText
            );
            Lifetime = numberLifetime;
            randOffset = randomOffsetStrength;
            numbers = new List<numberInfo>();
            var text = textPool.Get();
            textPool.Release(text);
        
        }

        void Start()
        {
            cam = Camera.main;
            GetComponent<RectTransform>();
        }
    
        #region  pool
        private TextMeshProUGUI CreateText()
        {
            TextMeshProUGUI obj = Instantiate(textPrefab, parentObject.transform);
            obj.name = "damageNumber";
            obj.gameObject.SetActive(false);
            return obj;
        }

        private void GetText(TextMeshProUGUI text)
        {
            text.gameObject.SetActive(true);
        }

        private void ReleaseText(TextMeshProUGUI text)
        {
            text.text = "";
            text.gameObject.SetActive(false);
        }

        private void DestroyText(TextMeshProUGUI text)
        {
            Destroy(text.gameObject);
        }
        #endregion
    
    
        void LateUpdate()
        {
            if(numbers.Count == 0) return;
            for (int i = 0; i < numbers.Count; i++)
            {
                var number = numbers[i];
                if (number.timeLeft <= 0)
                {
                    textPool.Release(number.text);
                    numbers.Remove(number);
                    continue;
                }
                number.timeLeft -= Time.deltaTime;
            
                Vector3 screenPos = cam.WorldToScreenPoint(number.target + offset);
            
                screenPos.y += floatCurve.Evaluate(1-Mathf.Clamp01(number.timeLeft/number.lifetime))*floatStrength;
                bool visible = screenPos.z > 0 &&
                               screenPos.x >= 0 && screenPos.x <= Screen.width &&
                               screenPos.y >= 0 && screenPos.y <= Screen.height;

                if (visible)
                {
                    number.text.enabled = true;
                    number.text.rectTransform.position = screenPos;
                    var textColor = number.text.color;
                    textColor.a = Mathf.Clamp01(number.timeLeft / number.lifetime);
                    number.text.color = textColor;
                }
                else number.text.enabled = false;
                
            
                numbers[i] = number;
            
            }
        }

        public static void CreateDamageNumber(Transform position, int damage)
        {
            if (damage == 0) 
                return;
            
            var text = textPool.Get();
            Vector3 rand = Random.insideUnitCircle*randOffset;
            Vector3 pos = new Vector3(position.position.x,position.position.y+2, position.position.z) + rand;
            var info = new numberInfo
            {
                text = text,
                lifetime = Lifetime,
                target = pos,
            };
            info.timeLeft = info.lifetime;
            info.text.text = damage.ToString();
            numbers.Add(info);
        }

        private struct numberInfo : IEquatable<numberInfo>
        {
            public TextMeshProUGUI text;
            public float timeLeft;
            public float lifetime;
            public Vector3 target;

            public bool Equals(numberInfo other)
            {
                return timeLeft.Equals(other.timeLeft) && lifetime.Equals(other.lifetime) && target.Equals(other.target);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(timeLeft, lifetime, target);
            }

            // public override bool Equals(object obj)
            // {
            //     return obj is numberInfo other && Equals(other);
            // }
        }
    }
}
