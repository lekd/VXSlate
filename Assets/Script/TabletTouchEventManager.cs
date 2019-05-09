using Assets;
using Assets.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletTouchEventManager : MonoBehaviour
{
    WebSocketSharp.WebSocket wsClient;
    // Start is called before the first frame update
    void Start()
    {
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
        if (e.IsText)
        {
            string msg = e.Data;
            //Debug.Log(msg);
            try
            {
                //Debug.Log(msg);
                TouchEventData touchEvent = TouchEventData.ParseToucEventFromJsonString(msg);
                //string decodedMsg = "From JSON: ";
                //decodedMsg += string.Format("Event type:{0};Pointers count: {1};AvaiPointers:{2}", touchEvent.EventType, touchEvent.PointerCount, touchEvent.AvaiPointers.Length);
                
            }
            catch (Exception ex)
            {
                Debug.Log("JSON Parsing Error:" + ex.Message);
            }
        }
    }
    void OnApplicationQuit()
    {
        if (wsClient != null)
        {
            wsClient.Close();
            wsClient = null;
        }
    }
}
