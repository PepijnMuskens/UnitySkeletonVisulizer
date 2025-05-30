using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;

public class Listener : MonoBehaviour
{
    Thread thread;
    public int connectionPort = 25001;
    public int jointsperskeleton;
    TcpListener server;
    TcpClient client;
    bool running;
    List<GameObject> lines = new List<GameObject>();
    public GameObject JointPrefab;
    public List<int[]> connectionsExeptions;
    List<GameObject> skeletons;
    List<Vector4[]> skeletondatas;
    // Position is the data being received in this example
 

    // Start is called before the first frame update
    void Start()
    {
        skeletondatas = new List<Vector4[]>();
        //all joints connnect by default to the joint index before them these joints are exeptions
        connectionsExeptions = new List<int[]>();
        /*connectionsExeptions.Add(new int[] { 5, 0 });
        connectionsExeptions.Add(new int[] { 5, 1 });
        connectionsExeptions.Add(new int[] { 9, 0 });
        connectionsExeptions.Add(new int[] { 9, 5 });
        connectionsExeptions.Add(new int[] { 13, 0});
        connectionsExeptions.Add(new int[] { 13, 9 });
        connectionsExeptions.Add(new int[] { 17, 0 });
        connectionsExeptions.Add(new int[] { 17, 13 });

        connectionsExeptions.Add(new int[] { 21, 21 });
        connectionsExeptions.Add(new int[] { 5 + 21, 0 + 21 });
        connectionsExeptions.Add(new int[] { 5 + 21, 1 + 21 });
        connectionsExeptions.Add(new int[] { 9 + 21, 0 + 21 });
        connectionsExeptions.Add(new int[] { 9 + 21, 5 + 21 });
        connectionsExeptions.Add(new int[] { 13 + 21, 0 + 21 });
        connectionsExeptions.Add(new int[] { 13 + 21, 9 + 21 });
        connectionsExeptions.Add(new int[] { 17 + 21, 0 + 21 });
        connectionsExeptions.Add(new int[] { 17 + 21, 13 + 21 });*/

        connectionsExeptions.Add(new int[] { 0, 0 });
        connectionsExeptions.Add(new int[] { 4, 0 });
        connectionsExeptions.Add(new int[] { 7, 3 });
        connectionsExeptions.Add(new int[] { 8, 6 });
        connectionsExeptions.Add(new int[] { 9, 9 });
        connectionsExeptions.Add(new int[] { 11, 11 });
        connectionsExeptions.Add(new int[] { 13, 11 });
        connectionsExeptions.Add(new int[] { 14, 12 });
        connectionsExeptions.Add(new int[] { 15, 13 });
        connectionsExeptions.Add(new int[] { 16, 14 });
        connectionsExeptions.Add(new int[] { 17, 15 });
        connectionsExeptions.Add(new int[] { 18, 16 });
        connectionsExeptions.Add(new int[] { 19, 15 });
        connectionsExeptions.Add(new int[] { 20, 16 });
        connectionsExeptions.Add(new int[] { 21, 15 });
        connectionsExeptions.Add(new int[] { 22, 16 });
        connectionsExeptions.Add(new int[] { 23, 11 });
        connectionsExeptions.Add(new int[] { 24, 12 });
        connectionsExeptions.Add(new int[] { 24, 23 });
        connectionsExeptions.Add(new int[] { 25, 23 });
        connectionsExeptions.Add(new int[] { 26, 24 });
        connectionsExeptions.Add(new int[] { 28, 26 });
        connectionsExeptions.Add(new int[] { 27, 25 });
        connectionsExeptions.Add(new int[] { 29, 27 });
        connectionsExeptions.Add(new int[] { 30, 28 });
        connectionsExeptions.Add(new int[] { 31, 27 });
        connectionsExeptions.Add(new int[] { 32, 28 });
        connectionsExeptions.Add(new int[] { 31, 29 });
        connectionsExeptions.Add(new int[] { 32, 30 });

        skeletons = new List<GameObject>();
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
            try
            {
                if (!Connection())
                {
                    //reconnect
                    server.Stop();
                    client.Dispose();
                    server = new TcpListener(IPAddress.Any, connectionPort);
                    server.Start();

                    // Create a client to get the data stream
                    client = server.AcceptTcpClient();

                }


            }
            catch(Exception e)
            {
                if(e.GetType() != typeof(ThreadAbortException))
                {
                    Console.WriteLine("O no, Anyway");
                }
            }
        }
        server.Stop();
    }
    // Update is called once per frame
    bool Connection()
    {
        // Read data from the network stream
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

        // Decode the bytes into a string
        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        nwStream.Write(buffer, 0, bytesRead);
        // Make sure we're not getting an empty string
        //dataReceived.Trim();
        if (dataReceived != null && dataReceived != "")
        {
            // Convert the received string of data to the format we are using
            skeletondatas = ParseData(dataReceived);
            return true;
        }
        else
        {
            Console.WriteLine("test");
            return false;
        }
    }

    // Use-case specific function, need to re-write this to interpret whatever data is being sent
    public static List<Vector4[]> ParseData(string dataString)
    {
       
        Debug.Log(dataString);
        // Remove the parentheses
        if (dataString.StartsWith("(") && dataString.EndsWith(")"))
        {
            dataString = dataString.Substring(1, dataString.Length - 2);
        }

        string[] skeletalsplit = dataString.Split('$');
        List<Vector4[]> vector4s = new List<Vector4[]>();
        for(int s = 0; s < skeletalsplit.Length; s++)
        {
            // Split the elements into an array
            string[] stringArray = skeletalsplit[s].Split('|');
            Vector4[] cords = new Vector4[stringArray.Length];
            for (int i = 0; i < stringArray.Length; i++)
            {
                string[] pos = stringArray[i].Split(',');
                // Store as a Vector3
                Vector4 result = new Vector4(
                    float.Parse(pos[0], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pos[1], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pos[2], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(pos[3], CultureInfo.InvariantCulture.NumberFormat));
                cords[i] = result;
            }
            vector4s.Add(cords);
        }

        return vector4s;
    }

    void Update()
    {
        for(int i = skeletons.Count; i < skeletondatas.Count; i++)
        {
            skeletons.Add(new GameObject("Skeleton"));
            for (int j = 0; j < jointsperskeleton; j++)
            {
                Instantiate(JointPrefab, skeletons[i].transform);
            }
        }
        for (int s = 0; s < skeletondatas.Count; s++)
        {
            Vector4[] positions = skeletondatas[s];
            for (int i = 0; i < positions.Length; i++)
            {
                skeletons[s].gameObject.transform.GetChild((int)positions[i][0]).gameObject.transform.position = new Vector3(positions[i][1], positions[i][2], positions[i][3]);
            }
        }
        for (int s = 0; s < skeletons.Count; s++)
        {
            if(s > skeletondatas.Count - 1)
            {
                skeletons[s].SetActive(false);
            }
            else
            {
                skeletons[s].SetActive(true);
            }
        }
        drawLines();
    }
    void drawLines()
    {
        foreach(GameObject g in lines)
        {
            Destroy(g);
        }
        lines.Clear();
        for (int s = 0; s < skeletons.Count; s++)
        {
            if (s > skeletondatas.Count - 1)
            {

            }
            else
            {
                Vector3 zero = new Vector3(0, 0, 0);
                for (int i = 1; i < jointsperskeleton; i++)
                {
                    if (skeletons[s].gameObject.transform.GetChild(i).gameObject.transform.position == zero || skeletons[s].gameObject.transform.GetChild(i-1).gameObject.transform.position == zero)
                    {
                        continue;
                    }
                    List<int[]> cons = connectionsExeptions.FindAll(x => x[0] == i);
                    if (cons.Count == 0)
                    {
                        var go = new GameObject();
                        var lr = go.AddComponent<LineRenderer>();
                        lr.SetPosition(0, skeletons[s].gameObject.transform.GetChild(i).gameObject.transform.position);
                        lr.SetPosition(1, skeletons[s].gameObject.transform.GetChild(i - 1).gameObject.transform.position);
                        lr.startWidth = 2;
                        lr.endWidth = 2;
                        lines.Add(go);
                    }
                    else
                    {
                        foreach (int[] con in cons)
                        {
                            var go = new GameObject();
                            var lr = go.AddComponent<LineRenderer>();
                            lr.SetPosition(0, skeletons[s].gameObject.transform.GetChild(i).gameObject.transform.position);
                            lr.SetPosition(1, skeletons[s].gameObject.transform.GetChild(con[1]).gameObject.transform.position);
                            lr.startWidth = 2;
                            lr.endWidth = 2;
                            lines.Add(go);
                        }
                    }
                }
            }
        }
        
    }
}
