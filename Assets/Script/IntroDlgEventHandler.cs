using Assets.Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

public class IntroDlgEventHandler : MonoBehaviour
{
    public InputField serverAddrIF;
    // Start is called before the first frame update
    void Start()
    {
        //Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GoToVirtualControlRoom()
    {
        if (serverAddrIF.text.CompareTo("") != 0)
        {
            GlobalData.SERVER_ADDR = serverAddrIF.text;
            SceneManager.LoadScene(1);
        }
    }
    
}
