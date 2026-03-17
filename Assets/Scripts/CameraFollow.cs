using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float fixedY = 0f; // ajusta este valor en el Inspector

    void LateUpdate()
    {
        transform.position = new Vector3(
            player.position.x,
            fixedY,
            -10
        );
    }
}