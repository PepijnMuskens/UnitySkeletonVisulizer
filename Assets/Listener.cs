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
    public int jointsperskeleton;
    TcpListener server;
    TcpClient client;
    bool running;
    List<GameObject> lines = new List<GameObject>();
    public GameObject JointPrefab;
    public List<int[]> connectionsExeptions;
    List<GameObject> skeletons;

    // Start is called before the first frame update
    void Start()
    {
        //all joints connnect by default to the joint index before them these joints are exeptions
        connectionsExeptions = new List<int[]>();
        connectionsExeptions.Add(new int[] { 5, 0 });
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
        connectionsExeptions.Add(new int[] { 17 + 21, 13 + 21 });

        connectionsExeptions.Add(new int[] { 0 + 42, 0 + 42 });
        connectionsExeptions.Add(new int[] { 4 + 42, 0 + 42 });
        connectionsExeptions.Add(new int[] { 7 + 42, 3 + 42 });
        connectionsExeptions.Add(new int[] { 8 + 42, 6 + 42 });
        connectionsExeptions.Add(new int[] { 9 + 42, 9 + 42 });
        connectionsExeptions.Add(new int[] { 11 + 42, 11 + 42 });
        connectionsExeptions.Add(new int[] { 13 + 42, 11 + 42 });
        connectionsExeptions.Add(new int[] { 14 + 42, 12 + 42 });
        connectionsExeptions.Add(new int[] { 15 + 42, 13 + 42 });
        connectionsExeptions.Add(new int[] { 16 + 42, 14 + 42 });
        connectionsExeptions.Add(new int[] { 17 + 42, 15 + 42 });
        connectionsExeptions.Add(new int[] { 18 + 42, 16 + 42 });
        connectionsExeptions.Add(new int[] { 19 + 42, 15 + 42 });
        connectionsExeptions.Add(new int[] { 20 + 42, 16 + 42 });
        connectionsExeptions.Add(new int[] { 21 + 42, 15 + 42 });
        connectionsExeptions.Add(new int[] { 22 + 42, 16 + 42 });
        connectionsExeptions.Add(new int[] { 23 + 42, 11 + 42 });
        connectionsExeptions.Add(new int[] { 24 + 42, 12 + 42 });
        connectionsExeptions.Add(new int[] { 24 + 42, 23 + 42 });
        connectionsExeptions.Add(new int[] { 25 + 42, 23 + 42 });
        connectionsExeptions.Add(new int[] { 26 + 42, 24 + 42 });
        connectionsExeptions.Add(new int[] { 28 + 42, 26 + 42 });
        connectionsExeptions.Add(new int[] { 27 + 42, 25 + 42 });
        connectionsExeptions.Add(new int[] { 29 + 42, 27 + 42 });
        connectionsExeptions.Add(new int[] { 30 + 42, 28 + 42 });
        connectionsExeptions.Add(new int[] { 31 + 42, 27 + 42 });
        connectionsExeptions.Add(new int[] { 32 + 42, 28 + 42 });
        connectionsExeptions.Add(new int[] { 31 + 42, 29 + 42 });
        connectionsExeptions.Add(new int[] { 32 + 42, 30 + 42 });

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
        if(skeletons.Count < 1)
        {
            skeletons.Add(new GameObject());
            for(int i = 0; i < jointsperskeleton; i++)
            {
                Instantiate(JointPrefab, skeletons[0].transform);
            }
        }
        for(int i =0; i < positions.Length;i++)
        {
            skeletons[0].gameObject.transform.GetChild((int)positions[i][0]).gameObject.transform.position = new Vector3(positions[i][1], positions[i][2], positions[i][3]);
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
        
        for(int i =1; i < jointsperskeleton; i++)
        {
            List<int[]> cons = connectionsExeptions.FindAll(x => x[0] == i);
            if (cons.Count == 0)
            {
                var go = new GameObject();
                var lr = go.AddComponent<LineRenderer>();
                lr.SetPosition(0, skeletons[0].gameObject.transform.GetChild(i).gameObject.transform.position);
                lr.SetPosition(1, skeletons[0].gameObject.transform.GetChild(i - 1).gameObject.transform.position);
                lr.SetWidth(2,2);
                lines.Add(go);
            }
            else
            {
                foreach(int[] con in cons)
                {
                    var go = new GameObject();
                    var lr = go.AddComponent<LineRenderer>();
                    lr.SetPosition(0, skeletons[0].gameObject.transform.GetChild(i).gameObject.transform.position);
                    lr.SetPosition(1, skeletons[0].gameObject.transform.GetChild(con[1]).gameObject.transform.position);
                    lines.Add(go);
                }
            }
        }
    }
}
