using UnityEngine;
using UnityEngine.UI;

public class WebcamDisplay : MonoBehaviour
{
    public RawImage rawImage;
    private WebCamTexture webcamTexture;

    void Start()
{
    if (rawImage == null)
    {
        rawImage = GetComponent<RawImage>();
    }

    if (WebCamTexture.devices.Length == 0)
    {
        Debug.LogError("Nessuna webcam trovata.");
        return;
    }

    webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name);
    rawImage.texture = webcamTexture;
    rawImage.material.mainTexture = webcamTexture;
    webcamTexture.Play();
}


    void Update()
    {
        if (webcamTexture.isPlaying)
        {
            rawImage.texture = webcamTexture;
        }
    }
}
