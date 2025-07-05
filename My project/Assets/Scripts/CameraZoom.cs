using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public System.Action OnZoomOutComplete;
    public Transform zoomTarget;       
    public float zoomSpeed = 1.5f;     
    public float targetFOV = 30f;      
    public CanvasGroup uiGroup;        

    private Camera cam;
    private bool zoomingIn = false;
    private bool zoomingOut =false;

    private Vector3 startPosition;
    private float startFOV;


    void Start()
    {
        cam = Camera.main;
        startPosition = cam.transform.position;
        startFOV = cam.fieldOfView;
    }

    public void StartZoomIn()
    {
        zoomingIn = true;
        zoomingOut = false;
    }

    public void StartZoomOut()
    {
        zoomingOut = true;
        zoomingIn = false;
    }

    void Update()
    {
        if (zoomingIn)
        {
            Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, 2f);
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.deltaTime * zoomSpeed);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);

            float t = Mathf.InverseLerp(-7.5f, 2f, cam.transform.position.z);
            if (uiGroup != null)
                uiGroup.alpha = 1f - t;

            if (cam.transform.position.z >= 1.95f)
            {
                cam.transform.position = targetPos;
                cam.fieldOfView = targetFOV;
                zoomingIn = false;
            }
        }
        else  if (zoomingOut)
        {
            Vector3 targetPos = startPosition;  
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.deltaTime * zoomSpeed);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, startFOV, Time.deltaTime * zoomSpeed);
            if (uiGroup != null)
                uiGroup.alpha = 1f;

            if (Vector3.Distance(cam.transform.position, targetPos) < 0.05f)
            {
                cam.transform.position = targetPos;
                cam.fieldOfView = startFOV;
                zoomingOut = false;

                
                OnZoomOutComplete?.Invoke();
            }
            
        }
    }



}
