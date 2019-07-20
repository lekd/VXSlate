using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayRenderer : MonoBehaviour
{
    LineRenderer _laserLine;
    // Start is called before the first frame update
    void Start()
    {
        _laserLine = GetComponent<LineRenderer>();
        _laserLine.startWidth = 0.0025f;
        _laserLine.endWidth = 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        _laserLine.SetPosition(0, this.transform.position);

        RaycastHit hitInfo = new RaycastHit();

        bool hit = Physics.Raycast(this.transform.position, this.transform.rotation * Vector3.forward, out hitInfo);

        if (hit)
        {
            _laserLine.SetPosition(1, hitInfo.point);
        }
    }
}
