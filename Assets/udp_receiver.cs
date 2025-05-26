
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
                case "zoom":
                    ZoomModel();
                    break;
                case "rotate":
                    RotateModel();
                    break;
                case "translate":
                    TranslateModel();
                    break;
                case "stop":
                    StopMovement();
                    break;
            }

            lastReceivedData = "";
        }
    }

    void ZoomModel()
    {
        Debug.Log("Zoom...");
        // Aggiungi qui codice per zoomare il modello
    }

    void RotateModel()
    {
        Debug.Log("Rotate...");
        // Aggiungi qui codice per ruotare il modello
    }

    void TranslateModel()
    {
        Debug.Log("Translate...");
        // Aggiungi qui codice per traslare il modello
    }

    void StopMovement()
    {
        Debug.Log("Stop...");
        // Aggiungi qui codice per fermare il movimento
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
    }
}
