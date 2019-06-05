using Assets.Script;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SimpleGame : MonoBehaviour
{
    public GameObject remoteControllerObj;
    public GameObject gameCharacterObj;
    private Texture2D screenTexture;
    Vector2 gameSize = new Vector2();
    // Start is called before the first frame update
    IRemoteController remoteController;
    SimpleCharacter gameCharacter;
    Color[] paintColors = new Color[1];
    Point2D drawnPoint = new Point2D();
    bool hasThingToDraw = false;
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
    }

    // Update is called once per frame
    void Update()
    {
        if(hasThingToDraw)
        {
            drawOnTexture(screenTexture, drawnPoint.X, drawnPoint.Y);
            hasThingToDraw = false;
            gameObject.GetComponent<Renderer>().material.mainTexture = screenTexture;
        }
    }
    void handleControlGesture(TouchGesture gesture)
    {
        bool handledByCharacter = false;
        if(gameCharacter != null)
        {
            handledByCharacter = gameCharacter.handleGesture(gesture);
        }
        if(!handledByCharacter)
        {
            if(gesture.GestureType == GestureType.SINGLE_TOUCH_DOWN
                || gesture.GestureType == GestureType.SINGLE_TOUCH_MOVE)
            {
                hasThingToDraw = true;
                Vector2 localTouchPos = (Vector2)gesture.MetaData;
                drawnPoint.X = (int)((localTouchPos.x + 0.5) * screenTexture.width);
                drawnPoint.Y = (int)((localTouchPos.y + 0.5) * screenTexture.height);
            }
        }
    }
    void drawOnTexture(Texture2D tex, int posX, int posY)
    {
        //tex.SetPixels(posX, posY, 5, 5, paintColors);
        tex.SetPixel(posX, posY, Color.red);
    }
}
