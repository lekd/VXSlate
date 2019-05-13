using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Script;

namespace Assets.Script
{
    public enum GestureType { PAD_SCALING, PAD_TRANSLATING, FIVE_POINTERS, OBJECT_SCALING, SINGLE_TAP, SINGLE_TOUCH_DOWN, SINGLE_TOUCH_MOVE, NONE }
    public interface IGestureRecognizer
    {
        void setGestureRecognizedListener(GestureRecognizedEventCallback listenerCallback);
        void informGestureRecognizedEvent(TouchGesture recognizedGesture);
        void RecognizeGesture(TouchEventData curTouchEvent);
    }
    public class TouchGestureRecognizer:IGestureRecognizer
    {
        private const int TOUCH_REFRESH_RATE = 60;
        
        RecordedTouch stayStillSingleTouchDown;
        event GestureRecognizedEventCallback gestureRecognizedListener;
        public void setGestureRecognizedListener(GestureRecognizedEventCallback listenerCallback)
        {
            gestureRecognizedListener += listenerCallback;
        }
        public void informGestureRecognizedEvent(TouchGesture recognizedGesture)
        {
            if(gestureRecognizedListener != null)
            {
                gestureRecognizedListener(recognizedGesture);
            }
        }
        
        public void RecognizeGesture(TouchEventData curTouchEvent)
        {
            TouchGesture recognizedGesture = new TouchGesture();
            //if there are more than one fingers, there is no chance for a single tap
            if(curTouchEvent.PointerCount > 1)
            {
                stayStillSingleTouchDown = null;
            }
            if(curTouchEvent.PointerCount == 1 && 
                (Math.Abs(curTouchEvent.AvaiPointers[0].RelVeloX)>0.1 || Math.Abs(curTouchEvent.AvaiPointers[0].RelVeloY) > 0.1))
            {
                stayStillSingleTouchDown = null;
            }
            //process cases with multi fingers
            if(curTouchEvent.PointerCount == 5)// && curTouchEvent.EventType == 0)
            {
                recognizedGesture.GestureType = GestureType.FIVE_POINTERS;
                informGestureRecognizedEvent(recognizedGesture);
                return;
            }
            //recognizing scaling virtual pad gesture
            if(curTouchEvent.PointerCount == 3 && curTouchEvent.EventType == 1)
            {
                bool isScalingPad = recognizePadScalingGesture(curTouchEvent.AvaiPointers, out recognizedGesture);
                informGestureRecognizedEvent(recognizedGesture);
            }
            //recognizing pad translating gesture
            if(curTouchEvent.PointerCount == 2)
            {
                //two fingers moving in the same direction => translating the virtual pad
                if(curTouchEvent.AvaiPointers[0].RelVeloX* curTouchEvent.AvaiPointers[1].RelVeloX>0
                    && curTouchEvent.AvaiPointers[0].RelVeloY*curTouchEvent.AvaiPointers[1].RelVeloY>0)
                {
                    float avgVeloX = (curTouchEvent.AvaiPointers[0].RelVeloX + curTouchEvent.AvaiPointers[1].RelVeloX) / 2;
                    float veloXDif = Math.Abs(curTouchEvent.AvaiPointers[0].RelVeloX - curTouchEvent.AvaiPointers[1].RelVeloX);
                    float avgVeloY = (curTouchEvent.AvaiPointers[0].RelVeloY + curTouchEvent.AvaiPointers[1].RelVeloY) / 2;
                    float veloYDif = Math.Abs(curTouchEvent.AvaiPointers[0].RelVeloY - curTouchEvent.AvaiPointers[1].RelVeloY);
                    if (veloXDif / avgVeloX < 0.2 && veloYDif / avgVeloY < 0.2)
                    {
                        recognizedGesture.GestureType = GestureType.PAD_TRANSLATING;
                        Vector2 translateVelo = new Vector2();
                        translateVelo.x = avgVeloX / TOUCH_REFRESH_RATE;
                        translateVelo.y = avgVeloY / TOUCH_REFRESH_RATE;
                        recognizedGesture.MetaData = translateVelo;
                        informGestureRecognizedEvent(recognizedGesture);
                        return;
                    }
                }
                //two fingers moving in opposite direction => object scaling
                else if(curTouchEvent.AvaiPointers[0].RelVeloX * curTouchEvent.AvaiPointers[1].RelVeloX < 0
                    && curTouchEvent.AvaiPointers[0].RelVeloY * curTouchEvent.AvaiPointers[1].RelVeloY < 0)
                {
                    recognizedGesture.GestureType = GestureType.OBJECT_SCALING;
                    Vector2 futurePointer1 = new Vector2();
                    futurePointer1.x = curTouchEvent.AvaiPointers[0].RelX + curTouchEvent.AvaiPointers[0].RelVeloX * TOUCH_REFRESH_RATE;
                    futurePointer1.y = curTouchEvent.AvaiPointers[0].RelY + curTouchEvent.AvaiPointers[0].RelVeloY * TOUCH_REFRESH_RATE;
                    Vector2 futurePointer2 = new Vector2();
                    futurePointer2.x = curTouchEvent.AvaiPointers[1].RelX + curTouchEvent.AvaiPointers[1].RelVeloX * TOUCH_REFRESH_RATE;
                    futurePointer2.y = curTouchEvent.AvaiPointers[1].RelY + curTouchEvent.AvaiPointers[1].RelVeloY * TOUCH_REFRESH_RATE;
                    Vector2 curPointersDif = new Vector2(curTouchEvent.AvaiPointers[1].RelX - curTouchEvent.AvaiPointers[0].RelX,
                                                        curTouchEvent.AvaiPointers[1].RelY - curTouchEvent.AvaiPointers[0].RelY);
                    double curPointersDist = Math.Sqrt(curPointersDif.x * curPointersDif.x + curPointersDif.y * curPointersDif.y);
                    Vector2 futurePointersDif = new Vector2(futurePointer2.x - futurePointer1.x, futurePointer2.y - futurePointer1.y);
                    double futurePointersDist = Math.Sqrt(futurePointersDif.x * futurePointersDif.x + futurePointersDif.y * futurePointersDif.y);
                    Vector2 scaleRatio = new Vector2((float)(futurePointersDist / curPointersDist), (float)(futurePointersDist / curPointersDist));
                    Vector2[] gestureData = new Vector2[3];
                    gestureData[0] = scaleRatio;
                    gestureData[1] = new Vector2(curTouchEvent.AvaiPointers[0].RelX, curTouchEvent.AvaiPointers[0].RelY);
                    gestureData[2] = new Vector2(curTouchEvent.AvaiPointers[1].RelX, curTouchEvent.AvaiPointers[1].RelY);
                    recognizedGesture.MetaData = gestureData;
                    informGestureRecognizedEvent(recognizedGesture);
                    return;
                }
            }
            //process when there is only one finger
            if(curTouchEvent.EventType == 0 && curTouchEvent.PointerCount == 1)
            {
                stayStillSingleTouchDown = new RecordedTouch();
                stayStillSingleTouchDown.TouchPointers.Add(TouchPointerData.Create(curTouchEvent.AvaiPointers[0]));
                recognizedGesture.GestureType = GestureType.SINGLE_TOUCH_DOWN;
                //recognizedGesture.MetaData =  TouchPointerData.Create(curTouchEvent.AvaiPointers[0]);
                recognizedGesture.MetaData = new Vector2(curTouchEvent.AvaiPointers[0].RelX, curTouchEvent.AvaiPointers[0].RelY);
                informGestureRecognizedEvent(recognizedGesture);
                return ;
            }
            if (curTouchEvent.EventType == 1 && curTouchEvent.PointerCount == 1)
            {
                recognizedGesture.GestureType = GestureType.SINGLE_TOUCH_MOVE;
                //recognizedGesture.MetaData =  TouchPointerData.Create(curTouchEvent.AvaiPointers[0]);
                recognizedGesture.MetaData = new Vector2(curTouchEvent.AvaiPointers[0].RelX, curTouchEvent.AvaiPointers[0].RelY);
                informGestureRecognizedEvent(recognizedGesture);
                return;
            }
            if (curTouchEvent.EventType == 2 && curTouchEvent.PointerCount == 0)
            {
                //first inform a non gesture for client to process if needed
                TouchGesture non_gesture = new TouchGesture();
                informGestureRecognizedEvent(non_gesture);
                RecordedTouch recordedTouchUp = new RecordedTouch();
                if(stayStillSingleTouchDown != null)
                {
                    TimeSpan touchDuration = recordedTouchUp.TimeStamp.Subtract(stayStillSingleTouchDown.TimeStamp);
                    if (touchDuration.TotalMilliseconds < 500)
                    {
                        recognizedGesture.GestureType = GestureType.SINGLE_TAP;
                        recognizedGesture.MetaData = new Vector2(stayStillSingleTouchDown.TouchPointers[0].RelX, stayStillSingleTouchDown.TouchPointers[0].RelY);
                    }
                    stayStillSingleTouchDown = null;
                    informGestureRecognizedEvent(recognizedGesture);
                    return;
                }
            }
        }

