using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeartPartsManager : MonoBehaviour
{
    [Header("Riferimenti alle parti del cuore")]
    public GameObject aorta;
    public GameObject leftAtrium;
    public GameObject leftVentricle;
    public GameObject pulmonaryArtery;
    public GameObject rightAtrium;
    public GameObject rightVentricle;
    public GameObject venaCava;

    [Header("Toggle UI")]
    public Toggle toggleAorta;
    public Toggle toggleLeftAtrium;
    public Toggle toggleLeftVentricle;
    public Toggle togglePulmonaryArtery;
    public Toggle toggleRightAtrium;
    public Toggle toggleRightVentricle;
    public Toggle toggleVenaCava;

    [Header("UI Volume")]
    public GameObject volumePanel;
    public TextMeshProUGUI volumeLabel;

    public void OnStartPressed()
{
    // Altri toggle → visibilità
    toggleAorta.onValueChanged.AddListener((val) => aorta.SetActive(val));
    toggleLeftAtrium.onValueChanged.AddListener((val) => leftAtrium.SetActive(val));
    toggleLeftVentricle.onValueChanged.AddListener((val) => leftVentricle.SetActive(val));
    togglePulmonaryArtery.onValueChanged.AddListener((val) => pulmonaryArtery.SetActive(val));
    toggleRightAtrium.onValueChanged.AddListener((val) => rightAtrium.SetActive(val));
    toggleRightVentricle.onValueChanged.AddListener((val) => rightVentricle.SetActive(val));
    toggleVenaCava.onValueChanged.AddListener((val) => venaCava.SetActive(val));

    // Solo le camere coinvolte aggiornano il volume
    toggleLeftAtrium.onValueChanged.AddListener((val) => OnToggleChanged());
    toggleLeftVentricle.onValueChanged.AddListener((val) => OnToggleChanged());
    toggleRightAtrium.onValueChanged.AddListener((val) => OnToggleChanged());
    toggleRightVentricle.onValueChanged.AddListener((val) => OnToggleChanged());
    toggleAorta.onValueChanged.AddListener((val) => OnToggleChanged());
    togglePulmonaryArtery.onValueChanged.AddListener((val) => OnToggleChanged());
    toggleVenaCava.onValueChanged.AddListener((val) => OnToggleChanged());


    OnToggleChanged();
}
   void OnToggleChanged()
    {
        int cameraCount = 0;
        string selected = "";

        if (toggleRightAtrium.isOn) { cameraCount++; selected = "Right Atrium"; }
        if (toggleRightVentricle.isOn) { cameraCount++; selected = "Right Ventricle"; }
        if (toggleLeftAtrium.isOn) { cameraCount++; selected = "Left Atrium"; }
        if (toggleLeftVentricle.isOn) { cameraCount++; selected = "Left Ventricle"; }

        bool altriAttivi =
            toggleAorta.isOn ||
            togglePulmonaryArtery.isOn ||
            toggleVenaCava.isOn;

        // Mostra volume SOLO se una camera è attiva e gli altri sono tutti disattivati
        if (cameraCount == 1 && !altriAttivi)
        {
            volumePanel.SetActive(true);
            volumeLabel.text = $"Volume: {GetVolume(selected)} ml";
        }
        else
        {
            volumePanel.SetActive(false);
        }
    }



    int GetVolume(string parte)
    {
        // Valori di esempio, puoi personalizzarli
        switch (parte)
        {
            case "Right Atrium": return 52;
            case "Right Ventricle": return 151;
            case "Left Atrium": return 71;
            case "Left Ventricle": return 149;
            default: return 0;
        }
    }
}
