using UnityEngine;
using System.Collections;
using System.Diagnostics;
public class MenuController : MonoBehaviour
{
    public CameraZoom cameraZoom;
    public GameObject uiGroupToHide;
    public GameObject heart;
    public GameObject GesturePanel; 
    public GameObject HeartPartsPanel;

    private bool hasStarted = false;
    private Process pythonProcess; 
    private void Start()
    {
        // Nascondi instruction panel all'avvio
        if (GesturePanel != null)
            GesturePanel.SetActive(false);

        if (HeartPartsPanel != null)
            HeartPartsPanel.SetActive(false);
    }

    public void OnStartPressed()
    {
        if (!hasStarted)
        {
            hasStarted = true;

            
            StartPythonScript();

            StartCoroutine(StartSequence());
        }
    }

    private void StartPythonScript()
{
    string pythonExePath = "C:/Users/franc/anaconda3/envs/Soluzione_ddd/python.exe"; 
    string scriptPath = Application.dataPath + "/Scripts/External/gesture_real_sense.py"; 

    UnityEngine.Debug.Log($"🟢 [StartPythonScript] Interprete Python: {pythonExePath}");
    UnityEngine.Debug.Log($"🟢 [StartPythonScript] Script Python: {scriptPath}");

    ProcessStartInfo start = new ProcessStartInfo();
    start.FileName = pythonExePath;
    start.Arguments = "\"" + scriptPath + "\"";
    start.UseShellExecute = false;
    start.RedirectStandardOutput = true;
    start.RedirectStandardError = true;
    start.CreateNoWindow = true;

    Process process = new Process();
    process.StartInfo = start;

    process.OutputDataReceived += (sender, args) =>
    {
        if (!string.IsNullOrEmpty(args.Data))
            UnityEngine.Debug.Log("📤 PYTHON OUTPUT: " + args.Data);
    };

    process.ErrorDataReceived += (sender, args) =>
    {
        if (!string.IsNullOrEmpty(args.Data))
            UnityEngine.Debug.LogError("❌ PYTHON ERROR: " + args.Data);
    };

    pythonProcess = new Process(); 
    pythonProcess.StartInfo = start;

    try
    {
        UnityEngine.Debug.Log("🚀 Avvio dello script Python...");

        pythonProcess.Start();
        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine();

        UnityEngine.Debug.Log("✅ Python script avviato con successo.");
    }
    catch (System.Exception e)
    {
        UnityEngine.Debug.LogError("❗ Errore avvio Python: " + e.Message);
    }
}


    private IEnumerator StartSequence()
    {
        // 1. Avvia zoom
        cameraZoom.StartZoom();

        // 2. Attendi che lo zoom finisca
        yield return new WaitForSeconds(1.5f); 

        // 3. Nascondi UI menu
        if (uiGroupToHide != null)
            uiGroupToHide.SetActive(false);

        // 4. Ferma rotazione cuore
        if (heart != null)
        {
            HeartRotator rot = heart.GetComponent<HeartRotator>();
            if (rot != null)
                rot.enabled = false;
        }

        // 5. Mostra instruction panel
        if (GesturePanel != null)
            GesturePanel.SetActive(true);

        if (HeartPartsPanel != null)
            HeartPartsPanel.SetActive(true);
    }

    

    public void OnQuitPressed()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void OnApplicationQuit() 
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            try
            {
                pythonProcess.Kill();
                pythonProcess.Dispose();
                UnityEngine.Debug.Log("Processo Python terminato.");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("Errore durante la chiusura del processo Python: " + e.Message);
            }
        }
    }
}
