using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

public class Listener : MonoBehaviour
{
    Thread thread;
    public int connectionPort = 25001;
    TcpListener server;
    TcpClient client;
    bool running;
    List<GameObject> lines = new List<GameObject>();
    public GameObject JointPrefab;

    // Start is called before the first frame update
    void Start()
    {
        // Receive on a separate thread so Unity doesn't freeze waiting for data
        ThreadStart ts = new ThreadStart(GetData);
        thread = new Thread(ts);
        thread.Start();
    }

    void GetData()
    {
        // Create the server
        server = new TcpListener(IPAddress.Any, connectionPort);
        server.Start();

        // Create a client to get the data stream
        client = server.AcceptTcpClient();

        // Start listening
        running = true;
        while (running)
        {
            Connection();
        }
        server.Stop();
    }

    // Update is called once per frame
    void Connection()
    {
        // Read data from the network stream
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

        // Decode the bytes into a string
        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        // Make sure we're not getting an empty string
        //dataReceived.Trim();
        if (dataReceived != null && dataReceived != "")
        {
            // Convert the received string of data to the format we are using
            positions = ParseData(dataReceived);
            nwStream.Write(buffer, 0, bytesRead);
        }
    }

    // Use-case specific function, need to re-write this to interpret whatever data is being sent
    public static Vector4[] ParseData(string dataString)
    {
        Debug.Log(dataString);
        // Remove the parentheses
        if (dataString.StartsWith("(") && dataString.EndsWith(")"))
        {
            dataString = dataString.Substring(1, dataString.Length - 2);
        }
       
        
        // Split the elements into an array
        string[] stringArray = dataString.Split('|');
        Vector4[] cords = new Vector4[stringArray.Length];
        for (int i = 0; i <stringArray.Length; i++)
        {
            string[] pos = stringArray[i].Split(',');
            // Store as a Vector3
            Vector4 result = new Vector4(
                float.Parse(pos[0]),
                float.Parse(pos[1]),
                float.Parse(pos[2]),
                float.Parse(pos[3]));
            cords[i] = result;
        }

        return cords;
    }

    // Position is the data being received in this example
    Vector4[] positions = new Vector4[0];

    void Update()
    {
        for(int i =0; i < positions.Length;i++)
        {
            this.gameObject.transform.GetChild((int)positions[i][0]).gameObject.transform.position = new Vector3(positions[i][1], positions[i][2], positions[i][3]);
        }
        drawLines();
        // Set this object's position in the scene according to the position received
    }


    void drawLines()
    {
        foreach(GameObject g in lines)
        {
            Destroy(g);
        }
        lines.Clear();
        
        for(int i =1; i < 21; i++)
        {
            var go = new GameObject();
            var go2 = new GameObject();
            var lr = go.AddComponent<LineRenderer>();
            var lr2 = go2.AddComponent<LineRenderer>();
            if (i == 5)
            {
                lr.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr.SetPosition(1, this.gameObject.transform.GetChild(0).gameObject.transform.position);

                lr2.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr2.SetPosition(1, this.gameObject.transform.GetChild(1).gameObject.transform.position);

            }
            else if(i == 9)
            {
                lr.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr.SetPosition(1, this.gameObject.transform.GetChild(0).gameObject.transform.position);

                lr2.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr2.SetPosition(1, this.gameObject.transform.GetChild(5).gameObject.transform.position);
            }
            else if(i == 13)
            {
                lr.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr.SetPosition(1, this.gameObject.transform.GetChild(0).gameObject.transform.position);

                lr2.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr2.SetPosition(1, this.gameObject.transform.GetChild(9).gameObject.transform.position);
            }
            else if(i == 17)
            {
                lr.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr.SetPosition(1, this.gameObject.transform.GetChild(0).gameObject.transform.position);

                lr2.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr2.SetPosition(1, this.gameObject.transform.GetChild(13).gameObject.transform.position);
            }
            else
            {
                lr.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr.SetPosition(1, this.gameObject.transform.GetChild(i-1).gameObject.transform.position);
            }
            lines.Add(go);
            lines.Add(go2);
        }

        for (int i = 22; i < 42; i++)
        {
            var go = new GameObject();
            var go2 = new GameObject();
            var lr = go.AddComponent<LineRenderer>();
            var lr2 = go2.AddComponent<LineRenderer>();
            if (i == 5 + 21)
            {
                lr.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr.SetPosition(1, this.gameObject.transform.GetChild(0+21).gameObject.transform.position);

                lr2.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr2.SetPosition(1, this.gameObject.transform.GetChild(1 + 21).gameObject.transform.position);

            }
            else if (i == 9 + 21)
            {
                lr.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr.SetPosition(1, this.gameObject.transform.GetChild(0 + 21).gameObject.transform.position);

                lr2.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr2.SetPosition(1, this.gameObject.transform.GetChild(5 + 21).gameObject.transform.position);
            }
            else if (i == 13 + 21)
            {
                lr.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr.SetPosition(1, this.gameObject.transform.GetChild(0 + 21).gameObject.transform.position);

                lr2.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr2.SetPosition(1, this.gameObject.transform.GetChild(9 + 21).gameObject.transform.position);
            }
            else if (i == 17 + 21)
            {
                lr.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr.SetPosition(1, this.gameObject.transform.GetChild(0 + 21).gameObject.transform.position);

                lr2.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr2.SetPosition(1, this.gameObject.transform.GetChild(13 + 21).gameObject.transform.position);
            }
            else
            {
                lr.SetPosition(0, this.gameObject.transform.GetChild(i).gameObject.transform.position);
                lr.SetPosition(1, this.gameObject.transform.GetChild(i - 1).gameObject.transform.position);
            }
            lines.Add(go);
            lines.Add(go2);
        }
    }
}
