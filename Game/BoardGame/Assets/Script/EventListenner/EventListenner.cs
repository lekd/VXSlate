using BoardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventListenner : MonoBehaviour
{
    //[Header("Object")]
    //public Piece _selectedPiece = null;
    [Header("Interaction Technique")]
    public bool _isMouse = false;
    public bool _isController = false;
    public bool _isTablet = false;

    [Header("Event")]
    public bool NONE = true;
    public bool CLICKDOWN = false;
    public bool CLICKUP = false;
    public bool OBJECT_SCALING = false;
    public bool OBJECT_ROTATING = false;

    [Header("Variables")]
    public float scalingLevel;
    public float rotatingAngle;
    public Vector2 touchPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        NONE = true;
        CLICKDOWN = false;
        CLICKUP = false;
        OBJECT_SCALING = false;
        OBJECT_ROTATING = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(_isMouse)
        {
            HandleMouseEvent();
        }

        if(_isController)
        {

        }

        if(_isTablet)
        {

        }
    }

    private void HandleMouseEvent()
    {
        if (Input.GetMouseButton(0))
        {
            NONE = false;
            CLICKDOWN = true;
            CLICKUP = false;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                OBJECT_SCALING = true;
                Debug.Log("Puzzle piece is scaled down!");
            } else if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                OBJECT_SCALING = true;
                Debug.Log("Puzzle piece is scaled up!");
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                OBJECT_ROTATING = true;

                Debug.Log("Puzzle piece is rotated left!");
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                OBJECT_ROTATING = true;
                Debug.Log("Puzzle piece is rotated right!");
            }
        }
        else
        {
            CLICKDOWN = false;
            CLICKUP = true;
            NONE = true;
        }
    }
}
