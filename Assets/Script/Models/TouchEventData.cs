using Assets.Script;
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
        public static TouchEventData ParseTouchEventFromBytes(byte[] byteData, int startingIndex)
        {
            TouchEventData touchEventData = new TouchEventData();
            byte[] buffer = new byte[4];
            int offset = startingIndex;
            Array.Copy(byteData, offset, buffer, 0, buffer.Length);
            touchEventData.EventType = GlobalUtilities.ByteArray2Int(buffer);
            offset += buffer.Length;
            Array.Copy(byteData, offset, buffer, 0, buffer.Length);
            touchEventData.PointerCount = GlobalUtilities.ByteArray2Int(buffer);
            offset += buffer.Length;
            List<TouchPointerData> pointersList = new List<TouchPointerData>();
            for(int i=0; i< touchEventData.PointerCount; i++)
            {
                byte[] pointerBytes = new byte[TouchPointerData.BYTE_SIZE];
                Array.Copy(byteData, offset, pointerBytes, 0, pointerBytes.Length);
                TouchPointerData pointer = TouchPointerData.parseFromBytes(pointerBytes);
                pointersList.Add(pointer);
                offset += TouchPointerData.BYTE_SIZE;
            }
            touchEventData.AvaiPointers = pointersList.ToArray<TouchPointerData>();
  
            return touchEventData;
        }
        public string toString()
        {
            string str = "";
            str += string.Format("EventType: {0};", _eventType);
            str += string.Format("PointerCount: {0};", _pointerCount);
            for(int i=0; i< _pointerCount; i++)
            {
                str += _avaiPointers[i].toString() + ";";
            }
            return str;
        }
    }
}
