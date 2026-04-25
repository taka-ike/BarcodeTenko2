using System;
using System.Diagnostics;
using System.Text;
using System.Timers;

namespace Tenko.Native.Services
{
    public class KeyboardCaptureService
    {
        private StringBuilder _buffer = new();
        private Stopwatch _stopwatch = new();
        private const long MaxIntervalMs = 100;

        public event Action<string>? OnScanCompleted;

        public void ProcessKey(string keyText)
        {
            char? digit = null;
            if (keyText.Length == 2 && keyText.StartsWith("D") && char.IsDigit(keyText[1]))
            {
                digit = keyText[1];
            }
            else if (keyText.StartsWith("NumPad") && keyText.Length == 7 && char.IsDigit(keyText[6]))
            {
                digit = keyText[6];
            }

            if (digit.HasValue)
            {
                long elapsed = _stopwatch.ElapsedMilliseconds;
                if (elapsed > MaxIntervalMs && _buffer.Length > 0)
                {
                    _buffer.Clear();
                }

                _buffer.Append(digit.Value);
                _stopwatch.Restart();
            }
            else if (keyText == "Return")
            {
                if (_buffer.Length == 10)
                {
                    OnScanCompleted?.Invoke(_buffer.ToString());
                }
                _buffer.Clear();
                _stopwatch.Reset();
            }
        }

        public void ClearBuffer()
        {
            _buffer.Clear();
            _stopwatch.Reset();
        }
    }
}
