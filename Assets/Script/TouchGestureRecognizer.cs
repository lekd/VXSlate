using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script
{
    public class TouchGestureRecognizer
    {
        private const int TOUCH_REFRESH_RATE = 60;
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
            //recognizing scaling virtual pad gesture
            if(curTouchEvent.PointerCount == 3 && curTouchEvent.EventType == 1)
            {
                TouchPointerData anchorFinger = null;
                TouchPointerData[] movingFingers = new TouchPointerData[2];
                int movingFingersIdx = 0;
                //classifying anchor and moving fingers
                for(int i=0; i< curTouchEvent.AvaiPointers.Length; i++)
                {
                    if(Math.Abs(curTouchEvent.AvaiPointers[i].RelVeloX) < 0.0001 && Math.Abs(curTouchEvent.AvaiPointers[i].RelVeloY)<0.0001)
                    {
                        anchorFinger = curTouchEvent.AvaiPointers[i];
                    }
                    else
                    {
                        if(movingFingersIdx < movingFingers.Length)
                        {
                            movingFingers[movingFingersIdx++] = curTouchEvent.AvaiPointers[i];
                        }
                    }
                }
                //check movement of moving fingers to see if they are moving in the same direction
                if (anchorFinger == null || movingFingersIdx != movingFingers.Length)
                {
                    recognizedGesture.GestureType = TouchGestureType.NONE;
                    return recognizedGesture;
                }
                if (movingFingers[0].RelVeloX * movingFingers[1].RelVeloX < 0 || movingFingers[0].RelVeloY * movingFingers[1].RelVeloY < 0)
                {
                    recognizedGesture.GestureType = TouchGestureType.NONE;
                    return recognizedGesture;
                }
                else
                {
                    Vector2 anchorPos = new Vector2(anchorFinger.RelX, anchorFinger.RelY);
                    Vector2 avgMovingVelo = new Vector2();
                    avgMovingVelo.x = (movingFingers[0].RelVeloX + movingFingers[1].RelVeloX) / 2;
                    avgMovingVelo.y = (movingFingers[0].RelVeloY + movingFingers[1].RelVeloY) / 2;
                    Vector2 curMovingCenter = new Vector2();
                    curMovingCenter.x = (movingFingers[0].RelX + movingFingers[1].RelX) / 2;
                    curMovingCenter.y = (movingFingers[0].RelY + movingFingers[1].RelY) / 2;
                    Vector2 futureMovingCenter = new Vector2();
                    futureMovingCenter.x = curMovingCenter.x + avgMovingVelo.x / TOUCH_REFRESH_RATE;
                    futureMovingCenter.y = curMovingCenter.y + avgMovingVelo.y / TOUCH_REFRESH_RATE;
                    float curDistanceToAnchor = Vector2.Distance(curMovingCenter, anchorPos);
                    float futureDistanceToAnchor = Vector2.Distance(futureMovingCenter, anchorPos);
                    Vector2 scaleRatio = new Vector2(futureDistanceToAnchor / curDistanceToAnchor, futureDistanceToAnchor / curDistanceToAnchor);
                    recognizedGesture.GestureType = TouchGestureType.PAD_SCALING;
                    recognizedGesture.MetaData = scaleRatio;
                    return recognizedGesture;
                }
            }
            //recognizing pad translating gesture
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
                        translateVelo.x = avgVeloX / TOUCH_REFRESH_RATE;
                        translateVelo.y = avgVeloY / TOUCH_REFRESH_RATE;
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
