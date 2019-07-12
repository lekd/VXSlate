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

            Debug.Log("Experiment Started!");

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
                if (hasTouchDown && _selectedPiece != null)
                {
                    _puzzleMaker.UpdatePiecePosition(_selectedPiece);
                }

                _puzzleMaker.HighlightGridPiece();

                if (!_puzzleMaker.isPuzzledDone && _puzzleMaker.CheckPuzzlesDone())
                {
                    _puzzleMaker.isPuzzledDone = true;
                    _puzzleMaker._puzzleDoneObject.SetActive(true);
                }

                if (_puzzleMaker.isPuzzledDone && !_puzzleMaker.isSketchStarted)
                {
                    _puzzleMaker.isSketchStarted = true;

                    if (_puzzleMaker._sketchedPixels == null)
                        _puzzleMaker._sketchedPixels = new List<Pixel>();

                    _puzzleMaker._statusObject.GetComponent<Text>().color = Color.green;
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "Puzzle grid is done!\nPlease start sketching from RED to BLUE point.";
                }

                if (_puzzleMaker.isSketchStarted && !_puzzleMaker.isSketchDoneSucessfully && hasTouchDown)
                    _puzzleMaker.CheckSketch();

                if (_puzzleMaker.isSketchDoneSucessfully)
                    _isExperimentFinished = true;

                if (Input.GetKeyDown(KeyCode.DownArrow) && _selectedPiece != null)
                {
                    _selectedPiece = _puzzleMaker.PuzzlePieceScaleDown(_selectedPiece);

                    Debug.Log("Puzzle piece is scaled down!");
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "Puzzle piece is scaled down!";
                }

                if (Input.GetKeyUp(KeyCode.UpArrow) && _selectedPiece != null)
                {
                    _selectedPiece = _puzzleMaker.PuzzlePieceScaleUp(_selectedPiece);

                    Debug.Log("Puzzle piece is scaled up!");
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "Puzzle piece is scaled up!";
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) && _selectedPiece != null)
                {
                    _selectedPiece = _puzzleMaker.PuzzlePieceRotateLeft(_selectedPiece);

                    Debug.Log("Puzzle piece is rotated left!");
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "Puzzle piece is rotated left!";
                }

                if (Input.GetKeyUp(KeyCode.RightArrow) && _selectedPiece != null)
                {
                    _selectedPiece = _puzzleMaker.PuzzlePieceRotateRight(_selectedPiece);

                    Debug.Log("Puzzle piece is rotated right!");
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "Puzzle piece is rotated right!";
                }
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

    void handleControlGesture(TouchGesture gesture)
    {
        if(_puzzleMaker == null && _puzzleMakerObject != null)
        {
            _puzzleMaker = _puzzleMakerObject.GetComponent<PuzzleMaker>();
            Debug.Log("Init Puzzle Maker");
        }
        else if(_puzzleMakerObject == null)
        {
            Debug.LogWarning("Missing Puzzle Maker Object!", _puzzleMakerObject);
        }

        if (gameMode == EditMode.OBJECT_MANIP)
        {
            bool handledByCharacter = false;
            if (gameCharacter != null)
            {
                handledByCharacter = gameCharacter.handleGesture(gesture);
            }
            if(gesture.GestureType == GestureType.SINGLE_TOUCH_MOVE)
            {
                Vector2 localTouchPos = (Vector2)gesture.MetaData;
                Debug.Log(string.Format("Finger move at: ({0},{1})", localTouchPos.x, localTouchPos.y));
            }
        }
        else if(gameMode == EditMode.DRAW)
        {
            if(gesture.GestureType == GestureType.SINGLE_TOUCH_DOWN || gesture.GestureType == GestureType.SINGLE_TOUCH_MOVE)
            {
                Vector2 local2DPos = (Vector2)gesture.MetaData;
                Vector2 abs2DPos = new Vector2(local2DPos.x*screenSize.x,local2DPos.y*screenSize.y);
               
                if (gesture.GestureType == GestureType.SINGLE_TOUCH_DOWN)
                {
                    hasTouchDown = true;
                    Debug.Log(string.Format("Drawing at ({0},{1})", abs2DPos.x, abs2DPos.y));
                }
                if(hasTouchDown && gesture.GestureType == GestureType.SINGLE_TOUCH_MOVE)
                {
                    Debug.Log(string.Format("Drawing at ({0},{1})", abs2DPos.x, abs2DPos.y));
                }
            }
            else if(gesture.GestureType == GestureType.NONE)
            {
                hasTouchDown = false;
            }
        }

        if(gesture.GestureType == GestureType.NONE)
        {
            hasTouchDown = false;
        }
        else
        {
            if (_isExperimentStarted)
            {
                //if (gesture.GestureType == GestureType.SINGLE_TOUCH_DOWN)
                //{

                //}
                //    //For testing
                //    //_puzzleMaker.isPuzzledDone = true;

                //    RaycastHit hitInfo1 = new RaycastHit();

                //    bool hit1 = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo1);

                //    //if (InStartPoints(ConvertPositionToPixelPosition(new Vector2(hitInfo1.point.x, hitInfo1.point.y))))
                //    //    Debug.Log("IN START POINTS");

                //    //if (InLinePoints(ConvertPositionToPixelPosition(new Vector2(hitInfo1.point.x, hitInfo1.point.y))))
                //    //    Debug.Log("IN LINE POINTS");

                //    if (!isMouseDown)
                //    {
                //        Debug.Log("OnMouseDown");

                //        if (!_puzzleMaker.isPuzzledDone)
                //        {
                //            RaycastHit hitInfo = new RaycastHit();

                //            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

                //            if (hit)
                //            {
                //                if (_selectedPiece != null && _selectedPiece.GameObject.name == hitInfo.transform.gameObject.name)
                //                {
                //                    _selectedPiece.IsSelected = false;
                //                    _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;

                //                    _selectedPiece = null;
                //                }
                //                else
                //                {
                //                    for (int i = 0; i < _puzzleMaker._puzzlePieces.Count; i++)
                //                    {
                //                        if (_puzzleMaker._puzzlePieces[i].GameObject.name == hitInfo.transform.gameObject.name)
                //                        {
                //                            float z = 0;

                //                            Piece piece = _puzzleMaker._puzzlePieces[i];

                //                            //Reset previous puzzle
                //                            if (_selectedPiece != null)
                //                            {
                //                                _selectedPiece.IsSelected = false;
                //                                _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                //                            }

                //                            z = _puzzleMaker._puzzlePieces[0].GameObject.transform.position.z;

                //                            for (int j = 0; j < i; j++)
                //                            {
                //                                _puzzleMaker._puzzlePieces[j].GameObject.transform.position = new Vector3(_puzzleMaker._puzzlePieces[j].GameObject.transform.position.x,
                //                                                                                                            _puzzleMaker._puzzlePieces[j].GameObject.transform.position.y,
                //                                                                                                            _puzzleMaker._puzzlePieces[j + 1].GameObject.transform.position.z);
                //                            }

                //                            // Highlight new puzzle
                //                            piece.IsSelected = true;
                //                            piece.GameObject.GetComponent<Renderer>().material.mainTexture = piece.Highlighted;

                //                            if (z != 0)
                //                                piece.GameObject.transform.position = new Vector3(piece.GameObject.transform.position.x,
                //                                                                                    piece.GameObject.transform.position.y,
                //                                                                                    z);

                //                            _selectedPiece = piece;

                //                            _puzzleMaker._puzzlePieces = _puzzleMaker.SortPieces(_puzzleMaker._puzzlePieces);

                //                            break;
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //        else
                //        {
                //            if (_selectedPiece != null)
                //            {
                //                _selectedPiece.IsSelected = false;
                //                _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                //            }


                //        }

                //        isMouseDown = true;
                //    }
                //}
            }
            else
            {
                if (gesture.GestureType == GestureType.SINGLE_TOUCH_DOWN)
                {
                    hasTouchDown = true;

                    Vector2 local2DPos = (Vector2)gesture.MetaData;
                    Vector2 abs2DPos = new Vector2(local2DPos.x * screenSize.x, local2DPos.y * screenSize.y);

                    
                    Vector3 rayPoint = new Vector3(abs2DPos.x + _puzzleMaker._largeScreenObject.transform.position.x,
                                                   abs2DPos.y + _puzzleMaker._largeScreenObject.transform.position.y,
                                                   _puzzleMaker._largeScreenObject.transform.position.z - 1.5f);
                    Debug.Log(rayPoint);

                    RaycastHit[] hits = Physics.RaycastAll(rayPoint, Vector3.forward);

                    foreach(var hitInfo in hits)
                    {
                        if (_puzzleMaker._startButtonObject.gameObject.name == hitInfo.transform.gameObject.name)
                        {
                            _puzzleMaker.Init(_textureID, _prepareTime);

                            _puzzleMaker._startButtonObject.SetActive(false);
                            _startTimer = true;

                            Debug.Log("Timer started! Counting down...");
                        }
                    }                    
                }

                //if (hasTouchDown)
                //{


                //}

                //if (_eventListenner._isMouse)
                //{
                //    RaycastHit hitInfo = new RaycastHit();

                //    bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

                //    if (hit && _puzzleMaker._startButtonObject.gameObject.name == hitInfo.transform.gameObject.name)
                //    {
                //        _puzzleMaker.Init(_textureID, _prepareTime);

                //        _puzzleMaker._startButtonObject.SetActive(false);
                //        _startTimer = true;

                //        Debug.Log("Timer started! Counting down...");
                //    }
                //}

                //if (_eventListenner._isTablet)
                //{
                //    Vector2 startButtonV2 = new Vector2(_puzzleMaker._startButtonObject.gameObject.transform.position.x,
                //                                        _puzzleMaker._startButtonObject.gameObject.transform.position.y);

                //    if ((startButtonV2 - _eventListenner.touchPosition).magnitude
                //        < _puzzleMaker._startButtonObject.gameObject.transform.localScale.x)
                //    {
                //        _puzzleMaker.Init(_textureID, _prepareTime);

                //        _puzzleMaker._startButtonObject.SetActive(false);
                //        _startTimer = true;

                //        Debug.Log("Timer started! Counting down...");
                //    }
                //}
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
}
