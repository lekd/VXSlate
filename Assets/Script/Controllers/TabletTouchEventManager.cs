using Assets;
using Assets.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TabletTouchEventManager : MonoBehaviour, IGeneralPointerEventListener
{
    WebSocketSharp.WebSocket wsClient;
    TouchGestureRecognizer gestureRecognizer = new TouchGestureRecognizer();
    event PointerReceivedEventCallback pointerReceivedListener;
    public void SetGestureRecognizedListener(GestureRecognizedEventCallback eventRecognizedCallback)
    {
        gestureRecognizer.setGestureRecognizedListener(eventRecognizedCallback);
    }
    CriticVar _pointersData;

    public CriticVar getCurrentAvaiPointers()
    {
        return _pointersData;
    }

    // Start is called before the first frame update
    void Start()
    {
        _pointersData = new CriticVar();
        connectWebSocketServer();
    }
    void connectWebSocketServer()
    {
        wsClient = new WebSocketSharp.WebSocket(string.Format("ws://{0}/main.html", GlobalUtilities.LoadServerAddress()));
        wsClient.OnMessage += WsClient_OnMessage;
        wsClient.Connect();
    }
    private void WsClient_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
    {
        //return;
        if (e.IsBinary)
        {
            byte[] rawMsg = e.RawData;
            string header = "TOUCH";
            byte[] headerBuffer = Encoding.Default.GetBytes(header);
            header = Encoding.UTF8.GetString(headerBuffer);
            int headerEnd = FindMessageHeader(rawMsg, header);
            TouchEventData touchEvent = TouchEventData.ParseTouchEventFromBytes(rawMsg, headerEnd);
            if (pointerReceivedListener != null)
            {
                pointerReceivedListener(touchEvent);
            }
            gestureRecognizer.RecognizeGesture(touchEvent);
            lock (_pointersData.AccessLock)
            {
                _pointersData.CriticData = touchEvent.AvaiPointers;
            }
            //Debug.Log(touchEvent.toString());
        }
        /*
        if (e.IsText)
        {
            string msg = e.Data;
            //Debug.Log(msg);
            try
            {
                //Debug.Log(msg);
                TouchEventData touchEvent = TouchEventData.ParseToucEventFromJsonString(msg);
                if(pointerReceivedListener != null)
                {
                    pointerReceivedListener(touchEvent);
                }
                gestureRecognizer.RecognizeGesture(touchEvent);
                //string decodedMsg = "From JSON: ";
                //decodedMsg += string.Format("Event type:{0};Pointers count: {1};AvaiPointers:{2}", touchEvent.EventType, touchEvent.PointerCount, touchEvent.AvaiPointers.Length);
                lock(_pointersData.AccessLock)
                {
                    _pointersData.CriticData = touchEvent.AvaiPointers;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("JSON Parsing Error:" + ex.Message);
            }
        }*/
    }
    void OnApplicationQuit()
    {
        if (wsClient != null)
        {
            wsClient.Close();
            wsClient = null;
        }
    }

    int FindMessageHeader(byte[] msgBytes,string header)
    {
        int headerIndex = -1;
        byte[] headerBuffer = Encoding.UTF8.GetBytes(header);
        byte[] searchBuffer = new byte[headerBuffer.Length];
        for(int i=0; i < msgBytes.Length; i++)
        {
            Array.Copy(msgBytes, i, searchBuffer, 0, headerBuffer.Length);
            string candidate = Encoding.UTF8.GetString(searchBuffer);
            if(candidate.CompareTo(header) == 0)
            {
                headerIndex = i + headerBuffer.Length;
                break;
            }
        }
        return headerIndex;
    }

    
}
