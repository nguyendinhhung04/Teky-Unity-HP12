using JetBrains.Annotations;
using Mono.Cecil;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float health;
    public float speed;
    public float damage;
    public float stamina;

    public GameObject target;
    
    public void Move()
    {
        if (target == null) return;
        Vector3 direction = target.transform.position - gameObject.transform.position;

        if(direction.magnitude < 20f)
        {
            Vector3 moveDistance = direction.normalized * speed * Time.deltaTime;

            gameObject.transform.Translate(moveDistance);
        }
    }

    public void Die()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        target = GameObject.Find("Player");
    }

}
