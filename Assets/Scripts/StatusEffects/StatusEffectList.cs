using Entities;
using StatusEffects.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace StatusEffects
{
    public class StatusEffectList
    {
        private static StatusEffectIcons ICONS;
        private static readonly Color TRANSPARENT = new Color(0, 0, 0, 0);
        
        private readonly Enemy _enemy;
        private StatusEffect[] _effects;

        private readonly Image _effectIcon;
        private readonly Image _countIcon;
        
        private int _capacity = 4;
        
        public int Count;

        private float _totalPulseDuration;
        private float _remainingPulseDuration;
        private float _pulseFrequency;
        private float _pulseIntensity;
        
        public StatusEffect this[int index] => _effects[index];
        
        public StatusEffectList(Enemy enemy, Image effectIcon, Image countIcon)
        {
            _enemy = enemy;
            
            _effects = new StatusEffect[_capacity];
            
            if (!ICONS)
                ICONS = Resources.Load<StatusEffectIcons>("Status Effect Icons");
            
            _effectIcon = effectIcon;
            _countIcon = countIcon;
            
            _effectIcon.sprite = null;
            _countIcon.sprite = null;

            _effectIcon.color = TRANSPARENT;
            _countIcon.color = TRANSPARENT;
        }
        
        public void Update()
        {
            for (int i = 0; i < Count; i++)
            {
                var effect = _effects[i];
                effect.Update();
                
                if (effect.Expired)
                    Remove(effect);
            }

            if (_remainingPulseDuration <= 0)
                return;
            
            _remainingPulseDuration -= Time.deltaTime;
            float t = _totalPulseDuration - _remainingPulseDuration;

            float interval = _pulseIntensity * 0.5f;
            float pulse = (1 + interval) + interval * Mathf.Sin(_pulseFrequency * t * 2 * Mathf.PI);
            _effectIcon.rectTransform.localScale = pulse * new Vector3(1, 2, 1);
        }

        public int GetCount<T>() where T : StatusEffect
        {
            var type = typeof(T);
            
            int count = 0;
            for (int i = 0; i < Count; i++)
            {
                if (_effects[i] == null)
                    break;
                
                if (_effects[i].GetType() == type)
                    count++;
            }

            return count;
        }
        
        private bool HasEffect(StatusEffect sampleEffect)
        {
            var type = sampleEffect.GetType();

            for (int i = 0; i < Count; i++)
            {
                if (_effects[i] == null)
                    return false;
                
                if (_effects[i].GetType() == type)
                    return true;
            }

            return false;
        }

        public bool HasEffect<T>() where T : StatusEffect
        {
            for (int i = 0; i < Count; i++)
            {
                if (_effects[i] == null)
                    return false;
                
                if (_effects[i].GetType() == typeof(T))
                    return true;
            }
            
            return false;
        }
        
        public void Add(StatusEffect effect)
        {
            if (effect.RefreshOnApplication)
                Refresh(effect);

            if (!effect.Stackable && HasEffect(effect))
                return;
            
            if (Count == _capacity)
                Extend();
            
            _effects[Count++] = effect;
            effect.Apply(_enemy);
            
            if (effect is StatusEffect_Frozen)
            {
                _totalPulseDuration = ICONS.FrozenPulseDuration;
                _remainingPulseDuration = ICONS.FrozenPulseDuration;
                _pulseFrequency = ICONS.FrozenPulseFrequency;
                _pulseIntensity = ICONS.FrozenPulseIntensity;
                
                _effectIcon.sprite = ICONS.FrozenIcon;
                _countIcon.sprite = null;

                _effectIcon.color = ICONS.MaxStackColor;
                _countIcon.color = TRANSPARENT;
                
                return;
            }
            
            _totalPulseDuration = ICONS.ChillPulseDuration;
            _remainingPulseDuration = ICONS.ChillPulseDuration;
            _pulseFrequency = ICONS.ChillPulseFrequency;
            _pulseIntensity = ICONS.ChillPulseIntensity;
            
            int chillCount = GetCount<StatusEffect_Chill>();
            float t = chillCount / 4f;
            var color = Color.Lerp(ICONS.MinStackColor, ICONS.MaxStackColor, t);
            
            _countIcon.sprite = ICONS.NumberSprites[chillCount - 1];
            _countIcon.color = color;
            
            _effectIcon.sprite = ICONS.ChillIcon;
            _effectIcon.color = color;
        }
        
        public void Remove(StatusEffect effect)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_effects[i] == effect)
                {
                    _effects[i].Remove(_enemy);
                    _effects[i] = null;
                    break;
                }
            }
            
            Rebuild();
            Count--;

            if (effect is StatusEffect_Frozen || Count == 0)
            {
                _effectIcon.sprite = null;
                _countIcon.sprite = null;
                _effectIcon.color = TRANSPARENT;
                _countIcon.color = TRANSPARENT;
                return;
            }
            
            int chillCount = GetCount<StatusEffect_Chill>();
            float t = chillCount / 4f;
            var color = Color.Lerp(ICONS.MinStackColor, ICONS.MaxStackColor, t);
            
            _countIcon.sprite = ICONS.NumberSprites[chillCount - 1];
            _countIcon.color = color;
            
            _effectIcon.sprite = ICONS.ChillIcon;
            _effectIcon.color = color;
        }

        public void RemoveAll<T>() where T : StatusEffect
        {
            var type = typeof(T);
            
            for (int i = Count - 1; i >= 0; i--)
            {
                if (_effects[i].GetType() != type)
                    continue;
                
                _effects[i].Remove(_enemy);
                _effects[i] = null;
                Count--;
            }
            
            Rebuild();
        
            _effectIcon.sprite = null;
            _countIcon.sprite = null;
            _effectIcon.color = TRANSPARENT;
            _countIcon.color = TRANSPARENT;
        }

        public void Clear()
        {
            for (int i = 0; i < Count; i++)
            {
                _effects[i].Remove(_enemy);
                _effects[i] = null;
            }
            
            Count = 0;
        }

        private void Extend()
        {
            _capacity *= 2;
            
            var newArray = new StatusEffect[_capacity];
            for (int i = 0; i < _effects.Length; i++)
            {
                newArray[i] = _effects[i];
            }
            
            _effects = newArray;
        }

        private void Rebuild()
        {
            int nextIndex = 1;
            for (int i = 0; i < _capacity - 1; i++)
            {
                if (_effects[i] != null)
                {
                    nextIndex++;
                    continue;
                }
                
                for (int j = nextIndex; j < _capacity; j++)
                {
                    if (_effects[j] == null)
                        continue;

                    _effects[i] = _effects[j];
                    _effects[j] = null;

                    nextIndex = j + 1;
                    
                    break;
                }
            }
        }

        private void Refresh(StatusEffect sampleEffect)
        {
            var effectType = sampleEffect.GetType();
            
            foreach (var effect in _effects)
            {
                if (effect == null || effect.GetType() != effectType)
                    continue;
                
                effect.RefreshDuration();
            }
        }
    }
}