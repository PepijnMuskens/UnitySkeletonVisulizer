using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Unity.VisualScripting;

public class cammove : MonoBehaviour
{
    public float sensitivity;
    public float slowSpeed;
    public float normalSpeed;
    public float sprintSpeed;
    public float rotationSpeed;
    float currentSpeed;

    private float rotation;
    private bool rotate;
    string ConfigFile = "/Config/config.yml";

    void Start()
    {
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
                float speed = config["camera"]["speed"].ConvertTo<float>() /10;
                slowSpeed = slowSpeed * speed;
                normalSpeed = normalSpeed * speed;
                sprintSpeed = sprintSpeed * speed;

            }
        }
        catch
        {

        }
    }
    void Update()
    {
        if (Input.GetMouseButton(1)) //if we are holding right click
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Movement();
            Rotation();
        }else if (Input.GetKeyDown(KeyCode.R))
        {
            rotate = !rotate;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        if (rotate)
        {
            Circle();
        }
    }

    public void Rotation()
    {
        Vector3 mouseInput = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        transform.Rotate(mouseInput * sensitivity);
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
    }

    public void Circle()
    {
        rotation += rotationSpeed * Time.deltaTime;
        this.transform.parent.transform.rotation = Quaternion.Euler(0, rotation, 0);

    }

    public void Movement()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftAlt))
        {
            currentSpeed = slowSpeed;
        }
        else
        {
            currentSpeed = normalSpeed;
        }
        transform.Translate(input * currentSpeed * Time.deltaTime);
    }

}
