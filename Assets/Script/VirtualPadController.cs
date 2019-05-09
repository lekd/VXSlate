using Assets;
using Assets.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualPadController : MonoBehaviour
{
    const int MAX_FINGERS_COUNT = 5;
    const float VIRTUALPAD_DEPTHOFFSET = -0.01f;
    //Public Game Objects
    public GameObject eventListenerObject;
    public GameObject gameCameraObject;
    public GameObject gazePointObject;
    public GameObject boardObject;
    public GameObject finger1;
    public GameObject finger2;
    public GameObject finger3;
    public GameObject finger4;
    public GameObject finger5;
    public GameObject menuManip;
    public GameObject menuDraw;
    GameObject[] fingers;

    public Material menuManipNormalMat;
    public Material menuManipPressedMat;
    public Material menuDrawNormalMat;
    public Material menuDrawPressedMat;

    Bounds boardObjectBound;

    // Variables for timer;
    float translationTime = 0f;
    Vector2 translationVector = Vector2.zero;

    Camera gameCamera;
    Transform gazePoint;
    IGeneralPointerEventListener eventTouchListener;
    
    GestureRecognizedEventCallback gestureRecognizedCallback = null;
    PointerReceivedEventCallback pointerReceivedCallback = null;

    public enum VirtualPadState { OBJECT_MANIP, DRAW, MENU_SELECTION }

    VirtualPadState _currentState;

    public VirtualPadState CurrentState
    {
        get
        {
            return _currentState;
        }

        set
        {
            _currentState = value;
        }
    }

    CriticVar curAvaiPointers = new CriticVar();
    CriticVar padTranslationByPointers = new CriticVar();
    CriticVar padScaleByPointers = new CriticVar();
    CriticVar padRotationByPointers = new CriticVar();
    void Start()
    {
        _currentState = VirtualPadState.OBJECT_MANIP;
        if (gameCameraObject)
        {
            gameCamera = gameCameraObject.GetComponent<Camera>();
        }

        if (gazePointObject)
        {
            gazePoint = gazePointObject.GetComponent<Transform>();
        }
        if(eventListenerObject)
        {
            eventTouchListener = eventListenerObject.GetComponent<TabletTouchEventManager>();
        }
        pointerReceivedCallback = this.pointerReceivedHandler;
        gestureRecognizedCallback = this.gestureRecognizedHandler;
        if(eventTouchListener != null)
        {
            eventTouchListener.SetTouchReceivedEventListener(pointerReceivedCallback);
            eventTouchListener.SetGestureRecognizedListener(gestureRecognizedCallback);
        }


        boardObjectBound = boardObject.GetComponent<Collider>().bounds;
        fingers = new GameObject[] { finger1, finger2, finger3, finger4, finger5 };
        for(int i=0; i< fingers.Length; i++)
        {
            fingers[i].SetActive(false);
        }

        padTranslationByPointers.CriticData = new Vector2(0, 0);
        padScaleByPointers.CriticData = new Vector2(0, 0);


        setMenuActiveness(false);
    }

    // Update is called once per frame
    void Update()
    {
        /*Debug.Log(center);

        // Translate the pad to a new position
        if(translationTime > 0)
        {
            translationTime -= Time.deltaTime;
            this.transform.Translate(translationVector * Time.deltaTime);
        }
        else if(translationTime < 0)
        {
            translationTime = 0;
        }*/

        //Update virtual pad scale
        lock(padScaleByPointers.AccessLock)
        {
            Vector2 scaleRatio = (Vector2)padScaleByPointers.CriticData;
            if(scaleRatio.x != 0 && scaleRatio.y != 0)
            {
                Vector3 curScale = gameObject.transform.localScale;
                gameObject.transform.localScale = new Vector3(scaleRatio.x * curScale.x, scaleRatio.y * curScale.y, curScale.z);
                padScaleByPointers.CriticData = new Vector2(0, 0);
            }
        }
        //get the 2D boundary of the board, which serve as the position limits of the virtual pad
        Bounds virtualPadBound = gameObject.GetComponent<Collider>().bounds;
        Rect board2DBound = new Rect();
        board2DBound.xMin = boardObjectBound.min.x + virtualPadBound.size.x / 2;
        board2DBound.xMax = boardObjectBound.max.x - virtualPadBound.size.x / 2;
        board2DBound.yMin = boardObjectBound.min.y + virtualPadBound.size.y / 2;
        board2DBound.yMax = boardObjectBound.max.y - virtualPadBound.size.y / 2;
        //Update Virtual Pad Following Camera Lookat Vector
        UpdateVirtualPadBasedOnCamera(board2DBound);
        UpdateFingersBasedOnTouch();
        //Update virtual pad translation caused by pointers
        lock(padTranslationByPointers.AccessLock)
        {
            
            Vector2 relTranslation = (Vector2)padTranslationByPointers.CriticData;
            if (relTranslation.x != 0 || relTranslation.y != 0)
            {
                Vector2 translationByTouch = new Vector2();
                translationByTouch.x = relTranslation.x * gameObject.GetComponent<Collider>().bounds.size.x;
                translationByTouch.y = -relTranslation.y * gameObject.GetComponent<Collider>().bounds.size.y;
                Vector3 curPos = gameObject.transform.position;
                Vector3 newPos = new Vector3(curPos.x + translationByTouch.x, curPos.y + translationByTouch.y, curPos.z);
                newPos = GlobalUtilities.boundPointToContainer(newPos, board2DBound);
                gameObject.transform.Translate(newPos.x - curPos.x, newPos.y - curPos.y, 0);
                padTranslationByPointers.CriticData = new Vector2(0, 0);
            }
        }
    }

    private void UpdateVirtualPadBasedOnCamera(Rect board2DBound)
    {

        if (!gameCamera)
        {
            return;
        }

        RaycastHit hitInfo;
        Ray camRay = new Ray(gazePointObject.transform.position, gazePointObject.transform.position - gameCamera.transform.position);
        if (Physics.Raycast(camRay, out hitInfo))
        {
            if (hitInfo.collider != null)
            {
                if (hitInfo.collider.name.CompareTo(boardObject.name) == 0)
                {
                    Vector3 hitPos = hitInfo.point;
                    Vector3 curPadPos = gameObject.transform.position;
                    Vector3 newPadPos = GlobalUtilities.boundPointToContainer(hitPos, board2DBound);
                    gameObject.transform.Translate(new Vector3(newPadPos.x - curPadPos.x, newPadPos.y - curPadPos.y, newPadPos.z - curPadPos.z + VIRTUALPAD_DEPTHOFFSET));
                }
            }
        }
    }

    private void UpdateFingersBasedOnTouch()
    {
        if(fingers == null)
        {
            fingers = new GameObject[] { finger1, finger2, finger3, finger4, finger5 };
        }
        lock (curAvaiPointers.AccessLock)
        {
            if(curAvaiPointers.CriticData == null)
            {
                return;
            }
            TouchPointerData[] avaiPointers = (TouchPointerData[])curAvaiPointers.CriticData;
            for (int i = 0; i < fingers.Length; i++)
            {
                GameObject finger = fingers[i];
                if (finger == null)
                {
                    continue;
                }
                if (i < avaiPointers.Length)
                {
                    finger.SetActive(true);

                    Vector2 relPosInPad = GlobalUtilities.ConvertMobileRelPosToUnityRelPos(new Vector2(avaiPointers[i].RelX, avaiPointers[i].RelY));
                    finger.transform.localPosition = new Vector3(relPosInPad.x, relPosInPad.y, finger.transform.localPosition.z);

                }
                else
                {
                    finger.SetActive(false);
                }
            }
        }
    }
    private void setMenuActiveness(bool isActive)
    {
        if(menuManip)
        {
            menuManip.GetComponent<MeshRenderer>().material = menuManipNormalMat;
            menuManip.SetActive(isActive);
        }
        if(menuDraw)
        {
            menuDraw.GetComponent<MeshRenderer>().material = menuDrawNormalMat;
            menuDraw.SetActive(isActive);
        }
    }
    #region handle touch/pointer data
    void pointerReceivedHandler(TouchEventData touchEvent)
    {
        lock(curAvaiPointers.AccessLock)
        {
            curAvaiPointers.CriticData = touchEvent.AvaiPointers;
        }
    }
    void gestureRecognizedHandler(TouchGestureRecognizer.TouchGesture recognizedGesture)
    {
        if(_currentState == VirtualPadState.MENU_SELECTION)
        {

        }
        else
        {
            if(recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.PAD_TRANSLATING)
            {
                Vector2 eventMetaData = (Vector2)recognizedGesture.MetaData;
                lock (padTranslationByPointers.AccessLock)
                {
                    padTranslationByPointers.CriticData = eventMetaData;
                }
            }
            else if(recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.PAD_SCALING)
            {
                lock(padScaleByPointers)
                {
                    padScaleByPointers.CriticData = recognizedGesture.MetaData;
                }
            }
        }
    }
    #endregion
}