        bool recognizePadScalingGesture(TouchPointerData[] avaiPointers, out TouchGesture recognizedGesture)
        {
            recognizedGesture = new TouchGesture();
            TouchPointerData anchorFinger = null;
            TouchPointerData[] movingFingers = new TouchPointerData[2];
            int movingFingersIdx = 0;
            //classifying anchor and moving fingers
            for (int i = 0; i < avaiPointers.Length; i++)
            {
                if (Math.Abs(avaiPointers[i].RelVeloX) < 0.01 && Math.Abs(avaiPointers[i].RelVeloY) < 0.01)
                {
                    anchorFinger = avaiPointers[i];
                }
                else
                {
                    if (movingFingersIdx < movingFingers.Length)
                    {
                        movingFingers[movingFingersIdx++] = avaiPointers[i];
                    }
                }
            }
            //check movement of moving fingers to see if they are moving in the same direction
            if (anchorFinger == null || movingFingersIdx != movingFingers.Length)
            {
                return false;
            }
            if (movingFingers[0].RelVeloX * movingFingers[1].RelVeloX < 0 || movingFingers[0].RelVeloY * movingFingers[1].RelVeloY < 0)
            {
                return false;
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
                recognizedGesture.GestureType = GestureType.PAD_SCALING;
                recognizedGesture.MetaData = scaleRatio;
                return true;
            }
        }
    }
}
