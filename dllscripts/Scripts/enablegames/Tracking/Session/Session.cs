using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using FullSerializer;
using DG.Tweening;
using System.Text;

public class Session
{
    public static string FileExtension = ".json";
    public static string ResultFolder = Path.Combine(Application.persistentDataPath, "AbleSessions");
    //public static string ResultFolder = "AbleSessions";
    [fsProperty]
    private string name;

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    [fsProperty]
    private List<TrackerMessage> messages;
    [fsIgnore]
    public List<TrackerMessage> Messages
    {
        get
        {
            return messages;
        }
    }

    private List<TrackerMessage> sendToBackend;
    private int numToSend = 30;

    private StreamWriter fs;

    [fsProperty]
    private string fn = "";

    private string Filename
    {
        get
        {
            if (fn != "")
            {
                return fn;
            }
            int fileNumber = 0;
            string n = this.name;
            if (n == "")
            {
                n = "DefaultSession";
            }
            DirectoryInfo info = new DirectoryInfo(ResultFolder);
            if (info.Exists == false)
            {
                info.Create();
            }
            string filename = Path.Combine(ResultFolder, n + FileExtension);
            if (File.Exists(filename))
            {
                do
                {
                    filename = Path.Combine(ResultFolder, n + "_" + fileNumber.ToString() + FileExtension);
                    fileNumber++;
                } while (File.Exists(filename));
            }
            return filename;
        }
    }

    public Session(string sessionName) :
        this()
    {
		Debug.Log ("Session(name)=" + sessionName);
        name = sessionName;
    }

    public Session()
    {
        name = "Default AbleGames Session";
        fs = new StreamWriter(Filename);
		Debug.Log ("Session()=" + Filename);
        fs.WriteLine("{\n\"messages\" : [");
        fs.Flush();
        messages = new List<TrackerMessage>();
        messages.Add(new TrackerMessage("Session Event", "Session Created"));
        sendToBackend = new List<TrackerMessage>();
    }

    public void AddMessage(TrackerMessage message)
    {
        AddMessage(message, false);
    }

    public void AddMessage(TrackerMessage message, bool closeJson)
    {
        if (!streamAlive)
        {
            return;
        }
        //Debug.Log("Session Message Added");
        messages.Add(message);
        fs.WriteLine(message.SerializedValue + (closeJson ? "]}" : ","));
        fs.Flush();
        if (!closeJson)
        {
            SendMessageToBackend(message);
        }
        else
        {
            FlushMessagesToBackend();
        }
    }

    private void SendMessageToBackend(TrackerMessage message)
    {
        // store up our list of messages to send to backend
        if (message.Key.Equals("Hand Game Position") ||
            message.Key.Equals("Avatar Skeleton") ||
            message.Key.Equals("Footer"))
        {
            sendToBackend.Add(message);
        }
        if (sendToBackend.Count >= numToSend)
        {
            FlushMessagesToBackend();
        }
    }

    private void FlushMessagesToBackend()
    {
        // generate our json to send to backend
        Debug.Log("sending json to backend");
        StringBuilder data = new StringBuilder("[");
        for (int i = 0; i < sendToBackend.Count; ++i)
        {
            data.Append(sendToBackend[i].SerializedValue);
            if (i < sendToBackend.Count - 1)
            {
                data.Append(",");
            }
        }
        data.Append("]");
        // send the data to backend
        EnableAPI.Instance.PushSessionData(data.ToString());
        // reset the list
        sendToBackend.Clear();
    }

    private bool streamAlive = true;
    public void Save()
    {
        //Debug.Log(JSONString());
#if !UNITY_WEBPLAYER
        AddMessage(new TrackerMessage("Session Event", "Session Saved"), true); // close the json (end the file)
        fs.Close();
        streamAlive = false;
        //JSONSerializer.Serialize(typeof(Session), this, Filename);
        //File.WriteAllText(filename, this.JSONString());
#endif
    }

    /// <summary>
    /// Serialize the session into JSON. A weird fullserializer issue adds backslashes when serializing nested JSON objects. The backslash gets removed here.
    /// DO NOT USE BACKSLASHES IN MESSAGES!
    /// </summary>
    /// <returns></returns>
    public string JSONString()
    {
        // return (JSONSerializer.Serialize(typeof(Session), this)).Replace(@"\", "");
        return JSONSerializer.Serialize(typeof(Session), this);
    }
}
