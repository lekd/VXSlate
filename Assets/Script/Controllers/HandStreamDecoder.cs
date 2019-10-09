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
    int curHandFrameSize = 0;
    MemoryStream handFrameStream;
    MemoryStream copiedHandFrameStream;
    Material mainHandMaterial;
    Stream handStream;
    WebResponse response;
    bool responseReceived = false;
    bool isStreaming = false;

    double frameInterval;
    double timer;

    object handstreamLock = new object();
    void Start()
    {
        mainHandMaterial = GetComponent<Renderer>().material;
        handVisTexture = new Texture2D(initWidth, initHeight, TextureFormat.ARGB32,false);
        //handVisTexture.alphaIsTransparency = true;
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
        isStreaming = true;
        Debug.Log("OnGetResponse");
        HttpWebRequest req = (HttpWebRequest)asyncResult.AsyncState;
        response = (HttpWebResponse)req.EndGetResponse(asyncResult);
        handStream = response.GetResponseStream();
        //read hand stream immediately once response received
        while (isStreaming)
        {
            //lock (handstreamLock)
            //{
            int bytesToRead = FindLength(handStream);
            curHandFrameSize = bytesToRead;

            if (bytesToRead == -1)
            {
                return;
            }
            CurFrameData = new byte[bytesToRead];
            int leftToRead = bytesToRead;
            while (leftToRead > 0)
            {
                leftToRead -= handStream.Read(CurFrameData, bytesToRead - leftToRead, leftToRead);
            }
            //}
            lock(handstreamLock)
            { 
                handFrameStream = new MemoryStream(CurFrameData, 0, bytesToRead, false, true);
                updateFrame = true;
            }
            handStream.ReadByte();
            handStream.ReadByte();
        }
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
    byte[] copiedHandFrameBuffer;
    IEnumerator GrabFrame()
    {
        lock (handstreamLock)
        {
            //handFrameStream = new MemoryStream(CurFrameData, 0, curHandFrameSize, false, true);
            copiedHandFrameBuffer = new byte[handFrameStream.GetBuffer().Length];
            handFrameStream.GetBuffer().CopyTo(copiedHandFrameBuffer, 0);
            //copiedHandFrameStream = new MemoryStream(copiedHandFrameBuffer, false);
            updateFrame = true;
            Debug.Log("Retrieved frame of hand images");
        }
        yield return null;
    }
    void Update()
    {
        timer += Time.deltaTime*1000;
        if(responseReceived)
        {
            //responseReceived = false;
            //isStreaming = true;
            StartCoroutine(GrabFrame());
        }
        if (updateFrame)
        {
            
            if (timer > frameInterval)
            {
                lock(handstreamLock)
                {
                    //handVisTexture.LoadImage(handFrameStream.GetBuffer());
                    handVisTexture.LoadImage(copiedHandFrameBuffer);
                    Debug.Log("Showing hand stream");
                    updateFrame = false;
                }
                handVisTexture.alphaIsTransparency = true;
                mainHandMaterial.mainTexture = handVisTexture;
                timer = timer % frameInterval;
            }
        }
    }
    void StopStreaming()
    {
        isStreaming = false;
        Thread.Sleep(500);

        if (handStream != null)
            handStream.Close();

        if(response != null)
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
