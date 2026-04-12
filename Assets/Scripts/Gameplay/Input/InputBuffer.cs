using System;
using System.Collections.Generic;

namespace Gameplay.Input
{
    public static class InputBuffer
    {
        private static readonly List<InputRecord> _buffer = new();
        
        public static void Update()
        {
            for (int i = _buffer.Count - 1; i >= 0; i--)
            {
                _buffer[i].Update();
                
                if (_buffer[i].Expired)
                    _buffer.RemoveAt(i);
            }
        }

        public static void Add(Func<bool> callback, float duration)
        {
            var input = new InputRecord(callback, duration);
            _buffer.Add(input);
        }

        public static void NextInput()
        {
            if (_buffer.Count == 0)
                return;

            List<int> expendedInputIndices = new();
            for (int i = 0; i < _buffer.Count; i++)
            {
                bool success = _buffer[i].Callback();
                if (success)
                    expendedInputIndices.Add(i);
            }
            expendedInputIndices.Reverse();
            
            foreach (var i in expendedInputIndices)
                _buffer.RemoveAt(i);
        }
    }
}