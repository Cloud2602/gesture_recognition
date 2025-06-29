using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;

public class UDPReceiver : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int port = 5065;
    string lastReceivedData = "";

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;

    public TextMeshPro modeText;
    private string currentMode = "-";

    void Start()
    {
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        if (modeText != null)
            modeText.text = "Modalità: -";
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
                Debug.Log("[Unity] Ricevuto: " + lastReceivedData);
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
            if (lastReceivedData.StartsWith("mode_"))
            {
                string label = GetModeLabel(lastReceivedData);
                UpdateModeLabel(label);
            }
            else
            {
                switch (lastReceivedData)
                {
                    case "zoom_in":
                        ZoomModel(1); break;
                    case "zoom_out":
                        ZoomModel(-1); break;

                    case "translate_right":
                        TranslateModel(Vector3.back); break;
                    case "translate_left":
                        TranslateModel(Vector3.forward); break;
                    case "translate_up":
                        TranslateModel(Vector3.up); break;
                    case "translate_down":
                        TranslateModel(Vector3.down); break;

                    case "rotate_right":
                        RotateModel(1); break;
                    case "rotate_left":
                        RotateModel(-1); break;
                    case "rotate_up":
                        RotateModelUpDown(-1); break;
                    case "rotate_down":
                        RotateModelUpDown(1); break;

                    case "stop":
                        StopMovement(); break;
                }
            }

            lastReceivedData = "";
        }
    }

    string GetModeLabel(string modeCommand)
    {
        switch (modeCommand)
        {
            case "mode_zoom":
                return "Zoom";
            case "mode_translate":
                return "Traslazione";
            case "mode_rotate":
                return "Rotazione";
            default:
                return currentMode;
        }
    }

    void UpdateModeLabel(string label)
    {
        if (modeText != null && !string.IsNullOrEmpty(label))
        {
            currentMode = label;
            modeText.text = "Modalità: " + currentMode;
            Debug.Log("Modalità aggiornata: " + currentMode);
        }
    }

    void ZoomModel(int direction)
    {
        Debug.Log("Zoom " + (direction > 0 ? "In" : "Out"));
        transform.position += transform.TransformDirection(Vector3.right) * 1f * direction;
    }

    void TranslateModel(Vector3 dir)
    {
        Debug.Log("Translate " + dir);
        transform.position += transform.TransformDirection(dir) * 1f;
    }

    void RotateModel(int direction)
    {
        Debug.Log("Rotate Y (Local) " + (direction > 0 ? "Right" : "Left"));
        transform.Rotate(Vector3.up, 15f * direction, Space.Self);
    }

    void RotateModelUpDown(int direction)
    {
        Debug.Log("Rotate X (Local) " + (direction > 0 ? "Down" : "Up"));
        transform.Rotate(Vector3.right, 15f * direction, Space.Self);
    }

    void StopMovement()
    {
        Debug.Log("Resetting model to default position and rotation");
        transform.position = defaultPosition;
        transform.rotation = defaultRotation;
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
    }
}
