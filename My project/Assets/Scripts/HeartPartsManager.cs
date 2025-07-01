using UnityEngine;
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
    }
}
