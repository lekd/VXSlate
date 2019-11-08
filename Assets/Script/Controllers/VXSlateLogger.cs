using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Script.Controllers
{
    public class VXSlateLogger
    {
        string fileName = "";
        string fileLocation = ".\\Assets\\ExperimentResults\\VXSlate_Activities_Log\\";
        StreamWriter streamWriter;
        List<string> loggedData = new List<string>();
        public void init(string participantID,string order,string textureID,string isTraining)
        {
            fileName = string.Format("VXSlateLog_{0}_{1}_{2}.csv", participantID, order, textureID);
            streamWriter = new StreamWriter(fileLocation + fileName);
            streamWriter.WriteLine("ParticipantID,Order,textureID,IsTraining,Stage,IsPadMoved,MoveX,MoveY,IsPadScaled,ScaleX,ScaleY");
        }
        public void WriteVirtualPadTranslation(string participandID,string order,string textureID,string stage, double moveX,double moveY)
        {
            /*streamWriter.WriteLine(participandID + ","
                               + order + ","
                               + textureID + ","
                               + stage + ","
                               + "TRUE"
                               + moveX.ToString() + ","
                               + moveY.ToString() + ","
                               + "FALSE,"
                               + "0,0");*/
            loggedData.Add(participandID + ","
                               + order + ","
                               + textureID + ","
                               + stage + ","
                               + "TRUE"
                               + moveX.ToString() + ","
                               + moveY.ToString() + ","
                               + "FALSE,"
                               + "0,0");
        }
        public void WriteVirtualPadScaling(string participandID, string order, string textureID, string stage, double scaleX, double scaleY)
        {
            /*streamWriter.WriteLine(participandID + ","
                               + order + ","
                               + textureID + ","
                               + stage + ","
                               + "FALSE"
                               + "0,0,"
                               + "TRUE,"
                               + scaleX.ToString() + ","
                               + scaleY.ToString());*/
            loggedData.Add(participandID + ","
                               + order + ","
                               + textureID + ","
                               + stage + ","
                               + "FALSE"
                               + "0,0,"
                               + "TRUE,"
                               + scaleX.ToString() + ","
                               + scaleY.ToString());
        }
        public void Close()
        {
            for(int i=0; i< loggedData.Count;i++)
            {
                streamWriter.WriteLine(loggedData[i]);
            }
            loggedData.Clear();
            streamWriter.Close();
        }
    }
}
