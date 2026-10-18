using UnityEngine;
using System.Collections;

public class Knight_Enemy : Enemy
{
    public bool isCoolDownAfterAttack = false;
    IEnumerator WaitFor3Second()
    {
        // Suspend execution for 3 seconds
        yield return new WaitForSeconds(3f);
    }


    public void AttackPlayer(PlayerScript player)
    {
        player.health -= damage;
        
    }


    private void Update()
    {
        if(isCoolDownAfterAttack==true)
        {
            float enemySpeed = speed;
            speed = 0;
            StartCoroutine(WaitFor3Second());
            speed = enemySpeed;
            isCoolDownAfterAttack = false;

        }

        if (health <= 0)
        {
            Die();
        }
        Move();

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player")) //Co cham vao target khong
        {
            Debug.Log("Knight Enemy Hit Player");
            AttackPlayer(target.GetComponent<PlayerScript>());
        }
    }
}
