using System;
using UnityEngine;

namespace Gameplay.Input
{
    public class InputRecord
    {
        public readonly Func<bool> Callback;
        private float _duration;

        public bool Expired => _duration <= 0;
        
        public InputRecord(Func<bool> callback, float duration)
        {
            Callback = callback;
            _duration = duration;
        }

        public void Update()
        {
            _duration -= Time.deltaTime;
        }
    }
}