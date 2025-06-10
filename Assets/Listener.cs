using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Globalization;
using YamlDotNet.Serialization;
using System.IO;
using YamlDotNet.RepresentationModel;
using Unity.VisualScripting;
using YamlDotNet.Serialization.NamingConventions;


public class Listener : MonoBehaviour
{
    Thread thread;
    public int connectionPort = 25001;
    public int jointsperskeleton;
    TcpListener server;
    TcpClient client;
    bool running;
    public Material material;
    List<GameObject> lines = new List<GameObject>();
    public GameObject JointPrefab;
    public GameObject JointPrefab_Predict;
    public GameObject Floor;
    public List<int[]> connectionsExeptions;
    public List<int[]> jointConnections;
    List<GameObject> skeletons;
    List<Vector4[]> skeletondatas;
    List<Color> skeletoncolors;
    // Position is the data being received in this example

    string ConfigFile = "/Config/config.yml";


    // Start is called before the first frame update
    void Start()
    {
        skeletoncolors = new List<Color>();
        try
        {
            string filePath = Directory.GetCurrentDirectory() + ConfigFile;
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            using (var reader = new StreamReader(filePath))
            {
                // Load the stream
                var yaml = new YamlStream();
                yaml.Load(reader);

                YamlNode config = yaml.Documents[0].RootNode;// the rest
                Console.WriteLine(config);
                YamlNode joints = config["skeleton_data"]["joint_connections"];
                jointConnections = deserializer.Deserialize<List<int[]>>(joints.ToString());

                YamlNode colors = config["skeleton_data"]["colors"];
                foreach(int[] col in deserializer.Deserialize<List<int[]>>(colors.ToString()))
                {
                    skeletoncolors.Add(new Color(col[0], col[1], col[2]));
                }
                int floory = deserializer.Deserialize<int>(config["floor"]["y"].ToString());
                Floor.transform.position = new Vector3(0, floory,0);

            }
        }
        catch
        {

        }
        skeletondatas = new List<Vector4[]>();
        //all joints connnect by default to the joint index before them these joints are exeptions


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
                if(i< 1)
                {
                    Instantiate(JointPrefab, skeletons[i].transform);
                }
                else
                {
                    Instantiate(JointPrefab_Predict, skeletons[i].transform);
                }
                
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
                    List<int[]> cons = jointConnections.FindAll(x => x[0] == i);
                    if (cons.Count == 0)
                    {
                    }
                    else
                    {
                        foreach (int[] con in cons)
                        {
                            var go = new GameObject();
                            var lr = go.AddComponent<LineRenderer>();
                            if (s < skeletoncolors.Count)
                            {
                                lr.material.color = skeletoncolors[s];
                            }
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
