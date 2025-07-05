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

        if (cameraZoom != null)
        cameraZoom.OnZoomOutComplete += HandleZoomOutComplete;

        
        if (GesturePanel != null)
            GesturePanel.SetActive(false);

        if (HeartPartsPanel != null)
            HeartPartsPanel.SetActive(false);
    }

    public void onHomePressed()
    {
        
        if (uiGroupToHide != null)
            uiGroupToHide.SetActive(true);

        
        if (GesturePanel != null)
            GesturePanel.SetActive(false);

        if (HeartPartsPanel != null)
            HeartPartsPanel.SetActive(false);

        cameraZoom.StartZoomOut();
        OnApplicationQuit() ;
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
        string scriptPath = Application.dataPath + "/Scripts/External/gesture.py"; 

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
        
        cameraZoom.StartZoomIn();

        
        yield return new WaitForSeconds(1.5f); 

        
        if (uiGroupToHide != null)
            uiGroupToHide.SetActive(false);

        
        if (heart != null)
        {
            HeartRotator rot = heart.GetComponent<HeartRotator>();
            if (rot != null)
                rot.enabled = false;
        }

        
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
                UnityEngine.Debug.Log("Python process closed successfully.");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("Error during the closing of the python process: " + e.Message);
            }
        }
    }

    public void HandleZoomOutComplete()
    {
        
        if (heart != null)
        {
            var rot = heart.GetComponent<HeartRotator>();
            if (rot != null)
                rot.enabled = true;
        }

        hasStarted = false;
    }
}
