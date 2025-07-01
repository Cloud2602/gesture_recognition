using UnityEngine;
using System.Collections;
using System.Diagnostics;
public class MenuController : MonoBehaviour
{
    public CameraZoom cameraZoom;
    public GameObject uiGroupToHide;
    public GameObject heart;
    public GameObject InstructionPanel; 
    public GameObject HeartPartsPanel;

    private bool hasStarted = false;
    private Process pythonProcess; 
    private void Start()
    {
        // Nascondi instruction panel all'avvio
        if (InstructionPanel != null)
            InstructionPanel.SetActive(false);

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
        string pythonExePath = "C:/Users/franc/anaconda3/envs/technologies_dm/python.exe"; // oppure "python3" o il path completo, es: "C:/Python39/python.exe"
        string scriptPath = Application.dataPath + "/Scripts/External/gesture.py"; // cambia il path se serve

        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = pythonExePath;
        start.Arguments = "\"" + scriptPath + "\"";
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.CreateNoWindow = true;

        Process process = new Process();
        process.StartInfo = start;

        process.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log("PYTHON: " + args.Data);
        process.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError("PYTHON ERROR: " + args.Data);
        pythonProcess = new Process(); 
        pythonProcess.StartInfo = start;
        try
        {
            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Errore avvio Python: " + e.Message);
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
        if (InstructionPanel != null)
            InstructionPanel.SetActive(true);
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
