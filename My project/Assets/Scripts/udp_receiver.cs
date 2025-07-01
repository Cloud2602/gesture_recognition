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

    void Start()
    {
        //  Memorizza la posizione iniziale all'avvio
        initialPosition = transform.position;

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
            Debug.Log("Gestione UI per: " + lastReceivedData);
            // GESTIONE UI
            switch (lastReceivedData)
            
            {

                case "start":
                case "default":
                    if (uiManager != null) uiManager.OnStartPremuto();
                    break;

                case "choose_mode":
                    Debug.Log("Modalità scelta: " + lastReceivedData);
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
        Debug.Log("Rotate Y (Local) " + (direction > 0 ? "Right" : "Left"));
        transform.Rotate(Vector3.up, 5f * direction, Space.Self);
    }

    void RotateModelUpDown(int direction)
    {
        Debug.Log("Rotate X (Local) " + (direction > 0 ? "Down" : "Up"));
        transform.Rotate(Vector3.right, 5f * direction, Space.Self);
    }

    void StopMovement()
    {
        Debug.Log("Stop movement");
        // Placeholder
    }

    //  Funzione per tornare alla posizione iniziale
    void ResetToInitialPosition()
    {
        Debug.Log("Reset alla posizione iniziale");
        transform.position = initialPosition;
        transform.rotation = Quaternion.identity;  // opzionale: resetta anche rotazione
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
