using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal class LevelGrid
    {
        private int _objectID;          //1
        private bool _flippedHorizontal;//4
        private bool _flippedVertical;  //5
        private int _rotation;          //6
        private int _colorTriggerRed;   //7
        private int _colorTriggerGreen; //8
        private int _colorTriggerBlue;  //9
        private float _triggerDuration; //10
        private bool _touchTriggered;   //11
        private bool _tintGround;       //14
        private int _targetColorID;     //23

        //Set Values
        public void SetObjectID(int value)
        {
            _objectID = value;
        }
        public void SetHorizontalFlipped(int value)
        {
            _flippedHorizontal = value == 1;
        }
        public void SetVerticalFlipped(int value)
        {
            _flippedVertical = value == 1;
        }
        public void SetRotation(int value)
        {
            _rotation = value;
        }
        public void SetColorTriggerRed(int value)
        {
            _colorTriggerRed = value;
        }
        public void SetColorTriggerGreen(int value)
        {
            _colorTriggerGreen = value;
        }
        public void SetColorTriggerBlue(int value)
        {
            _colorTriggerBlue = value;
        }
        public void SetTouchTriggered(int value)
        {
            _touchTriggered = value == 1;
        }
        public void SetTintGround(int value)
        {
            _tintGround = value == 1;
        }
        public void SetTriggerTargetColorID(int value)
        {
            _targetColorID = value;
        }
        public void SetTriggerDuration(float value)
        {
            _triggerDuration = value;
        }

        // GetValues
        public bool GetHorizontalFlip()
        {
            return _flippedHorizontal;
        }
        public bool GetVerticalFlip()
        {
            return _flippedVertical;
        }
        public bool GetTouchTriggered()
        {
            return _touchTriggered;
        }
        public int GetObjectID()
        {
            return _objectID;
        }
        public float GetTriggerDuration()
        {
            return _triggerDuration;
        }
        public byte GetColorRed()
        {
            return (byte)_colorTriggerRed;
        }
        public byte GetColorGreen()
        {
            return (byte)_colorTriggerGreen;
        }
        public byte GetColorBlue()
        {
            return (byte)_colorTriggerBlue;
        }
        public bool GetTintGround()
        {
            return _tintGround;
        }
    }
}
