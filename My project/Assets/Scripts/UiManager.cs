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
    public Sprite pinchSprite;
    public Sprite dragSprite;

    private string modalitaCorrente = "idle";

    public void OnStartPremuto()
    {
        modalitaCorrente = "waiting";
        AggiornaUI();
    }

    public void OnDueManiRilevate()
    {
        Debug.Log("Due mani rilevate, cambio modalità a 'choose_mode'");
        modalitaCorrente = "choose_mode";
        AggiornaUI();
    }

    public void OnModalitaSelezionata(string nuovaModalita)
    {
        Debug.Log($"Modalità selezionata: {nuovaModalita}");
        switch (nuovaModalita)
        {
            case "mode_zoom":
                modalitaCorrente = "zoom";
                AggiornaUI();
                break;
            case "mode_rotate":
                modalitaCorrente = "rotate";
                AggiornaUI();
                break;
            case "mode_translate":
                modalitaCorrente = "translate";
                AggiornaUI();
                break;
        }

        
    }

    private void AggiornaUI()
    {
         // Cancella contenuto precedente
        foreach (Transform child in suggerimentiPanel.transform)
            Destroy(child.gameObject);


        // Visualizza modalità attuale
        switch (modalitaCorrente)
        {
            case "waiting":
                modalitaText.text = "Mode: Waiting...";
                suggerimentiPanel.SetActive(true);
                CreaMessaggioConIcona("Choice mode", handsOpenSprite);
                break;

            case "choose_mode":
                modalitaText.text = "Mode: Choice Mode";
                suggerimentiPanel.SetActive(true);
                CreaLegenda("Zoom", zoomSprite);
                CreaLegenda("Rotation", rotateSprite);
                CreaLegenda("Translation", translateSprite);
                CreaMessaggioConIcona("Reset Heart", handsOpenSprite);
                break;

            case "zoom":
                modalitaText.text = "Mode: Zoom";
                suggerimentiPanel.SetActive(true);
                CreaMessaggioConIcona("Pinch to Zoom", pinchSprite);
                CreaMessaggioConIcona("Choice mode", handsOpenSprite);
                break;

            case "rotate":
                modalitaText.text = "Mode: Rotation";
                suggerimentiPanel.SetActive(true);
                CreaMessaggioConIcona("Move hand to rotate",dragSprite);
                CreaMessaggioConIcona("Choice mode", handsOpenSprite);
                break;

            case "translate":
                modalitaText.text = "Mode: Translation";
                suggerimentiPanel.SetActive(true);
                CreaMessaggioConIcona("Move hand to translate",dragSprite);
                CreaMessaggioConIcona("Choice mode", handsOpenSprite);
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
