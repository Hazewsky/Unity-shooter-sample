using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour, IDamageable {

    public float startingHealth;
    public float health { get; protected set; }
    protected bool isDead = false;

    public event System.Action OnDeath;

    protected virtual void Start()
    {
        health = startingHealth;
    }
    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        //stuff with hit

        TakeDamage(damage);
    }

   

    public void FullHealth()
    {
        health = startingHealth;
    }
    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    [ContextMenu("Self Destruct")]
    public virtual void Die()
    {
        isDead = true;
        //event of death
        if (OnDeath != null)
        {
            OnDeath();
        }
        Destroy(gameObject);
    }
}
