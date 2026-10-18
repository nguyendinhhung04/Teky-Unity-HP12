using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float health;

    public float speed;

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(moveX, moveY, 0) * speed * Time.deltaTime;

        transform.Translate(moveDirection);


    }
}
