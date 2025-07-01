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
        // Collega eventi toggle → visibilità
        toggleAorta.onValueChanged.AddListener((val) => aorta.SetActive(val));
        toggleLeftAtrium.onValueChanged.AddListener((val) => leftAtrium.SetActive(val));
        toggleLeftVentricle.onValueChanged.AddListener((val) => leftVentricle.SetActive(val));
        togglePulmonaryArtery.onValueChanged.AddListener((val) => pulmonaryArtery.SetActive(val));
        toggleRightAtrium.onValueChanged.AddListener((val) => rightAtrium.SetActive(val));
        toggleRightVentricle.onValueChanged.AddListener((val) => rightVentricle.SetActive(val));
        toggleVenaCava.onValueChanged.AddListener((val) => venaCava.SetActive(val));

        // Nascondi inizialmente il volume
        volumePanel.SetActive(false);
    }

    void OnToggleChanged()
    {
        // Conta quanti toggle sono attivi
        int count = 0;
        string attiva = "";

        if (toggleRightAtrium.isOn) { count++; attiva = "Right Atrium"; }
        if (toggleRightVentricle.isOn) { count++; attiva = "Right Ventricle"; }
        if (toggleLeftAtrium.isOn) { count++; attiva = "Left Atrium"; }
        if (toggleLeftVentricle.isOn) { count++; attiva = "Left Ventricle"; }

        if (count == 1)
        {
            volumePanel.SetActive(true);
            volumeLabel.text = $"Volume: {GetVolume(attiva)} ml";
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
