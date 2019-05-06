using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script
{
    public class GlobalData
    {
        public static string SERVER_ADDR = "192.168.0.100:8080";
        public static string LoadServerAddress()
        {
            string serverAddr = "";
            string filePath = "/Assets/Resources/VRSlateServerAddress.txt";
            if(Application.platform == RuntimePlatform.WindowsEditor)
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
                    sw.WriteLine(GlobalData.SERVER_ADDR);
                    sw.Close();
                }
                using (StreamReader sr = File.OpenText(filePath))
                {
                    serverAddr = sr.ReadLine();
                }
            }
            
            if(serverAddr == "")
            {
                //serverAddr = SERVER_ADDR;
            }
            return serverAddr;
        }
        
    }
}
