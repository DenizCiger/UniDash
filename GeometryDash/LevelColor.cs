using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal class LevelColor
    {
        private byte[] _startRGB { get; set; } = { 255, 255, 255 };
        private byte[] _currRGB = { 255, 255, 255 };
        private byte[] _targetRGB = new byte[3];
        private float _fadeTime = 0f;
        private float _leftFadeTime = 0f;

        public LevelColor HardClone()
        {
            LevelColor clonedColor = new LevelColor();

            clonedColor._startRGB = (byte[])_startRGB.Clone();
            clonedColor._currRGB = (byte[])_currRGB.Clone();
            clonedColor._targetRGB = (byte[])_targetRGB.Clone();
            clonedColor._fadeTime = this._fadeTime;
            clonedColor._leftFadeTime = this._leftFadeTime;

            return clonedColor;
        }

        public void SetChannelColor(byte r, byte g, byte b)
        {
            _currRGB[0] = r;
            _currRGB[1] = g;
            _currRGB[2] = b;
        }

        public void SetRGBFade(byte[] targetRGB, float fadeTime)
        {
            Array.Copy(targetRGB, _targetRGB, 3);
            Array.Copy(_currRGB, _startRGB, 3);
            _fadeTime = fadeTime;
            _leftFadeTime = fadeTime;
        }
        public void UpdateRGBFade(float deltaMillies)
        {
            float[] RGBDiff = { _targetRGB[0] - _currRGB[0], _targetRGB[1] - _currRGB[1], _targetRGB[2] - _currRGB[2] };
            float multiplier = deltaMillies / (_leftFadeTime * 1000f);

            _leftFadeTime -= (deltaMillies / 1000f);
            _leftFadeTime = (_leftFadeTime < 0) ? 0 : _leftFadeTime;

            if (multiplier > 1)
            {
                Array.Copy(_targetRGB, _currRGB, 3);
            }
            else
            {
                _currRGB[0] += (byte)(RGBDiff[0] * multiplier);
                _currRGB[1] += (byte)(RGBDiff[1] * multiplier);
                _currRGB[2] += (byte)(RGBDiff[2] * multiplier);
            }
        }

        public byte[] GetCurrRGB()
        {
            return _currRGB;
        }

        public float GetLeftFadeTime()
        {
            return _leftFadeTime;
        }

        public bool ReachedTargetColor()
        {
            return (ReachedRed() && ReachedGreen() && ReachedBlue());
        }

        private bool ReachedBlue()
        {
            return _currRGB[2] == _targetRGB[2];
        }

        private bool ReachedGreen()
        {
            return _currRGB[1] == _targetRGB[1];
        }

        private bool ReachedRed()
        {
            return _currRGB[0] == _targetRGB[0];
        }
    }
}
