using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualPadController : MonoBehaviour
{
    //Public Game Objects
    public GameObject eventListenner;
    public GameObject gameCamera;
    public GameObject gazePoint;

    // 2D Coordinates of the Center of the Virtual Pad
    Vector2 center; //Center point of the virtual pad

    Vector2 A; // Top left point
    Vector2 B; // Top right point
    Vector2 C; // Bottom right point
    Vector2 D; // Bottom left point

    // Variables for timer;
    float translationTime;
    Vector2 translationVector;

    float rotationTime;
    float rotationAngle;
    float rotationStartTime;

    void Start()
    {
        center = A = B = C = D = Vector2.zero;

        translationTime = rotationTime = 0f;
        translationVector = Vector2.zero;
        rotationTime = rotationAngle = rotationStartTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(center);

        // Translate the pad to a new position
        if(translationTime > 0)
        {
            translationTime -= Time.deltaTime;
            this.transform.Translate(translationVector * Time.deltaTime);
        }
        else
        {
            translationTime = 0;
        }
    }

    /// <summary>
    /// Init the Virtual Pad
    /// </summary>
    /// <param name="centerPoint">The center point of the pad</param>
    /// <param name="a">Top left position</param>
    /// <param name="b">Top right position</param>
    /// <param name="c">Bottom right position</param>
    /// <param name="d">Bottom left position</param>
    public void Init(Vector2 centerPoint, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        center = centerPoint;
        A = a;
        B = b;
        C = c;
        D = d;
    }

    /// <summary>
    /// Set the center of the virtual pad
    /// </summary>
    /// <param name="c">New center point</param>
    public void SetCenter(Vector2 c)
    {
        center = c;
    }

    /// <summary>
    /// Set the boundaries of the virtual pad
    /// </summary>
    /// <param name="a">Top left point</param>
    /// <param name="b">Top right point</param>
    /// <param name="c">Bottom right point</param>
    /// <param name="d">Bottom left point</param>
    public void SetBoundaries(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        A = a;
        B = b;
        C = c;
        D = d;
    }

    /// <summary>
    /// Get the center of the virtual pad
    /// </summary>
    /// <returns></returns>
    public Vector2 GetCenter()
    {
        return center;
    }

    /// <summary>
    /// Get the boundaries of the virtual pad
    /// </summary>
    /// <returns></returns>
    public Vector2[] GetBoundaries()
    {
        Vector2[] ret = new Vector2[4];
        ret[0] = A;
        ret[1] = B;
        ret[2] = C;
        ret[3] = D;

        return ret;
    }

    /// <summary>
    /// Translate the virtual pad to a new center in a "time" period
    /// </summary>
    /// <param name="newCenter"></param>
    /// <param name="time"></param>
    public void Translate(Vector2 newCenter, float time)
    {
        Debug.Log("Translate");
        translationTime = time;
        translationVector = newCenter - center;

        // Update the position of the center and boundaries
        center = newCenter;
        A += translationVector;
        B += translationVector;
        C += translationVector;
        D += translationVector;
    }

    /// <summary>
    /// Rotate the virtual pad with an "angle" in a "time" period
    /// </summary>
    /// <param name="angle">Rotate angle</param>
    /// <param name="time">Rotate duration</param>
    public void Rotate(float angle, float time)
    {
        rotationTime = time;
        rotationAngle = angle;

        rotationStartTime = Time.time;
    }

    //void RotateBoundaries()
    //{
    //    FindNewPositionAfterRotating(A);
    //    FindNewPositionAfterRotating(B);
    //    FindNewPositionAfterRotating(C);
    //    FindNewPositionAfterRotating(D);
    //}

    //void FindNewPositionAfterRotating(Vector2 point)
    //{
    //    Vector2 pointCenter = point - center;
    //    Vector2 perpendicularVector = new Vector2(pointCenter.y, -pointCenter.x);
    //    Quaternion rotationQuaternion = Quaternion.AngleAxis(rotationAngle, perpendicularVector);
    //    Vector2 newPointCenter = pointCenter * rotationQuaternion;

    //    float x = 0;
    //    float y = 0;

    //    y = ((pointCenter.magnitude * pointCenter.magnitude) * Mathf.Acos(rotationAngle) - pointCenter.x * x) / pointCenter.y;

    //    x ^ 2 = pointCenter.magnitude * pointCenter.magnitude - y ^ 2;
    //}
}
