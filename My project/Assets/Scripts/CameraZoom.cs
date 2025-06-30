using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public Transform zoomTarget;       // Oggetto verso cui zoommare (es. il cuore o sfera)
    public float zoomSpeed = 1.5f;     // Velocità dello zoom
    public float targetFOV = 30f;      // Field of View finale della camera
    public CanvasGroup uiGroup;        // Riferimento al CanvasGroup per il fade UI

    private Camera cam;
    private bool zooming = false;

    void Start()
    {
        cam = Camera.main;
    }

    public void StartZoom()
    {
        zooming = true;
    }

   void Update()
{
    if (zooming)
    {
        // Posizione target finale a z = 2, mantenendo x e y attuali
        Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, 2f);

        // Muovi gradualmente la camera verso z = 2
        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.deltaTime * zoomSpeed);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);

        // Calcola il valore normalizzato tra -7.5 e 2 per fading
        float t = Mathf.InverseLerp(-7.5f, 2f, cam.transform.position.z); // da -7.5 → 2 = 0 → 1

        // Fade-out della UI
        if (uiGroup != null)
            uiGroup.alpha = 1f - t;

        // Ferma lo zoom appena ci avviciniamo a z = 2
        if (cam.transform.position.z >= 1.95f)
        {
            cam.transform.position = targetPos;
            cam.fieldOfView = targetFOV;
            zooming = false;
        }
    }
}





}
