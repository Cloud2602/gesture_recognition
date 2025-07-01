using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI modalitaText;
    public GameObject suggerimentiPanel;           // Vertical Layout Group
    public GameObject legendaItemPrefab;           // Prefab con Icon + Label

    [Header("Sprites per le modalità")]
    public Sprite zoomSprite;
    public Sprite rotateSprite;
    public Sprite translateSprite;
    public Sprite handsOpenSprite;

    private string modalitaCorrente = "idle";

    public void OnStartPremuto()
    {
        modalitaCorrente = "waiting";
        AggiornaUI();
    }

    public void OnDueManiRilevate()
    {
        modalitaCorrente = "choose_mode";
        AggiornaUI();
    }

    public void OnModalitaSelezionata(string nuovaModalita)
    {
        switch (nuovaModalita)
        {
            case "mode_zoom":
                modalitaCorrente = "zoom";
                break;
            case "mode_rotate":
                modalitaCorrente = "rotate";
                break;
            case "mode_translate":
                modalitaCorrente = "translate";
                break;
            case "default":
                modalitaCorrente = "waiting";
                break;
            default:
                modalitaCorrente = "idle";
                break;
        }

        AggiornaUI();
    }

    private void AggiornaUI()
    {
        // Cancella contenuto precedente
        foreach (Transform child in suggerimentiPanel.transform)
            Destroy(child.gameObject);

        // Visualizza modalità attuale
        switch (modalitaCorrente)
        {
            case "idle":
                modalitaText.text = "";
                suggerimentiPanel.SetActive(false);
                break;

            case "waiting":
                modalitaText.text = "Mode: Waiting...";
                suggerimentiPanel.SetActive(true);
                CreaMessaggioConIcona("Show two hands for choice mode", handsOpenSprite);
                break;

            case "choose_mode":
                modalitaText.text = "Mode: Choice Mode";
                suggerimentiPanel.SetActive(true);
                CreaLegenda("Zoom", zoomSprite);
                CreaLegenda("Rotation", rotateSprite);
                CreaLegenda("Translation", translateSprite);
                break;

            case "zoom":
                modalitaText.text = "Mode: Zoom";
                suggerimentiPanel.SetActive(true);
                CreaMessaggioConIcona("Show two hands for choice mode", handsOpenSprite);
                break;

            case "rotate":
                modalitaText.text = "Mode: Rotation";
                suggerimentiPanel.SetActive(true);
                CreaMessaggioConIcona("Show two hands for choice mode", handsOpenSprite);
                break;

            case "translate":
                modalitaText.text = "Mode: Translation";
                suggerimentiPanel.SetActive(true);
                CreaMessaggioConIcona("Show two hands for choice mode", handsOpenSprite);
                break;
        }
    }

    private void CreaLegenda(string label, Sprite icon)
    {
        GameObject item = Instantiate(legendaItemPrefab, suggerimentiPanel.transform);

        var iconImage = item.transform.Find("Icon").GetComponent<Image>();
        var labelText = item.transform.Find("Label").GetComponent<TextMeshProUGUI>();

        if (iconImage != null) iconImage.sprite = icon;
        if (labelText != null) labelText.text = label;
    }

    

    private void CreaMessaggioConIcona(string msg, Sprite icon)
    {
        GameObject item = Instantiate(legendaItemPrefab, suggerimentiPanel.transform);

        var iconImage = item.transform.Find("Icon").GetComponent<Image>();
        var labelText = item.transform.Find("Label").GetComponent<TextMeshProUGUI>();

        if (iconImage != null) iconImage.sprite = icon;
        if (labelText != null) labelText.text = msg;
    }
}
