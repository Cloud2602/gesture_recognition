using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI modalitaText;
    public TextMeshProUGUI suggerimentiText;

    private bool startPremuto = false;
    private bool inSceltaModalita = false;
    private string modalitaCorrente = "Nessuna";

    public void OnStartPremuto()
    {
        startPremuto = true;
        modalitaCorrente = "In attesa...";
        inSceltaModalita = true;
        AggiornaUI();
    }

    public void OnDueManiRilevate()
    {
        inSceltaModalita = true;
        AggiornaUI();
    }

    public void OnModalitaSelezionata(string nuovaModalita)
    {
        modalitaCorrente = nuovaModalita;
        inSceltaModalita = false;
        AggiornaUI();
    }

    void AggiornaUI()
    {
        modalitaText.text = $"Sei in modalità: {modalitaCorrente}";

        if (!startPremuto)
        {
            suggerimentiText.text = "";
        }
        else if (inSceltaModalita && modalitaCorrente == "In attesa...")
        {
            suggerimentiText.text = "👐 Mostra due mani per scegliere la modalità";
        }
        else if (inSceltaModalita)
        {
            suggerimentiText.text = "Scegli la modalità:\n👉 Zoom\n✌️ Rotazione\n🤟 Traslazione";
        }
        else
        {
            suggerimentiText.text = "";
        }
    }
}
