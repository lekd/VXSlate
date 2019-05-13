using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGame : MonoBehaviour
{
    public GameObject remoteControllerObj;
    public Texture2D blankBackgroud;
    public Texture2D dummyCharacterTexture;
    Texture2D mainGameTexture;
    Vector2 gameSize = new Vector2();
    // Start is called before the first frame update
    void Start()
    {

        gameSize.x = gameObject.transform.GetComponent<Collider>().bounds.size.x;
        gameSize.y = gameObject.transform.GetComponent<Collider>().bounds.size.y;
        mainGameTexture = new Texture2D(Mathf.RoundToInt(gameSize.x), Mathf.RoundToInt(gameSize.y));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
