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

    void Start()
    {
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

            lastReceivedData = "";
        }
    }

    void ZoomModel(int direction)
    {
        Debug.Log("Zoom " + (direction > 0 ? "In" : "Out"));
        // Muove in avanti (Z negativo) o indietro (Z positivo) di 20 unità
        transform.position += Vector3.forward * 0.2f * direction;
    }

    void TranslateModel(Vector3 dir)
    {
        Debug.Log("Translate " + dir);
        transform.position += dir * 0.2f; // 0.2f = ~20 pixel world space (dipende dalla scala)
    }

    void RotateModel(int direction)
{
    Debug.Log("Rotate Y (Local) " + (direction > 0 ? "Right" : "Left"));
    transform.Rotate(Vector3.up, 5f * direction, Space.Self); // RUOTA SU SE STESSO
}

void RotateModelUpDown(int direction)
{
    Debug.Log("Rotate X (Local) " + (direction > 0 ? "Down" : "Up"));
    transform.Rotate(Vector3.right, 5f * direction, Space.Self); // RUOTA SU SE STESSO
}


    void StopMovement()
    {
        Debug.Log("Stop movement");
        // Placeholder per logica futura
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
    }
}
