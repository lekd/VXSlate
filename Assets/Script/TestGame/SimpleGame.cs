using Assets.Script;
using BoardGame;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SimpleGame : MonoBehaviour
{
    [Header("Objects Mapping")]
    public GameObject _puzzleMakerObject;
    public GameObject _eventListennerObject;

    [Header("Experiment Log")]
    public string _participantID;
    public int _textureID = 1;
    public bool _isExperimentStarted = false;
    public bool _isExperimentFinished = false;
    public float _prepareTime = 3; //in seconds

    bool _startTimer = false;

    [Header("Experiment Objects")]
    public GameObject tabletControllerObj;
    public GameObject oculusControllerObj;
    public GameObject mouseControllerObj;
    public GameObject gameCharacterObj;
    private Texture2D screenTexture;
    Vector2 gameSize = new Vector2();

    IRemoteController tabletController;
    IRemoteController mouseController;
    IRemoteController oculusController;

    SimpleCharacter gameCharacter;
    Color[] paintColors = new Color[1];
    Point2D drawnPoint = new Point2D();
    bool hasThingToDraw = false;
    EditMode gameMode;
    Vector3 screenSize = new Vector3();

    PuzzleMaker _puzzleMaker;
    Piece _selectedPiece = null;
    
    bool hasTouchDown = false;
    Vector3 difPosition = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        gameCharacterObj.transform.localPosition.Set(0, 0, -0.00001f);

        tabletController = tabletControllerObj.GetComponent<IRemoteController>();
        if(tabletController != null)
        {
            tabletController.setGestureRecognizedCallback(this.handleControlGesture);
            tabletController.setModeSwitchedCallback(this.handleEditModeChanged);
        }
        else
        {
            Debug.LogWarning("Missing Tablet Controller Object!", _puzzleMakerObject);
        }

        mouseController = mouseControllerObj.GetComponent<IRemoteController>();
        if (mouseController != null)
        {
            mouseController.setGestureRecognizedCallback(this.handleControlGesture);
            mouseController.setModeSwitchedCallback(this.handleEditModeChanged);
        }
        else
        {
            Debug.LogWarning("Missing Mouse Controller Object!", _puzzleMakerObject);
        }

        oculusController = oculusControllerObj.GetComponent<IRemoteController>();
        if (oculusController != null)
        {
            oculusController.setGestureRecognizedCallback(this.handleControlGesture);
            oculusController.setModeSwitchedCallback(this.handleEditModeChanged);
        }
        else
        {
            Debug.LogWarning("Missing Oculus Controller Object!", _puzzleMakerObject);
        }

        

        hasTouchDown = false;


        gameCharacter = gameCharacterObj.GetComponent<SimpleCharacter>();
        paintColors[0] = new Color(1, 0, 0);
        screenTexture = new Texture2D(2, 2);
        //load screen texture from image
        string textureImgPath = "./Assets/Resources/Images/solid_gray.png";
        byte[] imgData;
        if(File.Exists(textureImgPath))
        {
            imgData = File.ReadAllBytes(textureImgPath);
            screenTexture.LoadImage(imgData);
        }
        Debug.Log(string.Format("ScreenTexture size: ({0},{1})", screenTexture.width, screenTexture.height));
        gameMode = EditMode.OBJECT_MANIP;
        screenSize = gameObject.GetComponent<Collider>().bounds.size;
    }

    // Update is called once per frame
    void Update()
    {
        if (_startTimer && _prepareTime > 0)
        {
            _puzzleMaker._statusObject.GetComponent<Text>().text = "ARE YOU READY?\n" + ((int)_prepareTime + 1).ToString();
            _puzzleMaker._statusObject.GetComponent<Text>().font = _puzzleMaker._statusFont;
            _puzzleMaker._statusObject.GetComponent<Text>().color = Color.blue;
            _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 28;
            _puzzleMaker._statusObject.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            _prepareTime -= Time.deltaTime;
        }
        else if (_prepareTime < 0 && !_isExperimentStarted && _puzzleMaker._isInit)
        {
            _prepareTime = 0;
            _isExperimentStarted = true;
            _startTimer = false;

            _puzzleMaker.SetObjectsTransparency(0.5f);

            Debug.Log(">> Experiment Started!");

            _puzzleMaker._statusObject.GetComponent<Text>().text = "Experiment Started!";
            _puzzleMaker._statusObject.GetComponent<Text>().font = _puzzleMaker._statusFont;
            _puzzleMaker._statusObject.GetComponent<Text>().color = Color.black;
            _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 4;
            _puzzleMaker._statusObject.GetComponent<Text>().alignment = TextAnchor.LowerCenter;
        }
        else if (_isExperimentStarted && _puzzleMaker._isInit)
        {
            if (!_isExperimentFinished)
            {
                _puzzleMaker.HighlightGridPiece();

                if (!_puzzleMaker.isPuzzledDone && _puzzleMaker.CheckPuzzlesDone())
                {
                    _puzzleMaker.isPuzzledDone = true;
                    _puzzleMaker._puzzleDoneObject.SetActive(true);
                }

                ////For testing
                //_puzzleMaker.isPuzzledDone = true;
                /////

                if (_puzzleMaker.isPuzzledDone && !_puzzleMaker.isSketchStarted)
                {
                    _puzzleMaker.isSketchStarted = true;

                    ////For testing
                    //_puzzleMaker._puzzleDoneObject.SetActive(true);
                    //////

                    if (_puzzleMaker._sketchedPixels == null)
                        _puzzleMaker._sketchedPixels = new List<Pixel>();

                    _puzzleMaker._statusObject.GetComponent<Text>().color = Color.green;
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "Puzzle grid is done!\nPlease start sketching from RED to BLUE point.";
                }                

                if (_puzzleMaker.isSketchDoneSucessfully)
                    _isExperimentFinished = true;
            }
            else
            {
                _puzzleMaker._statusObject.GetComponent<Text>().color = Color.blue;
                _puzzleMaker._statusObject.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
                _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 10;
                _puzzleMaker._statusObject.GetComponent<Text>().text = "THE TASK IS FINISHED!\nPlease take off the HMD.";
            }
        }

        /*if(hasThingToDraw)
        {
            drawOnTexture(screenTexture, drawnPoint.X, drawnPoint.Y);
            hasThingToDraw = false;
            gameObject.GetComponent<Renderer>().material.mainTexture = screenTexture;
        }*/
    }
    void handleEditModeChanged(EditMode mode)
    {
        Debug.Log("Selected mode: " + mode.ToString());
        gameMode = mode;
    }

    int touchDownReceived = 0;
    void handleControlGesture(TouchGesture gesture)
    {
        if(_puzzleMaker == null && _puzzleMakerObject != null)
        {
            _puzzleMaker = _puzzleMakerObject.GetComponent<PuzzleMaker>();
            Debug.Log(">> Init Puzzle Maker");
        }
        else if(_puzzleMakerObject == null)
        {
            Debug.LogWarning("Missing Puzzle Maker Object!", _puzzleMakerObject);
        }
        if (gesture.GestureType == GestureType.NONE)
        {
            hasTouchDown = false;

            if (_selectedPiece != null)
            {
                _selectedPiece.IsSelected = false;
                _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
            }
            
        }
        else
        {
            if (_isExperimentStarted)
            {
                //RaycastHit hitInfo1 = new RaycastHit();

                //bool hit1 = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo1);

                ////if (InStartPoints(ConvertPositionToPixelPosition(new Vector2(hitInfo1.point.x, hitInfo1.point.y))))
                ////    Debug.Log("IN START POINTS");

                ////if (InLinePoints(ConvertPositionToPixelPosition(new Vector2(hitInfo1.point.x, hitInfo1.point.y))))
                ////    Debug.Log("IN LINE POINTS");
                ///

                if (!_puzzleMaker.isPuzzledDone)
                {
                    if (!hasTouchDown && gesture.GestureType == GestureType.SINGLE_TOUCH_DOWN)
                    {
                        touchDownReceived++;
                        Debug.Log("TouchDownReceived: " + touchDownReceived);
                        hasTouchDown = true;

                        Vector2 local2DPos = ConvertLocalToGlobal((Vector2)gesture.MetaData);

                        Vector3 rayPoint = new Vector3(local2DPos.x,
                                                        local2DPos.y,
                                                        _puzzleMaker._largeScreenObject.transform.position.z - 1.5f);

                        Debug.Log(rayPoint);

                        RaycastHit[] hits = Physics.RaycastAll(rayPoint, Vector3.forward);

                        for (int i = 0; i < hits.Length - 1; i++)
                        {
                            for (int j = i + 1; j < hits.Length; j++)
                            {
                                if (hits[i].transform.position.z > hits[j].transform.position.z)
                                {
                                    var tmp = hits[i];
                                    hits[i] = hits[j];
                                    hits[j] = tmp;
                                }
                            }
                        }

                        for (int i = 0; i < hits.Length; i++)
                        {
                            if (hits[i].transform.gameObject.name.Contains("Puzzle Piece"))
                            {
                                for (int j = 0; j < _puzzleMaker._puzzlePieces.Count; j++)
                                {
                                    if (_puzzleMaker._puzzlePieces[j].GameObject.name == hits[i].transform.gameObject.name)
                                    {
                                        //Reset previous puzzle
                                        if (_selectedPiece != null)
                                        {
                                            _selectedPiece.IsSelected = false;
                                            _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                                        }

                                        float z = 0;
                                        z = _puzzleMaker._puzzlePieces[0].GameObject.transform.position.z;

                                        for (int k = 0; k < j; k++)
                                        {
                                            _puzzleMaker._puzzlePieces[k].GameObject.transform.position = new Vector3(_puzzleMaker._puzzlePieces[k].GameObject.transform.position.x,
                                                                                                                        _puzzleMaker._puzzlePieces[k].GameObject.transform.position.y,
                                                                                                                        _puzzleMaker._puzzlePieces[k + 1].GameObject.transform.position.z);
                                        }

                                        _puzzleMaker._puzzlePieces[j].GameObject.transform.position = new Vector3(_puzzleMaker._puzzlePieces[j].GameObject.transform.position.x,
                                                                                                                    _puzzleMaker._puzzlePieces[j].GameObject.transform.position.y,
                                                                                                                    z);

                                        _puzzleMaker.SortPieces();
                                        // Highlight new puzzle
                                        Piece piece = _puzzleMaker._puzzlePieces[0];

                                        piece.IsSelected = true;
                                        piece.GameObject.GetComponent<Renderer>().material.mainTexture = piece.Highlighted;

                                        piece.GameObject.transform.position = new Vector3(piece.GameObject.transform.position.x,
                                                                                            piece.GameObject.transform.position.y,
                                                                                            _puzzleMaker._puzzlePieces[0].GameObject.transform.position.z);

                                        _puzzleMaker._puzzlePieces[0] = piece;
                                        _selectedPiece = piece;

                                        difPosition = rayPoint - _selectedPiece.GameObject.transform.position;

                                        //
                                        //Debug.Log(_puzzleMaker._puzzlePieces[j].GameObject.transform.position);
                                        Debug.Log(">>>> SELECTED PIECE: " + hits[i].transform.gameObject.name);
                                        Debug.Log(">>>> SELECTED PIECE: " + piece.GameObject.name);

                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else if (hasTouchDown)
                    {
                        Debug.Log(gesture.GestureType);
                        if (_selectedPiece != null && gesture.GestureType == GestureType.SINGLE_TOUCH_MOVE)
                        {
                            Vector2 local2DPos = ConvertLocalToGlobal((Vector2)gesture.MetaData);

                            Vector3 rayPoint = new Vector3(local2DPos.x,
                                                           local2DPos.y,
                                                            _puzzleMaker._largeScreenObject.transform.position.z - 1.5f);


                            Vector3 difV = rayPoint - difPosition;

                            _selectedPiece.GameObject.transform.Translate(difV - _selectedPiece.GameObject.transform.position, Space.World);
                        }
                        else if (_selectedPiece != null && gesture.GestureType == GestureType.OBJECT_SCALING)
                        {
                            Vector2 local2DScale = (Vector2)gesture.MetaData;

                            _selectedPiece.GameObject.transform.localScale = new Vector3(_selectedPiece.GameObject.transform.localScale.x * local2DScale.x,
                                                                                          _selectedPiece.GameObject.transform.localScale.y * local2DScale.y,
                                                                                          _selectedPiece.GameObject.transform.localScale.z);
                        }
                        else if (_selectedPiece != null && gesture.GestureType == GestureType.OBJECT_ROTATING)
                        {
                            Vector2 local2DRotation = (Vector2)gesture.MetaData * -1;

                            _selectedPiece.GameObject.transform.RotateAround(_selectedPiece.GameObject.transform.position, _selectedPiece.GameObject.transform.forward, local2DRotation.x);
                        }

                        if (gesture.GestureType == GestureType.NONE)
                        {
                            if (_selectedPiece != null)
                            {
                                _selectedPiece.IsSelected = false;
                                _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                            }
                        }
                    }
                }
                else
                {
                    if (_selectedPiece != null)
                    {
                        _selectedPiece.IsSelected = false;
                        _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                    }

                    if (_puzzleMaker.isSketchStarted && !_puzzleMaker.isSketchDoneSucessfully)
                    {
                        if ((!hasTouchDown && gesture.GestureType == GestureType.SINGLE_TOUCH_DOWN) ||
                            (hasTouchDown && gesture.GestureType == GestureType.SINGLE_TOUCH_MOVE))
                        {

                            hasTouchDown = true;

                            Vector2 local2DPos = ConvertLocalToGlobal((Vector2)gesture.MetaData);

                            Vector3 rayPoint = new Vector3(local2DPos.x,
                                                            local2DPos.y,
                                                            _puzzleMaker._largeScreenObject.transform.position.z - 1.5f);

                            _puzzleMaker.CheckSketch(rayPoint);
                        }
                    }
                }
            }
            else
            {
                if (gesture.GestureType == GestureType.SINGLE_TOUCH_DOWN)
                {
                    hasTouchDown = true;

                    Vector2 local2DPos = ConvertLocalToGlobal((Vector2)gesture.MetaData);

                    Vector3 rayPoint = new Vector3(local2DPos.x,
                                                   local2DPos.y,
                                                   _puzzleMaker._largeScreenObject.transform.position.z - 1.5f);

                    RaycastHit[] hits = Physics.RaycastAll(rayPoint, Vector3.forward);

                    foreach(var hitInfo in hits)
                    {
                        if (_puzzleMaker._startButtonObject.gameObject.name == hitInfo.transform.gameObject.name)
                        {
                            _puzzleMaker.Init(_textureID, _prepareTime);

                            _puzzleMaker._startButtonObject.SetActive(false);
                            _startTimer = true;

                            Debug.Log(">> Timer started! Counting down...");
                        }
                    }                    
                }
            }

            
        }


        /*if(!handledByCharacter)
        {
            if(gesture.GestureType == GestureType.SINGLE_TOUCH_DOWN
                || gesture.GestureType == GestureType.SINGLE_TOUCH_MOVE)
            {
                hasThingToDraw = true;
                Vector2 localTouchPos = (Vector2)gesture.MetaData;
                drawnPoint.X = (int)((localTouchPos.x) * screenTexture.width);
                drawnPoint.Y = (int)((localTouchPos.y) * screenTexture.height);
            }
        }*/
    }
    void drawOnTexture(Texture2D tex, int posX, int posY)
    {
        //tex.SetPixels(posX, posY, 5, 5, paintColors);
        tex.SetPixel(posX, posY, Color.red);
    }

    Vector2 ConvertLocalToGlobal(Vector2 local2DPos)
    {
        float scale = 1;
        if (_puzzleMaker._isLargeScreenPlane)
            scale = 10;

        return new Vector2(local2DPos.x * _puzzleMaker._largeScreenObject.transform.localScale.x * scale + _puzzleMaker._largeScreenObject.transform.position.x,
                           local2DPos.y * _puzzleMaker._largeScreenObject.transform.localScale.y * scale + _puzzleMaker._largeScreenObject.transform.position.y);

    }
}
