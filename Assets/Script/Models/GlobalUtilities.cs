using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script
{
    public class GlobalUtilities
    {
        public enum DEVICES { TABLET, CONTROLER, MOUSE, NONE}
        public static string SERVER_ADDR = "192.168.0.103:8080";
        public static string HAND_STREAM_ADDR = "192.168.0.101:4040";
        public static LoggingVariable globalGameState = null;
        public static string _curParticipantID = "";
        public static DEVICES curControlDevice = DEVICES.NONE;
        public static string LoadServerAddress()
        {
            string serverAddr = "";
            string filePath = "./Assets/Resources/VRSlateServerAddress.txt";
            /*if(Application.platform == RuntimePlatform.W)
            {
                filePath = "/Assets/Resources/VRSlateServerAddress.txt";
                TextAsset serverAddrTxt = (TextAsset)Resources.Load("VRSlateServerAddress", typeof(TextAsset));
                serverAddr = serverAddrTxt.text;
            }
            else if(Application.platform == RuntimePlatform.Android)
            {
                filePath = Application.persistentDataPath + "/VRSlateServerAddress.txt";
                if(!File.Exists(filePath))
                {
                    FileStream Fs = File.Create(filePath);
                    StreamWriter sw = new StreamWriter(Fs);
                    sw.WriteLine(GlobalUtilities.SERVER_ADDR);
                    sw.Close();
                }
                using (StreamReader sr = File.OpenText(filePath))
                {
                    serverAddr = sr.ReadLine();
                }
            }
            
            if(serverAddr == "")
            {
                serverAddr = SERVER_ADDR;
            }*/
            /*using (StreamReader fileReader = new StreamReader(filePath))
            {
                serverAddr = fileReader.ReadLine();
                fileReader.Close();
            }
            if(serverAddr == "")
            {
                serverAddr = SERVER_ADDR;
            }*/
            TextAsset addressesTxt = (TextAsset)Resources.Load("VRSlateServerAddress", typeof(TextAsset));
            string allText = addressesTxt.text;
            char[] separators = {'\n' };
            string[] componentStrs = allText.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            serverAddr = componentStrs[0];
            return serverAddr;
        }
        
        public static string LoadHandStreamAddress()
        {
            string handStreamAddr = "";
            /*string filePath = "./Assets/Resources/VRSlateServerAddress.txt";
            using (StreamReader fileReader = new StreamReader(filePath))
            {
                fileReader.ReadLine();
                handStreamAddr = fileReader.ReadLine();
                fileReader.Close();
            }
            if (handStreamAddr == "")
            {
                handStreamAddr = HAND_STREAM_ADDR;
            }*/
            TextAsset addressesTxt = (TextAsset)Resources.Load("VRSlateServerAddress", typeof(TextAsset));
            string allText = addressesTxt.text;
            char[] separators = { '\n' };
            string[] componentStrs = allText.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            handStreamAddr = componentStrs[1];
            return handStreamAddr;
        }
        public static Vector2 ConvertMobileRelPosToUnityRelPos(Vector2 mobileRelPos)
        {
            return new Vector2(mobileRelPos.x - 0.5f, 0.5f - mobileRelPos.y);
        }
        static public Vector3 boundPointToContainer(Vector3 src, Rect container2D)
        {
            Vector3 boundedPoint = new Vector3(src.x, src.y, src.z);
            if (boundedPoint.x < container2D.xMin)
            {
                boundedPoint.x = container2D.x;
            }
            else if (boundedPoint.x > container2D.xMax)
            {
                boundedPoint.x = container2D.xMax;
            }
            if (boundedPoint.y < container2D.yMin)
            {
                boundedPoint.y = container2D.yMin;
            }
            else if (boundedPoint.y > container2D.yMax)
            {
                boundedPoint.y = container2D.yMax;
            }
            return boundedPoint;
        }
        static public int ByteArray2Int(byte[] byteData)
        {
            //if(!BitConverter.IsLittleEndian)
            //{
                Array.Reverse(byteData);
            //}
            return BitConverter.ToInt32(byteData,0);
        }
        static public float ByteArray2Float(byte[] byteData)
        {
            //if (!BitConverter.IsLittleEndian)
            //{
                Array.Reverse(byteData);
            //}
            return BitConverter.ToSingle(byteData, 0);
        }
        static public string getValuesString(byte[] byteData)
        {
            string str = "";
            for(int i=0; i< byteData.Length; i++)
            {
                str += string.Format("{0} ", (int)(byteData[i]));
            }
            return str;
        }
    }
}
