using BoardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleMaker : MonoBehaviour
{
    bool isInit = false;

    private const float _coefficientCoverterPixelToM = 3779.5275590551f;

    public GameObject _largeScreenObject;
    // Large screen size in pixels
    float _largeScreenWidth;
    float _largeScreenHeight;

    public Material _screenMaterial;
    public Texture2D _mainPuzzleTexture;
    public Texture2D _gridPieceTexture;

    float _percentageForMargins = 0.05f;
    float _marginSize = 0;
    
    int _x = 4; // number of collumn
    int _y = 4; // number of rows

    Vector2 _largeScreenCenter; //in pixel
    Vector2 _sampleScreenCenter; //in pixel

    // Puzzle Grid
    public float _gridWidth = 3f;
    public float _gridHeight = 2f;
     
    public Material _sampleMaterial;

    List<Texture2D> _listOfTextiles;
    List<Piece> _gridPieces;
    List<Piece> _puzzlePieces;

    Texture2D _mainTexture;

    float _overlapThreshold = 0.02f; //in meters
    int _differentZThreshold = 10;
    
    Piece _selectedPiece = null;

    float _differentScale = 0.1f;
    int _numberOfScale = 10;
    Vector3 _standardPieceScale = Vector3.zero;


    float _differentAngle = 4;
    int _numberOfRotation = 80;
    Vector3 _standardRotation = Vector3.zero;

    bool isMouseDown = false;

    bool isPuzzledDone = false;

    // Check sketch progress
    bool isSketchStarted = false;
    bool isInStartPoints = false;
    bool isInLinePoints = false;
    bool isInEndPoints = false;
    bool isSketchDoneSucessfully = false;

    Color _startPointsColor = new Color(255, 0, 0);
    Color _linePointsColor = new Color(255, 255, 0);
    Color _endPointsColor = new Color(0, 0, 255);

    List<Vector2> _startStopPoints;
    List<Vector2> _linePoints;

    Vector2 _firstGridPixelPosition = Vector2.zero;
    float _mainPuzzleTextureRatio = 1;
    List<Pixel> _sketchedPixels;
    int _sketchedBrush = 5;
    GameObject _puzzleDoneObject;
    
    // Start is called before the first frame update
    void Start()
    {
        /// Large Screen Object
        /// 

        if (_largeScreenObject != null)
        {
            _largeScreenWidth = ConvertMetersToPixels(_largeScreenObject.transform.localScale.x);
            _largeScreenHeight = ConvertMetersToPixels(_largeScreenObject.transform.localScale.y);
        }
        else
        {
            _largeScreenObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _largeScreenObject.name = "Large Screen";

            _largeScreenWidth = ConvertMetersToPixels(7f);
            _largeScreenHeight = ConvertMetersToPixels(3f);

            //_largeScreenWidth = ConvertMetersToPixels(15f);
            //_largeScreenHeight = ConvertMetersToPixels(3f);

            _largeScreenObject.transform.position = new Vector3(0, 2.5f, 0);
            _largeScreenObject.transform.rotation = Quaternion.identity;
            _largeScreenObject.transform.localScale = new Vector3(ConvertPixelsToMeters(_largeScreenWidth), ConvertPixelsToMeters(_largeScreenHeight), 0.01f);

            //_largeScreenObject.AddComponent<MeshFilter>();
            //_largeScreenObject.AddComponent<MeshRenderer>();

            if (_screenMaterial != null)
            {
                _largeScreenObject.GetComponent<Renderer>().material = _screenMaterial;
            }
            else
            {
                Debug.LogWarning("Missing screen material!", _screenMaterial);
            }

            _largeScreenCenter = new Vector2(_largeScreenWidth / 2, _largeScreenHeight / 2);

            Debug.LogWarning("Large screen object is missed! A random object is generated.", _largeScreenObject);
        }

        
        if(_mainPuzzleTexture != null)
        {
            _marginSize = _largeScreenWidth * _percentageForMargins;

            Vector2 _mainTextureArea = new Vector2(0.4f * (_largeScreenWidth - 4 * _marginSize), _largeScreenHeight - 2 * _marginSize);

            _mainTextureArea = RescaleArea(_mainTextureArea, new Vector2(_mainPuzzleTexture.width, _mainPuzzleTexture.height));

            _mainPuzzleTextureRatio = _mainTextureArea.x / _mainPuzzleTexture.width;
                       
            /// Sample Screen Area
            /// 

            Vector2 _sampleScreenArea = new Vector2(0.2f * (_largeScreenWidth - 4 * _marginSize), _largeScreenHeight - 2 * _marginSize);

            _sampleScreenArea = RescaleArea(_sampleScreenArea, new Vector2(_mainPuzzleTexture.width, _mainPuzzleTexture.height));


            _sampleScreenCenter = new Vector2(_sampleScreenArea.x / 2, _sampleScreenArea.y / 2);

            Quaternion sampleScreenRotation = new Quaternion();
            sampleScreenRotation.eulerAngles = new Vector3(0, 0, 180);

            GameObject sampleScreen = CreateCubeGameObject("Sample Screen",
                                                            new Vector3(_largeScreenObject.transform.position.x + ConvertPixelsToMeters(_largeScreenWidth * 0.5f - 
                                                                                                                                        _marginSize -
                                                                                                                                        (int)_sampleScreenArea.x/2),
                                                                        _largeScreenObject.transform.position.y,
                                                                        -0.0001f),
                                                            sampleScreenRotation,
                                                            new Vector3(ConvertPixelsToMeters(_sampleScreenArea.x),
                                                                        ConvertPixelsToMeters(_sampleScreenArea.y),
                                                                        0.01f),
                                                            null,
                                                            _mainPuzzleTexture,
                                                            Color.white);

            /// Puzzle Area
            /// 

            Vector2 puzzleArea = new Vector2(0.4f * (_largeScreenWidth - 4 * _marginSize), _largeScreenHeight - 2 * _marginSize);

            //puzzleArea = RescaleArea(puzzleArea, new Vector2(_mainPuzzleTexture.width, _mainPuzzleTexture.height));

            Quaternion puzzleAreaRotation = new Quaternion();
            sampleScreenRotation.eulerAngles = new Vector3(0, 0, 180);

            float middleMargin = (_largeScreenWidth - puzzleArea.x - _sampleScreenArea.x - _mainTextureArea.x - 4 * _marginSize) /2;
            float rightPart = middleMargin + 2 * _marginSize + _sampleScreenArea.x;
            float shift = rightPart - _largeScreenWidth / 2;

            float puzzlePositionX = puzzleArea.x / 2 + shift;

            GameObject puzzleAreaObject = CreateCubeGameObject("Puzzle Area",
                                                                new Vector3(_largeScreenObject.transform.position.x - ConvertPixelsToMeters(puzzlePositionX),
                                                                            _largeScreenObject.transform.position.y,
                                                                            -0.0001f),
                                                                puzzleAreaRotation,
                                                                new Vector3(ConvertPixelsToMeters(puzzleArea.x),
                                                                            ConvertPixelsToMeters(puzzleArea.y),
                                                                            0.01f),
                                                                null,
                                                                null,
                                                                Color.white);

            /// Puzzle Grid Area
            /// 
            if (_gridPieceTexture != null)
            {
                Quaternion puzzleDoneRotation = new Quaternion();
                puzzleDoneRotation.eulerAngles = new Vector3(0, 0, 180);

                Vector3 puzzleDonePosition = Vector3.zero;                           

                puzzleDonePosition.x = _largeScreenObject.transform.position.x - ConvertPixelsToMeters(_largeScreenCenter.x - _marginSize - _mainTextureArea.x / 2);
                puzzleDonePosition.y = _largeScreenObject.transform.position.y + ConvertPixelsToMeters(_largeScreenCenter.y - _marginSize - _mainTextureArea.y / 2);
                puzzleDonePosition.z = -0.005f;

                _puzzleDoneObject = CreateCubeGameObject("Puzzle Done",
                                                         puzzleDonePosition,
                                                         puzzleDoneRotation,
                                                         new Vector3(ConvertPixelsToMeters(_mainTextureArea.x),
                                                                     ConvertPixelsToMeters(_mainTextureArea.y),
                                                                     0.01f),
                                                         null,
                                                         _mainPuzzleTexture,
                                                         Color.white);

                Destroy(_puzzleDoneObject.GetComponent<BoxCollider>());
                _puzzleDoneObject.AddComponent<MeshCollider>();

                _puzzleDoneObject.SetActive(false);

                _gridPieces = new List<Piece>();
                _puzzlePieces = new List<Piece>();

                GameObject gridMasterObject = new GameObject();
                gridMasterObject.name = "Puzzle Grid";

                GameObject puzzleMasterObject = new GameObject();
                puzzleMasterObject.name = "Puzzle Pieces";

                float _gridPieceWidth = _mainTextureArea.x / _x;
                float _gridPieceHeight = _mainTextureArea.y / _y;
                Vector2 _firstGridPosition = new Vector2(_largeScreenObject.transform.position.x - ConvertPixelsToMeters(_largeScreenCenter.x - _marginSize - _gridPieceWidth / 2),
                                                         _largeScreenObject.transform.position.y + ConvertPixelsToMeters(_largeScreenCenter.y - _marginSize - _gridPieceHeight / 2));

                _firstGridPixelPosition = new Vector2(_largeScreenObject.transform.position.x - ConvertPixelsToMeters(_largeScreenCenter.x - _marginSize),
                                                      _largeScreenObject.transform.position.y - ConvertPixelsToMeters(_largeScreenCenter.y - _marginSize));

                int puzzlePieceWidthScaledUp = (int)(_mainTextureArea.x / _x);
                int puzzlePieceHeightScaledUp = (int)(_mainTextureArea.y / _y);

                _overlapThreshold = ConvertPixelsToMeters(puzzlePieceHeightScaledUp/5);

                int puzzlePieceWidth = (int)(_mainPuzzleTexture.width / _x);
                int puzzlePieceHeight = (int)(_mainPuzzleTexture.height / _y);

                _startStopPoints = new List<Vector2>();
                _linePoints = new List<Vector2>();

                for (int i = 0; i < _x; i++)
                {
                    for (int j = 0; j < _y; j++)
                    {
                        Quaternion gridPieceRotation = new Quaternion();
                        gridPieceRotation.eulerAngles = new Vector3(0, 0, 180);

                        Vector3 gridPiecePosition = new Vector3(_firstGridPosition.x + ConvertPixelsToMeters(i * _gridPieceWidth),
                                                                _firstGridPosition.y - ConvertPixelsToMeters(j * _gridPieceHeight),
                                                                -0.0001f);

                        GameObject gridPieceObject = CreateCubeGameObject("Grid Piece " + (i * _x + _y - j).ToString(),
                                                                        gridPiecePosition,
                                                                        gridPieceRotation,
                                                                        new Vector3(ConvertPixelsToMeters(_gridPieceWidth),
                                                                                    ConvertPixelsToMeters(_gridPieceHeight),
                                                                                    0.01f),
                                                                        null,
                                                                        _gridPieceTexture,
                                                                        Color.white);

                        //Vector2 check = ConvertPositionToPixelPosition(new Vector2(gridPiecePosition.x, gridPiecePosition.y));

                        //if (check.x < 0 || check.y < 0 || check.x > _mainPuzzleTexture.width || check.y > _mainPuzzleTexture.height)
                        //{
                        //    Debug.Log("Exceed texture!");
                        //}

                        gridPieceObject.transform.parent = gridMasterObject.transform;

                        Piece gridPiece = new Piece(gridPieceObject, i.ToString() + (_y - j - 1).ToString());
                        _gridPieces.Add(gridPiece);



                        /// Generate Puzzle Pieces
                        /// 

                        Texture2D puzzleTextureOrginal = new Texture2D(puzzlePieceWidth, puzzlePieceHeight);
                        Texture2D puzzleTextureHighlighted = new Texture2D(puzzlePieceWidth, puzzlePieceHeight);

                        for (int x = 0; x < puzzlePieceWidth; x++)
                        {
                            for (int y = 0; y < puzzlePieceHeight; y++)
                            {
                                Color color = _mainPuzzleTexture.GetPixel(x + i * puzzlePieceWidth, y + j * puzzlePieceHeight);
                                puzzleTextureOrginal.SetPixel(x, y, color);

                                if ((x < 15) || (x > puzzlePieceWidth - 15) || (y < 15) || (y > puzzlePieceHeight - 15))
                                {
                                    puzzleTextureHighlighted.SetPixel(x, y, Color.black);
                                }
                                else
                                    puzzleTextureHighlighted.SetPixel(x, y, color);


                                // Get Start, Stop, and Line Points
                                if (color.r == 1f && color.g == 0f && color.b == 0f)
                                {
                                    _startStopPoints.Add(new Vector2(x + i * puzzlePieceWidth, y + j * puzzlePieceHeight));
                                }
                                else if (color.r == 1f && color.g == 1f && color.b == 0f)
                                {
                                    _linePoints.Add(new Vector2(x + i * puzzlePieceWidth, y + j * puzzlePieceHeight));
                                }
                            }
                        }

                        puzzleTextureOrginal.Apply();
                        puzzleTextureHighlighted.Apply();

                        Quaternion puzzlePieceRotation = new Quaternion();

                        
                        _standardRotation = new Vector3(0, 0, 180);

                        float zAngle = ((int)(UnityEngine.Random.Range(0, _numberOfRotation))) * _differentAngle;

                        puzzlePieceRotation.eulerAngles = new Vector3(0, 0, zAngle);

                        //puzzlePieceRotation.eulerAngles = new Vector3(0, 0, StandardizeRotationAngle(UnityEngine.Random.Range(0, 360), _differentZThreshold));

                        Vector3 puzzlePiecePosition = Vector3.zero;

                        float gridPieceWidthInMeter = ConvertPixelsToMeters(_gridPieceWidth);
                        float gridPieceHeightInMeter = ConvertPixelsToMeters(_gridPieceHeight);

                        _standardPieceScale = new Vector3(gridPieceWidthInMeter, gridPieceHeightInMeter, 0.01f);

                        float ratio = ((int)(((int)(UnityEngine.Random.Range(1, _numberOfScale))) / 2)) * _differentScale;

                        if(UnityEngine.Random.Range(-1, 1) < 0)
                        {
                            ratio *= (-1);
                        }

                        Vector3 puzzlePieceScale = Vector3.zero;
                        puzzlePieceScale.x = _standardPieceScale.x * (1 + ratio);
                        puzzlePieceScale.y = _standardPieceScale.y * (1 + ratio);
                        puzzlePieceScale.z = _standardPieceScale.z;

                        do
                        {
                            bool flag = false;

                            float x = UnityEngine.Random.Range(puzzleAreaObject.transform.position.x - puzzleAreaObject.transform.localScale.x / 2 + puzzlePieceScale.x / 2,
                                                    puzzleAreaObject.transform.position.x + puzzleAreaObject.transform.localScale.x / 2 - puzzlePieceScale.x / 2);
                            float y = UnityEngine.Random.Range(puzzleAreaObject.transform.position.y - puzzleAreaObject.transform.localScale.y / 2 + puzzlePieceScale.y / 2,
                                                    puzzleAreaObject.transform.position.y + puzzleAreaObject.transform.localScale.y / 2 - puzzlePieceScale.y / 2);

                            float z = UnityEngine.Random.Range(-0.005f, -0.0015f);

                            for (int id = 0; id < _puzzlePieces.Count; id++)
                            {
                                if(_puzzlePieces[id].GameObject.transform.position.z == z)
                                {
                                    flag = true;
                                }
                            }

                            if (!flag)
                            {
                                puzzlePiecePosition = new Vector3(x, y, z);

                                break;
                            }
                        }
                        while (true);

                        GameObject puzzlePieceObject = CreateCubeGameObject("Puzzle Piece " + (i * _x + j + 1).ToString(),
                                                                            puzzlePiecePosition,
                                                                            puzzlePieceRotation,
                                                                            puzzlePieceScale,
                                                                            null,
                                                                            puzzleTextureOrginal,
                                                                            Color.white);

                        Destroy(puzzlePieceObject.GetComponent<BoxCollider>());
                        puzzlePieceObject.AddComponent<MeshCollider>();                        

                        puzzlePieceObject.transform.parent = puzzleMasterObject.transform;

                        Piece puzzlePiece = new Piece(puzzlePieceObject, i.ToString() + j.ToString(), false, puzzleTextureOrginal, puzzleTextureHighlighted, (1 + ratio));
                        _puzzlePieces.Add(puzzlePiece);
                    }
                }

                _puzzlePieces = SortPieces(_puzzlePieces);

                isInit = true;
                Debug.Log("Init finished!");

                //For testing
                _puzzleDoneObject.SetActive(true);
                foreach (var grid in _gridPieces)
                {
                    foreach(var puzzle in _puzzlePieces)
                    {
                        if(grid.Name == puzzle.Name)
                        {
                            puzzle.GameObject.transform.position = new Vector3(grid.GameObject.transform.position.x,
                                                                               grid.GameObject.transform.position.y,
                                                                               puzzle.GameObject.transform.position.z);
                            puzzle.GameObject.transform.rotation = grid.GameObject.transform.rotation;
                            puzzle.GameObject.transform.localScale = grid.GameObject.transform.localScale;

                            puzzle.GameObject.SetActive(false);
                        }
                    }
                }
                
            }
            else
            {
                Debug.LogWarning("Missing grid piece texture!", _gridPieceTexture);
            }            

        }
        else
        {
            Debug.LogWarning("Missing puzzle texture!", _mainPuzzleTexture);
        }        
    }

    // Update is called once per frame
    void Update()
    {
        OnMouseDown();
        OnMouseUp();
        UpdatePiecePosition();
        HighlightGridPiece();
        isPuzzledDone = CheckPuzzlesDone();
        if(isPuzzledDone && !isSketchStarted)
        {
            isSketchStarted = true;

            if(_sketchedPixels == null)
            _sketchedPixels = new List<Pixel>();
        }        

        CheckSketch();

        if (Input.GetKeyDown(KeyCode.DownArrow) && _selectedPiece != null)
        {
            _selectedPiece = PuzzlePieceScaleDown(_selectedPiece);

            Debug.Log("Puzzle piece is scaled down!");
        }

        if (Input.GetKeyUp(KeyCode.UpArrow) && _selectedPiece != null)
        {
            _selectedPiece = PuzzlePieceScaleUp(_selectedPiece);

            Debug.Log("Puzzle piece is scaled up!");
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && _selectedPiece != null)
        {
            _selectedPiece = PuzzlePieceRotateLeft(_selectedPiece);

            Debug.Log("Puzzle piece is rotated left!");
        }

        if (Input.GetKeyUp(KeyCode.RightArrow) && _selectedPiece != null)
        {
            _selectedPiece = PuzzlePieceRotateRight(_selectedPiece);

            Debug.Log("Puzzle piece is rotated right!");
        }

        //if (_selectedPiece != null)
        //{
        //    Vector3 scale = StandardizeScale(_selectedPiece.GameObject.transform.localScale, _stardardPieceScale, _differentScale);
        //    _selectedPiece.GameObject.transform.localScale = scale;
        //}
    }

    private void CheckSketch()
    {    
        if(isPuzzledDone && isMouseDown)
        {
            //if (CheckPointPixelColor(_startPointsColor) && !isInStartPoints)
            //{
            //    isInStartPoints = true;
            //    isInEndPoints = false;
            //    isInLinePoints = false;
            //    Debug.Log("IN START POINTS");
            //} else if (CheckPointPixelColor(_linePointsColor) && !isInLinePoints)
            //{
            //    isInStartPoints = false;
            //    isInEndPoints = false;
            //    isInLinePoints = true;
            //    Debug.Log("IN LINE POINTS");
            //}
            //else if((!isInStartPoints && !isInLinePoints) || ((!CheckPointPixelColor(_startPointsColor) && !isInStartPoints) && (!CheckPointPixelColor(_linePointsColor) && isInLinePoints)))
            //{
            //    Debug.Log("======================");
            //}

            if (!isSketchStarted)
            {
                isSketchStarted = true;
            }

            if (isSketchStarted)
            {
                if (!isInStartPoints && CheckPointPixelColor(_startPointsColor))
                {
                    isInStartPoints = true;
                    isInLinePoints = false;
                    isInEndPoints = false;

                    Debug.Log("IN START POINTS");
                }
                else if (isInStartPoints && !CheckPointPixelColor(_startPointsColor))
                {                   
                    if (!isInLinePoints && CheckPointPixelColor(_linePointsColor))
                    {
                        isInStartPoints = true;
                        isInLinePoints = true;
                        isInEndPoints = false;

                        Debug.Log("IN LINE POINTS");
                    }
                    else if (isInLinePoints && !CheckPointPixelColor(_linePointsColor))
                    {
                        if (!isInEndPoints && CheckPointPixelColor(_endPointsColor))
                        {
                            isInStartPoints = true;
                            isInEndPoints = true;
                            isInLinePoints = true;

                            Debug.Log("IN END POINTS");
                        }
                        else if(isInStartPoints && isInLinePoints && isInEndPoints)
                        {
                            isSketchDoneSucessfully = true;
                            Debug.Log("Sketch is successfully done!");
                        }
                        else
                        {
                            isInStartPoints = false;
                            isInEndPoints = false;
                            isInLinePoints = false;

                            ResetTexture();

                            //_puzzleDoneObject.gameObject.GetComponent<Renderer>().material.mainTexture = _mainPuzzleTexture;

                            Debug.Log("Sketch is suddenly failed!");
                        }
                    }
                }               
            }
        }        
    }

    bool CheckPointPixelColor(Color color)
    {
        RaycastHit hit;

        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return false;

        Renderer rend = hit.transform.GetComponent<Renderer>();
        MeshCollider mesh = hit.collider as MeshCollider;

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || mesh == null)
            return false;

        if (!hit.transform.name.Contains("Puzzle Done"))
            return false;

        Texture2D texture2D = rend.material.mainTexture as Texture2D;// new Texture2D(rend.material.mainTexture.width, rend.material.mainTexture.height);
        //Texture2D texture2D;
        //Graphics.CopyTexture(rend.material.mainTexture, texture2D);

        Vector2 pixelUV = hit.textureCoord;

        pixelUV.x *= texture2D.width;
        pixelUV.y *= texture2D.height;

        //Debug.Log("X:::" + pixelUV.x + " Y::" + pixelUV.y);

        Color32 c;
        c = texture2D.GetPixel((int)pixelUV.x, (int)pixelUV.y);
        //Debug.Log(c);

        float difR = c.r - color.r;
        float difG = c.g - color.g;
        float difB = c.b - color.b;

        if (difR < 20 && difR > -20 &&
            difG < 20 && difG > -20 &&
            difB < 20 && difB > -20)
        {               
            hit.transform.GetComponent<Renderer>().material.mainTexture = BrushSketchLines(texture2D, (int)pixelUV.x, (int)pixelUV.y); ;

            return true;
        }

        if(c.r == 0 && c.g == 0 && c.b == 0)
        {
            return true;
        }

        return false;
    }

    private Texture2D BrushSketchLines(Texture2D tex, int x, int y)
    {
        for(int i = 0; i < _sketchedBrush; i++)
        {
            for(int j = 0; j < _sketchedBrush; j++)
            {
                for(int sign = 0; sign < 4; sign++)
                {
                    int a, b;
                    a = x;
                    b = y;

                    if(sign == 0)
                    {
                        a += i;
                        b += j;
                    }
                    else if (sign == 1)
                    {
                        a -= i;
                        b += j;
                    }
                    else if (sign == 2)
                    {
                        a += i;
                        b -= j;
                    }
                    else if (sign == 3)
                    {
                        a -= i;
                        b -= j;
                    }

                    if (a >= tex.width)
                        a = tex.width - 1;

                    if (b >= tex.height)
                        b = tex.height - 1;

                    if (a < 0)
                        a = 0;

                    if (b < 0)
                        b = 0;

                    float distance = (new Vector2(a, b) - new Vector2(x, y)).magnitude;
                    if (distance <= _sketchedBrush)
                    {
                        Pixel p = new Pixel(a, b, tex.GetPixel(a, b), Color.black);
                        tex.SetPixel(a, b, Color.black);

                        bool flag = false;
                        foreach(var pixel in _sketchedPixels)
                        {
                            if(p.X == pixel.X && p.Y == pixel.Y)
                            {
                                flag = true;
                            }
                        }

                        if(!flag)
                            _sketchedPixels.Add(p);
                    }                    
                }
            }
        }
        
        tex.Apply();

        return tex;
    }

    private bool CheckPuzzlesDone()
    {
        foreach (var gridPiece in _gridPieces)
        {
            foreach(var puzzlePiece in _puzzlePieces)
            {
                if(puzzlePiece.Name == gridPiece.Name && (new Vector3(puzzlePiece.GameObject.transform.position.x - gridPiece.GameObject.transform.position.x,
                                                                      puzzlePiece.GameObject.transform.position.y - gridPiece.GameObject.transform.position.y,
                                                                      0)).magnitude > 0.02f)
                {
                    return false;
                }

                //if (puzzlePiece.Name == gridPiece.Name && (new Vector3(puzzlePiece.GameObject.transform.position.x - gridPiece.GameObject.transform.position.x,
                //                                                      puzzlePiece.GameObject.transform.position.y - gridPiece.GameObject.transform.position.y,
                //                                                      0)).magnitude < 0.02f)
                //{
                //    Debug.Log(puzzlePiece.Name);
                //}
            }
        }

        return true;
    }

    private void UpdatePiecePosition()
    {
        if(isMouseDown && _selectedPiece != null)
        {
            RaycastHit hitInfo = new RaycastHit();

            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

            bool flag = false;

            Piece gridPieceOverlapped = null;

            foreach (var gridPiece in _gridPieces)
            {
                if ((new Vector3(hitInfo.point.x, hitInfo.point.y, _selectedPiece.GameObject.transform.position.z) - gridPiece.GameObject.transform.position).magnitude < 0.02f)
                {
                    flag = true;
                    gridPieceOverlapped = gridPiece;

                    break;
                }
            }

            if (!flag)
            {
                _selectedPiece.GameObject.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, _selectedPiece.GameObject.transform.position.z);
            }
            else
                _selectedPiece.GameObject.transform.position = new Vector3(gridPieceOverlapped.GameObject.transform.position.x,
                                                                           gridPieceOverlapped.GameObject.transform.position.y,
                                                                           _selectedPiece.GameObject.transform.position.z);

            //if(_selectedPiece != null)
            //{
            //    Debug.Log(_selectedPiece.Name + ": " + _selectedPiece.GameObject.transform.position.ToString());
            //}
        }
    }

    private void HighlightGridPiece()
    {

        foreach (var gridPiece in _gridPieces)
        {
            bool flag = false;

            foreach (var piece in _puzzlePieces)
            {
                if ((piece.GameObject.transform.position - gridPiece.GameObject.transform.position).magnitude < _overlapThreshold)
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
                gridPiece.GameObject.GetComponent<Renderer>().material.color = Color.yellow;
            else
                gridPiece.GameObject.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    private void OnMouseDown()
    {
        if (isInit)
        {        
            if(Input.GetMouseButton(0))
            {
                isPuzzledDone = true;

                RaycastHit hitInfo1 = new RaycastHit();

                bool hit1 = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo1);

                //if (InStartPoints(ConvertPositionToPixelPosition(new Vector2(hitInfo1.point.x, hitInfo1.point.y))))
                //    Debug.Log("IN START POINTS");

                //if (InLinePoints(ConvertPositionToPixelPosition(new Vector2(hitInfo1.point.x, hitInfo1.point.y))))
                //    Debug.Log("IN LINE POINTS");

                if (!isMouseDown)
                {
                    Debug.Log("OnMouseDown");                    

                    if (!isPuzzledDone)
                    {
                        RaycastHit hitInfo = new RaycastHit();

                        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

                        if (hit)
                        {
                            if (_selectedPiece != null && _selectedPiece.GameObject.name == hitInfo.transform.gameObject.name)
                            {
                                _selectedPiece.IsSelected = false;
                                _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;

                                _selectedPiece = null;
                            }
                            else
                            {
                                for (int i = 0; i < _puzzlePieces.Count; i++)
                                {
                                    if (_puzzlePieces[i].GameObject.name == hitInfo.transform.gameObject.name)
                                    {
                                        float z = 0;

                                        Piece piece = _puzzlePieces[i];

                                        //Reset previous puzzle
                                        if (_selectedPiece != null)
                                        {
                                            _selectedPiece.IsSelected = false;
                                            _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                                        }

                                        z = _puzzlePieces[0].GameObject.transform.position.z;

                                        for (int j = 0; j < i; j++)
                                        {
                                            _puzzlePieces[j].GameObject.transform.position = new Vector3(_puzzlePieces[j].GameObject.transform.position.x,
                                                                                                         _puzzlePieces[j].GameObject.transform.position.y,
                                                                                                         _puzzlePieces[j + 1].GameObject.transform.position.z);
                                        }

                                        // Highlight new puzzle
                                        piece.IsSelected = true;
                                        piece.GameObject.GetComponent<Renderer>().material.mainTexture = piece.Highlighted;

                                        if (z != 0)
                                            piece.GameObject.transform.position = new Vector3(piece.GameObject.transform.position.x,
                                                                                              piece.GameObject.transform.position.y,
                                                                                              z);

                                        _selectedPiece = piece;

                                        _puzzlePieces = SortPieces(_puzzlePieces);

                                        break;
                                    }
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


                    }

                    isMouseDown = true;
                }                
            }                     
        }
    }

    Vector2 ConvertPositionToPixelPosition(Vector2 position)
    {
        Vector2 ret = Vector2.zero;

        ret.x = (int)(ConvertMetersToPixels(position.x - _firstGridPixelPosition.x) / _mainPuzzleTextureRatio);
        ret.y = (int)(ConvertMetersToPixels(position.y - _firstGridPixelPosition.y) / _mainPuzzleTextureRatio);

        if (ret.x < 0)
            ret.x *= (-1);

        if (ret.y < 0)
            ret.y *= (-1);

        return ret;
    }

    bool InStartPoints(Vector2 point)
    {
        foreach (var p in _startStopPoints)
        {
            if (p.x == point.x && p.y == point.y)
            {
                return true;
            }
        }

        return false;
    }

    bool InLinePoints(Vector2 point)
    {
        foreach (var p in _linePoints)
        {
            if (p.x == point.x && p.y == point.y)
            {
                return true;
            }
        }

        return false;
    }

    private void OnMouseUp()
    {
        if (!Input.GetMouseButton(0) && isMouseDown == true)
        {
            isMouseDown = false;

            if(isSketchStarted && !isSketchDoneSucessfully)
            {
                ResetTexture();
            }

            Debug.Log("OnMouseUp");
        }
    }

    private void ResetTexture()

    {
        Texture2D texture2D = _puzzleDoneObject.gameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
        foreach (var p in _sketchedPixels)
        {
            texture2D.SetPixel(p.X, p.Y, p.PreviousColor);

        }

        texture2D.Apply();

        _sketchedPixels.Clear();
        _puzzleDoneObject.gameObject.GetComponent<Renderer>().material.mainTexture = texture2D;
    }

    private void OnMouseDrag()
    {
        if (isInit)
        {
            Debug.Log("OnMouseDrag");

            RaycastHit hitInfo = new RaycastHit();

            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            
            if (hit)
            {
                if (_selectedPiece != null)
                {
                    _selectedPiece.IsSelected = false;
                    _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                }

                foreach (var piece in _puzzlePieces)
                {
                    if (piece.GameObject.name == hitInfo.transform.gameObject.name)
                    {
                        if (_selectedPiece != null && _selectedPiece.GameObject.name == hitInfo.transform.gameObject.name)
                        {
                            _selectedPiece.IsSelected = false;
                            _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;

                            _selectedPiece = null;
                        }
                        else
                        {
                            piece.IsSelected = true;
                            piece.GameObject.GetComponent<Renderer>().material.mainTexture = piece.Highlighted;

                            _selectedPiece = piece;
                        }
                    }
                }
            }
            else
            {
                if (_selectedPiece != null)
                {
                    _selectedPiece.IsSelected = false;
                    _selectedPiece = null;
                }
            }
        }
    }

    List<Piece> SortPieces(List<Piece> list)
    {
        for(int i = 0; i < list.Count - 1; i++)
        {
            for(int j = i+1; j < list.Count - 1; j++)
            {
                if(list[i].GameObject.transform.position.z > list[j].GameObject.transform.position.z)
                {
                    Piece tmp = list[i];
                    list[i] = list[j];
                    list[j] = tmp;
                }
            }
        }

        return list;
    }

    float ConvertPixelsToMeters(float pixels)
    {
        return pixels / _coefficientCoverterPixelToM;
    }

    float ConvertMetersToPixels(float meters)
    {
        return meters * _coefficientCoverterPixelToM;
    }

    GameObject CreateCubeGameObject(string name, Vector3 position, Quaternion rotation, Vector3 localScale, Material material, Texture2D texture, Color color)
    {
        GameObject newGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        newGameObject.name = name;

        newGameObject.transform.position = position;
        newGameObject.transform.rotation = rotation;
        newGameObject.transform.localScale = localScale;

        //newGameObject.AddComponent<MeshFilter>();
        //newGameObject.AddComponent<MeshRenderer>();

        if (material != null)
            newGameObject.GetComponent<Renderer>().material = material;
        else if (texture != null)
            newGameObject.GetComponent<Renderer>().material.mainTexture = texture;
        else 
            newGameObject.GetComponent<Renderer>().material.color = color;

        return newGameObject;
    }

    Vector2 RescaleArea(Vector2 orginal, Vector2 scale)
    {
        if(orginal.x / scale.x > orginal.y / scale.y)
        {
            orginal.x = scale.x * orginal.y / scale.y;
        }
        else
        {
            orginal.y = orginal.x / scale.x * scale.y;
        }

        return orginal;
    }

    int StandardizeRotationAngle(float angle, int standardThreshold = 10)
    {
        int coef = (int)(angle / 360);


        if (coef > 0)
        {
            angle -= coef * 360;
        }
        else if (coef < 0)
        {
            angle += coef * 360;
        }

        int dif = Mathf.RoundToInt(angle / _differentZThreshold);

        return dif * standardThreshold;
    }

    float NormalizeScaleLevel(float newScale, float orginialScale, float scaleSpace)
    {
        int time = (int)((newScale - orginialScale) / scaleSpace);

        return orginialScale + time * scaleSpace;
    }

    Vector3 StandardizeScale(Vector3 newScale, Vector3 standardScale, float scaleSpace)
    {
        int time = (int)((newScale.x - standardScale.x) / scaleSpace);

        return new Vector3(standardScale.x + scaleSpace * time, standardScale.y + scaleSpace * time, standardScale.z);
    }

    Piece PuzzlePieceRotateLeft(Piece piece)
    {
        piece.GameObject.transform.Rotate(Vector3.forward, _differentAngle);

        return piece;
    }

    Piece PuzzlePieceRotateRight(Piece piece)
    {
        piece.GameObject.transform.Rotate(Vector3.forward, (-1) * _differentAngle);

        return piece;
    }

    Piece PuzzlePieceScaleUp(Piece piece)
    {
        piece.CurrentScaleLevel += _differentScale;
        piece.GameObject.transform.localScale = new Vector3(_standardPieceScale.x * piece.CurrentScaleLevel,
                                                            _standardPieceScale.y * piece.CurrentScaleLevel,
                                                            _standardPieceScale.z);

        return piece;
    }

    Piece PuzzlePieceScaleDown(Piece piece)
    {
        piece.CurrentScaleLevel -= _differentScale;

        piece.GameObject.transform.localScale = new Vector3(_standardPieceScale.x * piece.CurrentScaleLevel,
                                                            _standardPieceScale.y * piece.CurrentScaleLevel,
                                                            _standardPieceScale.z);

        return piece;
    }

    Vector3 ScaleUp(Vector3 currentScale, float scaleLevel)
    {
        float x = currentScale.x + scaleLevel;
        float y = x * currentScale.y / currentScale.x;
        float z = currentScale.z;
        return new Vector3(x, y, z);
    }

    Vector3 ScaleDown(Vector3 currentScale, float scaleLevel)
    {
        float x = NormalizeScaleLevel(currentScale.x - scaleLevel, currentScale.x, scaleLevel);
        float y = x * currentScale.y / currentScale.x;
        float z = currentScale.z;
        return new Vector3(x, y, z);
    }
}
