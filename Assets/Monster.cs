using UnityEngine;

public class Monster : MonoBehaviour
{
    public float health = 100f;
    public float speed = 2f;
    public float damage = 10f;
    public float detectionRange = 20f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;
    public GameObject target;

    private float attackTimer;

    private void Start()
    {
        FindTarget();
    }

    private void Update()
    {
        if (health <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        if (target == null)
        {
            FindTarget();
        }

        if (target == null)
        {
            return;
        }

        Vector2 direction = target.transform.position - transform.position;
        float distance = direction.magnitude;

        if (distance > detectionRange)
        {
            return;
        }

        attackTimer -= Time.deltaTime;
        if (distance <= attackRange)
        {
            if (attackTimer <= 0f)
            {
                Player1 player1 = target.GetComponent<Player1>();
                if (player1 != null)
                {
                    player1.TakeDamage(damage);
                    attackTimer = attackCooldown;
                    return;
                }

                PlayerScript player = target.GetComponent<PlayerScript>();
                if (player != null && damage > 0f)
                {
                    player.health = Mathf.Max(0f, player.health - damage);
                    attackTimer = attackCooldown;
                }
            }

            return;
        }

        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
    }

    private void FindTarget()
    {
        Player1 player1 = FindFirstObjectByType<Player1>();
        if (player1 != null)
        {
            target = player1.gameObject;
            return;
        }

        PlayerScript player = FindFirstObjectByType<PlayerScript>();
        if (player != null)
        {
            target = player.gameObject;
        }
    }
}
