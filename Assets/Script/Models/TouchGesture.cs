using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script
{
    public class TouchGesture
    {
        private GestureType _gestureType;
        private object _metaData;

        public GestureType GestureType
        {
            get
            {
                return _gestureType;
            }

            set
            {
                _gestureType = value;
            }
        }

        public object MetaData
        {
            get
            {
                return _metaData;
            }

            set
            {
                _metaData = value;
            }
        }

        public TouchGesture()
        {
            _gestureType = GestureType.NONE;
            _metaData = null;
        }
    }
}
