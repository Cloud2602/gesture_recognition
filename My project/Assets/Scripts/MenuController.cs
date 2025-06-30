using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour
{
    public CameraZoom cameraZoom;
    public GameObject uiGroupToHide;
    public GameObject heart;
    public GameObject InstructionPanel; 

    private bool hasStarted = false;

    private void Start()
    {
        // Nascondi instruction panel all'avvio
        if (InstructionPanel != null)
            InstructionPanel.SetActive(false);
    }

    public void OnStartPressed()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            StartCoroutine(StartSequence());
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

    public void OnSettingsPressed()
    {
        Debug.Log("Settings aperto (da implementare)");
    }

    public void OnQuitPressed()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
