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

    Bounds boardObjectBound;

    // Variables for timer;
    float translationTime = 0f;
    Vector2 translationVector = Vector2.zero;

    Camera gameCamera;
    Transform gazePoint;
    IGeneralPointerEventListener eventTouchListener;
    MenuItemListener[] menuItems = new MenuItemListener[2];
    
    GestureRecognizedEventCallback gestureRecognizedCallback = null;
    PointerReceivedEventCallback pointerReceivedCallback = null;

    public enum EditMode { OBJECT_MANIP, DRAW, MENU_SELECTION }

    EditMode _currentMode;
    public EditMode CurrentMode
    {
        get
        {
            return _currentMode;
        }

        set
        {
            _currentMode = value;
        }
    }
    
    CriticVar padTranslationByPointers = new CriticVar();
    CriticVar padScaleByPointers = new CriticVar();
    CriticVar padRotationByPointers = new CriticVar();
    Rect boardFlatBound = new Rect();
    Rect padFlatBound = new Rect();
    Vector2 localPadFlatCenter = new Vector2();
    void Start()
    {
        _currentMode = EditMode.OBJECT_MANIP;
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
        gestureRecognizedCallback = this.gestureRecognizedHandler;
        if(eventTouchListener != null)
        {
            eventTouchListener.SetGestureRecognizedListener(gestureRecognizedCallback);
        }
        menuItems[0] = menuManip.GetComponent<MenuItemListener>();
        menuItems[0].menuSelectedListener += VirtualPadController_menuSelectedListener;
        menuItems[0].CorrespondingMode = EditMode.OBJECT_MANIP;
        menuItems[1] = menuDraw.GetComponent<MenuItemListener>();
        menuItems[1].menuSelectedListener += VirtualPadController_menuSelectedListener;
        menuItems[1].CorrespondingMode = EditMode.DRAW;

        boardObjectBound = boardObject.GetComponent<Collider>().bounds;
        boardFlatBound.min = new Vector2(boardObjectBound.min.x, boardObjectBound.min.y);
        boardFlatBound.max = new Vector2(boardFlatBound.max.x, boardFlatBound.max.y);
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
        
        //Update virtual pad scale
        lock (padScaleByPointers.AccessLock)
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
        if (_currentMode == EditMode.MENU_SELECTION)
        {
            setMenuActiveness(true);
        }
        else
        {
            setMenuActiveness(false);
        }
        //update current size of the virtual flat boundary
        padFlatBound.min = new Vector2(gameObject.GetComponent<Collider>().bounds.min.x, gameObject.GetComponent<Collider>().bounds.min.y);
        padFlatBound.max = new Vector2(gameObject.GetComponent<Collider>().bounds.max.x, gameObject.GetComponent<Collider>().bounds.max.y);
        localPadFlatCenter.Set(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y);
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
        CriticVar curAvaiPointers = eventTouchListener.getCurrentAvaiPointers();
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
        menuManip.SetActive(isActive);
        menuDraw.SetActive(isActive);
    }
    #region handle touch/pointer data
    void gestureRecognizedHandler(TouchGestureRecognizer.TouchGesture recognizedGesture)
    {
        if(_currentMode == EditMode.MENU_SELECTION)
        {
            if (recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.NONE
                || recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.SINGLE_TOUCH_DOWN
                || recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.SINGLE_TAP)
            {
                if(recognizedGesture.GestureType != TouchGestureRecognizer.TouchGestureType.NONE)
                {
                    Vector2 rawLocalTouchPos = (Vector2)recognizedGesture.MetaData;
                    recognizedGesture.MetaData = GlobalUtilities.ConvertMobileRelPosToUnityRelPos(rawLocalTouchPos);
                }
                for (int i = 0; i < menuItems.Length; i++)
                {
                    menuItems[i].HandlePointerGesture(recognizedGesture);
                }
            }
        }
        else
        {
            if (recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.FIVE_POINTERS)
            {
                _currentMode = EditMode.MENU_SELECTION;
            }
            else if (recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.PAD_TRANSLATING)
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
    private void VirtualPadController_menuSelectedListener(EditMode selectedMode)
    {
        _currentMode = selectedMode;
    }
    private Vector2 toLocalPosInBoard(Vector2 localPosInPad)
    {
        return new Vector2();
    }
    #endregion
}
