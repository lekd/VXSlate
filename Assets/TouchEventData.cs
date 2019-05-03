using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace Assets
{
    [Serializable]
    public class TouchEventData
    {
        
        int _eventType;
        public int EventType
        {
            get
            {
                return _eventType;
            }

            set
            {
                _eventType = value;
            }
        }
        
        int _pointerCount;
        public int PointerCount
        {
            get
            {
                return _pointerCount;
            }

            set
            {
                _pointerCount = value;
            }
        }
        
        TouchPointerData[] _avaiPointers;
        public TouchPointerData[] AvaiPointers
        {
            get
            {
                return _avaiPointers;
            }

            set
            {
                _avaiPointers = value;
            }
        }
        public static TouchEventData ParseToucEventFromJsonString(string jsonStr)
        {
            return JsonConvert.DeserializeObject<TouchEventData>(jsonStr);
        }
        public void Clone(TouchEventData source)
        {
            this.EventType = source.EventType;
            this.PointerCount = source.PointerCount;
            this.AvaiPointers = new TouchPointerData[this.PointerCount];
            for(int i=0;i<this.PointerCount;i++)
            {
                AvaiPointers[i] = new TouchPointerData();
                AvaiPointers[i].Clone(source.AvaiPointers[i]);
            }
        }
        
    }
}
