using Assets;
using Assets.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public class BoardEventListener : MonoBehaviour
{
    //GameObject board;
    // Start is called before the first frame update
    const float FOCUSWINDOW_DEPTHOFFSET = 0.02f;
    const float GAZEPOINTER_DEPTHOFFSET = 0.01f;
    const int MAX_POINTERS = 5;
    Vector3 camScreenCenter = new Vector3(0,0,0);
    GameObject board;
    Bounds boardBound;
    GameObject focusWindow;
    GameObject gazePointer;
    WebSocketSharp.WebSocket wsClient;
    //simple handling of touch pointers for UI updateing
    System.Object pointerUpdateLock = new System.Object();
    TouchEventData latestTouchEvent = new TouchEventData();
    Material pointerInviMat;
    Material pointerVisMat;
    bool canGazeControl = true;
    System.Object gazeControlLocker = new System.Object();

    TouchGestureRecognizer gestureRecognizer = new TouchGestureRecognizer();
    void Start()
    {
        //StartCoroutine(VRActivator("Cardboard"));
        board = GameObject.Find("Board");
        boardBound = board.transform.GetComponent<Collider>().bounds;
        camScreenCenter = Camera.main.WorldToScreenPoint(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane)));
        focusWindow = GameObject.Find("FocusWindow");
        gazePointer = GameObject.Find("GazePointer");
        pointerInviMat = Resources.Load("Materials/TransparentMat", typeof(Material)) as Material;
        pointerVisMat = Resources.Load("Materials/PointerMarkMat", typeof(Material)) as Material;
        connectWebSocketServer();
    }
    public IEnumerator VRActivator(string deviceName)
    {
        XRSettings.LoadDeviceByName(deviceName);
        yield return null;
        XRSettings.enabled = true;
    }
    void OnApplicationQuit()
    {
        wsClient.Close();
    }
    void connectWebSocketServer()
    {
        wsClient = new WebSocketSharp.WebSocket(string.Format("ws://{0}/main.html", GlobalData.LoadServerAddress()));
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
                lock(pointerUpdateLock)
                {
                    latestTouchEvent.Clone(touchEvent);
                }
                HandleTouchEvent(touchEvent);
            }
            catch(Exception ex)
            {
                Debug.Log("JSON Parsing Error:" + ex.Message);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(camScreenCenter);
        //check current gaze
        if(Physics.Raycast(ray, out hitInfo))
        {
            if(hitInfo.collider != null)
            {
                //gazePointer.transform.position = hitInfo.point;
                if (hitInfo.collider.name.CompareTo("Board") == 0 || hitInfo.collider.name.CompareTo("FocusWindow") == 0)
                {
                    //Debug.Log("Hit object: " + hitInfo.collider.name);
                    gazePointer.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z - GAZEPOINTER_DEPTHOFFSET);
                    if (hitInfo.collider.name.CompareTo("Board") == 0)
                    {
                        Vector3 newPos = hitInfo.point;
                        Vector3 curPos = focusWindow.transform.position;
                        Bounds fwBound = focusWindow.GetComponent<Collider>().bounds;
                        Bounds nextBound = new Bounds(newPos, fwBound.size);
                        if (!boardBound.Contains(nextBound.min))
                        {
                            if (nextBound.min.x < boardBound.min.x)
                            {
                                newPos.x = newPos.x + (boardBound.min.x - nextBound.min.x);
                            }
                            if (nextBound.min.y < boardBound.min.y)
                            {
                                newPos.y = newPos.y + (boardBound.min.y - nextBound.min.y);
                            }
                        }
                        if (!boardBound.Contains(nextBound.max))
                        {
                            if (nextBound.max.x > boardBound.max.x)
                            {
                                newPos.x = newPos.x - (nextBound.max.x - boardBound.max.x);
                            }
                            if (nextBound.max.y > boardBound.max.y)
                            {
                                newPos.y = newPos.y - (nextBound.max.y - boardBound.max.y);
                            }
                        }
                        //focusWindow.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z - 0.02f);
                        lock (gazeControlLocker)
                        {
                            if (canGazeControl)
                            {
                                focusWindow.transform.Translate(new Vector3(newPos.x - curPos.x, newPos.y - curPos.y, newPos.z - FOCUSWINDOW_DEPTHOFFSET - curPos.z));
                            }
                        }
                        //Debug.Log("Collision Point: " + hitInfo.point.ToString());
                    }
                    
                }
            }
        }
        lock (gazeControlLocker)
        {
            if(!canGazeControl && (focusWindowRelTouchTranslate.x != 0 || focusWindowRelTouchTranslate.y != 0))
            {
                Vector2 translationByTouch = new Vector2();
                translationByTouch.x = focusWindowRelTouchTranslate.x * focusWindow.GetComponent<Collider>().bounds.size.x;
                translationByTouch.y = -focusWindowRelTouchTranslate.y * focusWindow.GetComponent<Collider>().bounds.size.y;
                focusWindow.transform.Translate(translationByTouch.x, translationByTouch.y, 0);
            }
        }
        //Process UI based on touch input
        UpdateTouchPointersViz(latestTouchEvent);
    }
    void UpdateTouchPointersViz(TouchEventData latestTouchEvent)
    {
        Bounds virtualPadArea = focusWindow.GetComponent<Collider>().bounds;
        
        lock(pointerUpdateLock)
        {
            //Update visibility
            for(int i=0;i<MAX_POINTERS;i++)
            {
                GameObject pointer = GameObject.Find(string.Format("Pointer{0}", i + 1));
                if(i < latestTouchEvent.PointerCount)
                {
                    //pointer.GetComponent<MeshRenderer>().material = pointerVisMat;
                    Material[] pMats = pointer.GetComponent<MeshRenderer>().materials;
                    if(pMats.Length>0)
                    {
                        pMats[0] = pointerVisMat;
                        pointer.GetComponent<MeshRenderer>().materials = pMats;
                    }
                    //compute position in virtual pad
                    Vector3 pos = new Vector3();
                    pos.x = virtualPadArea.min.x + virtualPadArea.size.x * latestTouchEvent.AvaiPointers[i].RelX;
                    pos.y = virtualPadArea.max.y - virtualPadArea.size.y * latestTouchEvent.AvaiPointers[i].RelY;
                    pos.z = virtualPadArea.min.z;
                    pointer.transform.position = pos;
                }
                else
                {
                    pointer.GetComponent<Renderer>().material = pointerInviMat;
                    pointer.transform.position = virtualPadArea.min;
                }
            }
            
        }
    }

    object focusWindowTouchTranslateLock = new object();
    Vector2 focusWindowRelTouchTranslate = new Vector2();
    void HandleTouchEvent(TouchEventData touchEvent)
    {
        focusWindowRelTouchTranslate.Set(0, 0);
        TouchGestureRecognizer.TouchGesture recognizedGesture = gestureRecognizer.recognizeGesture(touchEvent);
        if(recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.PAD_TRANSLATING)
        {
            lock (gazeControlLocker)
            {
                canGazeControl = false;
            }
            Vector2 eventMetaData = (Vector2)recognizedGesture.MetaData;
            lock (focusWindowTouchTranslateLock)
            {
                focusWindowRelTouchTranslate.Set(eventMetaData.x, eventMetaData.y);
            }
            //Debug.Log("Translating Focus Window");
            return;
        }
        if(recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.NONE)
        {
            lock (gazeControlLocker)
            {
                canGazeControl = true;
            }
        }
    }
}
