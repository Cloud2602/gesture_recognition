using UnityEngine;

public class HeartRotator : MonoBehaviour
{
    public float interval = 2f;
    public float rotationAngle = 30f;
    private float timer = 0f;

    // Contatore per ciclo rotazioni (0,1,2,3)
    private int rotationStep = 0;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            switch (rotationStep)
            {
                case 0:
                    // Ruota in senso orario sull'asse Y
                    transform.Rotate(0f, rotationAngle, 0f);
                    break;
                case 1:
                    // Ruota in senso antiorario sull'asse Y
                    transform.Rotate(0f, -rotationAngle, 0f);
                    break;
                case 2:
                    // Ruota verso l'alto sull'asse X
                    transform.Rotate(rotationAngle, 0f, 0f);
                    break;
                case 3:
                    // Ruota verso il basso sull'asse X
                    transform.Rotate(-rotationAngle, 0f, 0f);
                    break;
            }

            rotationStep = (rotationStep + 1) % 4; // ciclo 0-3
            timer = 0f;
        }
    }
}
