using Assets.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;


public class HandStreamDecoder : MonoBehaviour
{
    const int FPS = 20;
    bool updateFrame = false;
    Texture2D handVisTexture;
    const int initWidth = 2;
    const int initHeight = 2;
    byte[] CurFrameData;
    MemoryStream handFrameStream;
    Material mainHandMaterial;
    Stream handStream;
    WebResponse response;
    bool responseReceived = false;
    bool isStreaming = false;

    double frameInterval;
    double timer;
    void Start()
    {
        mainHandMaterial = GetComponent<Renderer>().material;
        handVisTexture = new Texture2D(initWidth, initHeight, TextureFormat.RGBA4444, false);
        handVisTexture.alphaIsTransparency = true;
        frameInterval = 1000 / FPS;
        timer = 0;
        startStreamingTask();
    }
    void startStreamingTask()
    {
        responseReceived = false;
        string streamAddress = "http://" + GlobalUtilities.LoadHandStreamAddress();
        Debug.Log("Start streaming from: " + streamAddress);
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(streamAddress));
        //response = request.GetResponse();
        request.BeginGetResponse(OnGetResponse, request);
    }
    private void OnGetResponse(IAsyncResult asyncResult)
    {
        responseReceived = true;
        Debug.Log("OnGetResponse");
        HttpWebRequest req = (HttpWebRequest)asyncResult.AsyncState;
        response = (HttpWebResponse)req.EndGetResponse(asyncResult);
        handStream = response.GetResponseStream();
    }
    IEnumerator GetFrame()
    {
        while(isStreaming)
        {
            int bytesToRead = FindLength(handStream);
            
            if(bytesToRead == -1)
            {
                yield break;
            }
            CurFrameData = new byte[bytesToRead];
            int leftToRead = bytesToRead;
            while(leftToRead >0)
            {
                
                leftToRead -= handStream.Read(CurFrameData, bytesToRead - leftToRead, leftToRead);
                yield return null;
            }
            handFrameStream = new MemoryStream(CurFrameData, 0, bytesToRead, false, true);
            handStream.ReadByte();
            handStream.ReadByte();
            updateFrame = true;
        }
        if(!isStreaming)
        { 
            //handStream.Close();
            //response.Close();
        }
    }
    void Update()
    {
        timer += Time.deltaTime*1000;
        if(responseReceived)
        {
            responseReceived = false;
            isStreaming = true;
            StartCoroutine(GetFrame());
        }
        if (updateFrame)
        {
            
            if (timer > frameInterval)
            {
                handVisTexture.LoadImage(handFrameStream.GetBuffer());
                //handVisTexture.alphaIsTransparency = true;
                mainHandMaterial.mainTexture = handVisTexture;
                updateFrame = false;
                timer = timer % frameInterval;
            }
        }
    }
    void StopStreaming()
    {
        isStreaming = false;
        Thread.Sleep(500);
        handStream.Close();
        response.Close();
    }
    void OnApplicationQuit()
    {
        StopStreaming();
       
    }
    int FindLength(Stream stream)
    {
        int b;
        string line = "";
        int result = -1;
        bool atEOL = false;

        while ((b = stream.ReadByte()) != -1)
        {
            if (b == 10) continue; // ignore LF char
            if (b == 13)
            { // CR
                if (atEOL)
                {  // two blank lines means end of header
                    stream.ReadByte(); // eat last LF
                    return result;
                }
                if (line.StartsWith("Content-Length:"))
                {
                    result = Convert.ToInt32(line.Substring("Content-Length:".Length).Trim());
                }
                else
                {
                    line = "";
                }
                atEOL = true;
            }
            else
            {
                atEOL = false;
                line += (char)b;
            }
        }
        return -1;
    }
}
