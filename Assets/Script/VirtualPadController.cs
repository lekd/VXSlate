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
    public GameObject eventListennerObject;
    public GameObject gameCameraObject;
    public GameObject gazePointObject;
    public GameObject boardObject;
    public GameObject finger1;
    public GameObject finger2;
    public GameObject finger3;
    public GameObject finger4;
    public GameObject finger5;
    GameObject[] fingers;

    Bounds boardObjectBound;

    // Variables for timer;
    float translationTime = 0f;
    Vector2 translationVector = Vector2.zero;

    Camera gameCamera;
    Transform gazePoint;

    void Start()
    {
        if (gameCameraObject)
        {
            gameCamera = gameCameraObject.GetComponent<Camera>();
        }

        if (gazePointObject)
            gazePoint = gazePointObject.GetComponent<Transform>();

        boardObjectBound = boardObject.GetComponent<Collider>().bounds;
        fingers = new GameObject[] { finger1, finger2, finger3, finger4, finger5 };
        for(int i=0; i< fingers.Length; i++)
        {
            fingers[i].SetActive(false);
        }
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


        //Update Virtual Pad Following Camera Lookat Vector
        UpdateVirtualPadBasedOnCamera();
    }

    private void UpdateVirtualPadBasedOnCamera()
    {

        if (!gameCamera)
        {
            return;
        }
        Bounds virtualPadBound = gameObject.GetComponent<Collider>().bounds;
        Rect pad2DContainerLimit = new Rect();
        pad2DContainerLimit.xMin = boardObjectBound.min.x + virtualPadBound.size.x / 2;
        pad2DContainerLimit.xMax = boardObjectBound.max.x - virtualPadBound.size.x / 2;
        pad2DContainerLimit.yMin = boardObjectBound.min.y + virtualPadBound.size.y / 2;
        pad2DContainerLimit.yMax = boardObjectBound.max.y - virtualPadBound.size.y / 2;

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
                    Vector3 newPadPos = GlobalUtilities.boundPointToContainer(hitPos, pad2DContainerLimit);
                    gameObject.transform.Translate(new Vector3(newPadPos.x - curPadPos.x, newPadPos.y - curPadPos.y, newPadPos.z - curPadPos.z + VIRTUALPAD_DEPTHOFFSET));
                }
            }
        }
    }
}
