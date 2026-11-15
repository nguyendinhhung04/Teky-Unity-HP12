using UnityEngine;
using System.Collections;

public class Knight_Enemy : Enemy
{
    public bool isCoolDownAfterAttack = false;

    public float attackCoolDown = 0.5f;
    IEnumerator WaitFor3Second(float time)
    {
        float enemySpeed = speed;
        speed = 0;
        // Suspend execution for 3 seconds
        yield return new WaitForSeconds(time);
        speed = enemySpeed;
        isCoolDownAfterAttack = false;
    }


    public void AttackPlayer(PlayerScript player)
    {
        if (player != null && damage > 0f)
        {
            player.health = Mathf.Max(0f, player.health - damage);
        }
    }


    protected override void Update()
    {

        if (health <= 0)
        {
            Die();
            return;
        }
        Move();

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Player1 player1 = collision.gameObject.GetComponentInParent<Player1>();
        PlayerScript player = collision.gameObject.GetComponentInParent<PlayerScript>();
        if (player1 == null && player == null)
        {
            return;
        }

        if (isCoolDownAfterAttack)
        {
            return;
        }

        if (player1 != null)
        {
            player1.TakeDamage(damage);
        }
        else
        {
            AttackPlayer(player);
        }

        isCoolDownAfterAttack = true;
        StartCoroutine(WaitFor3Second(attackCoolDown));
    }
}
