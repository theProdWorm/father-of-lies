using System;
using System.Collections.Generic;

namespace Gameplay.Input
{
    public class InputBuffer
    {
        private readonly List<InputRecord> _buffer = new();
        private readonly float _bufferMargin;
        
        public InputBuffer(float bufferMargin)
        {
            _bufferMargin = bufferMargin;
        }

        public void Update()
        {
            for (int i = _buffer.Count - 1; i >= 0; i--)
            {
                _buffer[i].Update();
                
                if (_buffer[i].Expired)
                    _buffer.RemoveAt(i);
            }
        }

        public void Add(Func<bool> callback)
        {
            var input = new InputRecord(callback, _bufferMargin);
            _buffer.Add(input);
        }

        public void NextInput()
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