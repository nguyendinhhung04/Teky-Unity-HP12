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
        player.health -= damage;
        
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
        if(collision.gameObject.CompareTag("Player")) //Co cham vao target khong
        {
            if (!isCoolDownAfterAttack)
            {
                Debug.Log("Knight Enemy Hit Player");

                AttackPlayer(collision.gameObject.GetComponent<PlayerScript>());

                isCoolDownAfterAttack = true;

                StartCoroutine(WaitFor3Second(attackCoolDown));
            }

        }
    }
}
