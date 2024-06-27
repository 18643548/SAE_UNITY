using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject Direction;
    public GameObject Corps;

    public Transform target;

    private void Update()
    {
        float rawHorizontalAxis = Input.GetAxis("Roll");
        float VerticalAxis = Input.GetAxis("Pitch");
        
        Vector3 direction = Vector3.zero;
        direction.x = rawHorizontalAxis;
        direction.y = VerticalAxis;

        float timeSinceLastFrame = Time.deltaTime;

        Vector3 translation = direction * timeSinceLastFrame*10;

        transform.Translate(
          translation
        );

        transform.LookAt(target);
        transform.LookAt(target, Vector3.left);
    }
}