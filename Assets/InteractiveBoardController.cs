using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveBoardController : MonoBehaviour
{
    public GameObject virtualPad;

    // Local variables for handling virtual pad information
    VirtualPadController virtualPadController;
    void Start()
    {
        if(virtualPad != null)
            virtualPadController = virtualPad.GetComponent<VirtualPadController>();

        // example usage
        // set center of the virtual pad
        if(virtualPadController != null)
            virtualPadController.SetCenter(Vector2.zero);
        //translate the virtual pad

        if (virtualPadController != null)
            virtualPadController.Translate(new Vector2(3, 4), 10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
