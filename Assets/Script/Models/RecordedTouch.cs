using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script
{
    public class RecordedTouch
    {
        DateTime _timeStamp;

        public DateTime TimeStamp
        {
            get
            {
                return _timeStamp;
            }

            set
            {
                _timeStamp = value;
            }
        }

        public List<TouchPointerData> TouchPointers
        {
            get
            {
                return _touchPointers;
            }

            set
            {
                _touchPointers = value;
            }
        }

        List<TouchPointerData> _touchPointers;

        public RecordedTouch()
        {
            _timeStamp = System.DateTime.Now;
            _touchPointers = new List<TouchPointerData>();
        }
    }
}
