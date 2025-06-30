using UnityEngine;
using UnityEngine.UI;

public class HeartRotator : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 20f, 0);
    public float interval = 2f;
    public float rotationAngle = 30f;
    private float timer = 0f;
    private int rotationStep = 0;
    private bool isStepRotationActive = false;

    private Button startButton;

    void Start()
    {
        // Cerca il bottone con nome "StartButton" nella scena
        GameObject buttonObj = GameObject.Find("StartButton");
        if (buttonObj != null)
        {
            startButton = buttonObj.GetComponent<Button>();
            if (startButton != null)
                startButton.onClick.AddListener(OnStartButtonClicked);
            else
                Debug.LogWarning("StartButton non ha componente Button.");
        }
        else
        {
            Debug.LogWarning("StartButton non trovato in scena.");
        }
    }

   public bool enableStepRotation = false; // Impostalo da Inspector

    void Update()
    {
        if (!isStepRotationActive)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
        else if (enableStepRotation)
        {
            timer += Time.deltaTime;

            if (timer >= interval)
            {
                switch (rotationStep)
                {
                    case 0:
                        transform.Rotate(0f, rotationAngle, 0f);
                        break;
                    case 1:
                        transform.Rotate(0f, -rotationAngle, 0f);
                        break;
                    case 2:
                        transform.Rotate(rotationAngle, 0f, 0f);
                        break;
                    case 3:
                        transform.Rotate(-rotationAngle, 0f, 0f);
                        break;
                }

                rotationStep = (rotationStep + 1) % 4;
                timer = 0f;
            }
        }
    }


    public void OnStartButtonClicked()
    {
        isStepRotationActive = true;
        timer = 0f;
        rotationStep = 0;
    }
}
