using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script
{
    public class TouchGestureRecognizer
    {
        public enum TouchGestureType { PAD_SCALING, PAD_TRANSLATING, FIVE_POINTERS, OBJECT_SCALING, SINGLE_TOUCH,NONE}
        public class TouchGesture
        {
            private TouchGestureType _gestureType;
            private object _metaData;

            public TouchGestureType GestureType
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
                _gestureType = TouchGestureType.NONE;
                _metaData = null;
            }
        }
        public TouchGesture recognizeGesture(TouchEventData curTouchEvent)
        {
            TouchGesture recognizedGesture = new TouchGesture();
            if(curTouchEvent.PointerCount == 5 && curTouchEvent.EventType == 0)
            {
                recognizedGesture.GestureType = TouchGestureType.FIVE_POINTERS;
                return recognizedGesture;
            }
            if(curTouchEvent.PointerCount == 3)
            {

            }
            if(curTouchEvent.PointerCount == 2)
            {
                if(curTouchEvent.AvaiPointers[0].RelVeloX* curTouchEvent.AvaiPointers[1].RelVeloX>0
                    && curTouchEvent.AvaiPointers[0].RelVeloY*curTouchEvent.AvaiPointers[1].RelVeloY>0)
                {
                    float avgVeloX = (curTouchEvent.AvaiPointers[0].RelVeloX + curTouchEvent.AvaiPointers[1].RelVeloX) / 2;
                    float veloXDif = Math.Abs(curTouchEvent.AvaiPointers[0].RelVeloX - curTouchEvent.AvaiPointers[1].RelVeloX);
                    float avgVeloY = (curTouchEvent.AvaiPointers[0].RelVeloY + curTouchEvent.AvaiPointers[1].RelVeloY) / 2;
                    float veloYDif = Math.Abs(curTouchEvent.AvaiPointers[0].RelVeloY - curTouchEvent.AvaiPointers[1].RelVeloY);
                    //if(veloXDif/avgVeloX < 0.2 && veloYDif/avgVeloY<0.2)
                    //{
                        recognizedGesture.GestureType = TouchGestureType.PAD_TRANSLATING;
                        Vector2 translateVelo = new Vector2();
                        translateVelo.x = avgVeloX / 100;
                        translateVelo.y = avgVeloY / 100;
                        recognizedGesture.MetaData = translateVelo;
                        return recognizedGesture;
                    //}
                    //else
                    //{
                        //return recognizedGesture;
                    //}
                }
                if(curTouchEvent.AvaiPointers[0].RelVeloX * curTouchEvent.AvaiPointers[1].RelVeloX < 0
                    || curTouchEvent.AvaiPointers[0].RelVeloY * curTouchEvent.AvaiPointers[1].RelVeloY < 0)
                {
                    recognizedGesture.GestureType = TouchGestureType.OBJECT_SCALING;
                    return recognizedGesture;
                }
            }
            return recognizedGesture;
        }
    }
}
