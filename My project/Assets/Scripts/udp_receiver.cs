using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPReceiver : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int port = 5065;
    string lastReceivedData = "";
    public UIManager uiManager;

    private Vector3 initialPosition;  
    private Quaternion initialRotation;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);
                byte[] data = client.Receive(ref anyIP);
                lastReceivedData = Encoding.UTF8.GetString(data);
                Debug.Log("[Unity] Received: " + lastReceivedData);
            }
            catch (System.Exception err)
            {
                Debug.LogError(err.ToString());
            }
        }
    }

    void Update()
    {
        
        if (!string.IsNullOrEmpty(lastReceivedData))
        {
            if (lastReceivedData.StartsWith("[MODEL ERROR]"))
            {
                Debug.LogError(lastReceivedData);
                return;
            }
            switch (lastReceivedData)
            {
                case "zoom_in":
                    ZoomModel(1); break;
                case "zoom_out":
                    ZoomModel(-1); break;

                case "translate_right":
                    TranslateModel(Vector3.right); break;
                case "translate_left":
                    TranslateModel(Vector3.left); break;
                case "translate_up":
                    TranslateModel(Vector3.up); break;
                case "translate_down":
                    TranslateModel(Vector3.down); break;

                case "rotate_left":
                    RotateModel(1); break;
                case "rotate_right":
                    RotateModel(-1); break;
                case "rotate_down":
                    RotateModelUpDown(-1); break;
                case "rotate_up":
                    RotateModelUpDown(1); break;

                case "stop":
                    StopMovement(); break;
                
                case "default":
                    ResetToInitialPosition(); break;
                
            }
            Debug.Log("UI handling for: " + lastReceivedData);
            
            switch (lastReceivedData)
            {
                case "start":
                case "default":
                    if (uiManager != null) uiManager.OnStartPremuto();
                    break;

                case "choose_mode":
                    Debug.Log("Chosen mode: " + lastReceivedData);
                    if (uiManager != null) uiManager.OnDueManiRilevate();
                    break;

                case "mode_zoom":
                case "mode_rotate":
                case "mode_translate":
                    if (uiManager != null) uiManager.OnModalitaSelezionata(lastReceivedData);
                    break;
            }

            lastReceivedData = "";
        }
    }

    void ZoomModel(int direction)
    {
        Debug.Log("Zoom " + (direction > 0 ? "In" : "Out"));
        transform.position += Vector3.forward * 0.2f * direction;
    }

    void TranslateModel(Vector3 dir)
    {
        Debug.Log("Translate " + dir);
        transform.position += dir * 0.2f;
    }

    void RotateModel(int direction)
    {
        Vector3 initialUp = initialRotation * Vector3.up;  
        transform.Rotate(initialUp, 5f * direction, Space.World);
    }

    void RotateModelUpDown(int direction)
    {
        Vector3 initialRight = initialRotation * Vector3.right;  
        transform.Rotate(initialRight, 5f * direction, Space.World);
    }

    void StopMovement()
    {
        Debug.Log("Stop movement");
    }

    void ResetToInitialPosition()
    {
        Debug.Log("Reset to inital position");
        transform.position = initialPosition;
        transform.rotation = Quaternion.identity;  
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();

        using (UdpClient quitSender = new UdpClient())
        {
            byte[] quitMsg = Encoding.UTF8.GetBytes("quit");
            quitSender.Send(quitMsg, quitMsg.Length, "127.0.0.1", 5065);
        }
    }
}
