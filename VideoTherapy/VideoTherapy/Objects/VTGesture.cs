using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy.Objects
{
    public class VTGesture
    {
        public string GestureName { set; get; }
        public float MaxProgressValue { set; get; }
        public float MinProgressValue { set; get; }
        public float TrsholdProgressValue { set; get; }
        public float ConfidanceTrshold { set; get; }
        public Boolean IsSuccess { set; get;}
        public Boolean IsTrack { set; get; }

        private float _progressValue = 0;
        private float _confidanceValue = 0;

        public VTGesture()
        {

        }

        // to duplicate gestures - copy constractor
        public VTGesture(VTGesture _serverGesture)
        {
            GestureName = _serverGesture.GestureName;
            MaxProgressValue = _serverGesture.MaxProgressValue;
            MinProgressValue = _serverGesture.MinProgressValue;
            TrsholdProgressValue = _serverGesture.TrsholdProgressValue;
            IsTrack = _serverGesture.IsTrack;
            IsSuccess = false;

            InitProgressDirection();
        }

        public float ProgressValue
        {
            get
            {
                return _progressValue;
            }

            set
            {
                if (MaxProgressValue > MinProgressValue) // from 0 to 1
                {
                    if (_progressValue < value)
                    {
                        _progressValue = value;
                    }
                }
                else // from 1 to 0
                {
                    if (_progressValue > value)
                    {
                        _progressValue = value;
                    }
                }
            }
        }

        public float ConfidenceValue
        {
            get
            {
                return _confidanceValue;
            }

            set
            {
                if (value > _confidanceValue)
                {
                    _confidanceValue = value;
                }
            }
        }

        // If the values are from 0 to 1 or from 1 to 0
        public void InitProgressDirection()
        {
            _progressValue = (MaxProgressValue < MinProgressValue) 
                             ? Math.Max(MinProgressValue, MaxProgressValue)
                             : Math.Min(MinProgressValue, MaxProgressValue);
        }

        public Boolean IsPassConfidanceTrshold()
        {
            return ConfidenceValue > ConfidanceTrshold;
            //return ConfidenceValue > 0f;
        }

        public Boolean IsPassProgressTrshold()
        {
            if (MaxProgressValue > MinProgressValue) // from 0 to 1
            {
                return ProgressValue > TrsholdProgressValue;
            }
            else // from 1 to 0
            {
                return TrsholdProgressValue > ProgressValue;
            }
        }
    }
}
