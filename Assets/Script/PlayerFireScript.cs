using UnityEngine;

public class PlayerFireScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject bulletPrefab;

    public KeyCode keyBind;

    public float bulletSpeed;

    public Vector3 mouse;
    public Vector3 player;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(keyBind))
        {
            if (bulletPrefab.GetComponent<Rigidbody2D>() == null)
            {
                Debug.Log("No Rigidbody2D in bulletPrefab");
                return;
            }

            Vector3 screenPos = Input.mousePosition;

            // Đổi sang tọa độ thế giới
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
            mouseWorldPos.z = 0f; // Game 2D nên đặt z = 0

            Vector3 fireDirection = (mouseWorldPos - transform.position).normalized;

            player = transform.position;
            mouse = mouseWorldPos;

            GameObject clone = Instantiate(bulletPrefab, transform);
            clone.GetComponent<Rigidbody2D>().linearVelocity = fireDirection * bulletSpeed;

        }
    }
}
