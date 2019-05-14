using Assets.Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGame : MonoBehaviour
{
    public GameObject remoteControllerObj;
    public GameObject gameCharacterObj;
    Vector2 gameSize = new Vector2();
    // Start is called before the first frame update
    IRemoteController remoteController;
    SimpleCharacter gameCharacter;
    void Start()
    {
        gameCharacterObj.transform.localPosition.Set(0, 0, -0.00001f);
        remoteController = remoteControllerObj.GetComponent<IRemoteController>();
        if(remoteController != null)
        {
            GestureRecognizedEventCallback gestureRecognizedHandler = this.handleControlGesture;
            remoteController.setGestureRecognizedCallback(gestureRecognizedHandler);
        }
        gameCharacter = gameCharacterObj.GetComponent<SimpleCharacter>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void handleControlGesture(TouchGesture gesture)
    {
        if(gameCharacter != null)
        {
            gameCharacter.handleGesture(gesture);
        }
    }
}
