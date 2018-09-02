using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : Entity {

    public ParticleSystem deathEffect;
    
    NavMeshAgent pathfinder;
    Transform target;
    Material skinMaterial;
    Entity targetEntity;
    public static event System.Action OnDeathStatic;

    Color originalColor;
    Color attackColor = new Color(.85f, .05f, .17f);
    float attackDistanceThreshold = .5f;
    float timeBetweenAttacks = 1;
    float attackSpeed = 5;
    float nextAttackTime;
    float chaseDistanceThreshold = 10f;
    float attackDamage = 1;


    float collisionRadius;
    float targetCollisionRadius;

    public enum State {Idle, Chasing, Attacking}

    State currentState;
    bool hasTarget;
    //Awake before start before everything
    private void Awake()
    {
        //pathFinder
        pathfinder = GetComponent<NavMeshAgent>();
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<Entity>();   
            //collision
            collisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

        }
    }
    // Use this for initialization
    protected override void Start () {
        base.Start();
        if (hasTarget)
        {
            currentState = State.Chasing;      
            targetEntity.OnDeath += OnTargetDeath;
            //start enumerator
            StartCoroutine(UpdatePath());
        }

    }
	
    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }
    // Update is called once per frame
    void Update() {
        if (hasTarget) {
            if (Time.time > nextAttackTime)
            {
                // Vector3.Distance() use square root - expensive
                //better
                float sqrDistToTarget = (target.position - transform.position).sqrMagnitude;
                //print(sqrDistToTarget);
                //if in range
                if (sqrDistToTarget < (attackDistanceThreshold + collisionRadius + targetCollisionRadius)
                    * (attackDistanceThreshold + collisionRadius + targetCollisionRadius))
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());

                }
            }
        }
	}

    public void SetCharacteristics(float moveSpeed,int hitsToKillPlayer, float _health, Color skinColor)
    {
        pathfinder.speed = moveSpeed;
        if (hasTarget)
        {
            //to closest int
            attackDamage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }
        health = _health;
        startingHealth = _health;
        //material + color
        skinMaterial = GetComponent<Renderer>().material;
        skinMaterial.color = skinColor;
        originalColor = skinColor;
        //change color of death effect
        ParticleSystemRenderer pr = deathEffect.GetComponent<ParticleSystemRenderer>();
      // pr.renderMode = ParticleSystemRenderMode.VerticalBillboard;
        pr.sharedMaterial.color = skinColor;

    }

    IEnumerator Attack()
    {
        State bufState = currentState;
        currentState = State.Attacking;
        pathfinder.enabled = false;
        skinMaterial.color = attackColor;
        Vector3 originPos = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        //not to enter each other's rigidbodies + dist to attack
        Vector3 attackPos = target.position - dirToTarget * (collisionRadius);
       
        float percent = 0;
        bool hasAppliedDamage = false;
        while(percent <= 1)
        {
            if(percent >= .5 && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(attackDamage);
            }
            //skip frame between each step
            percent += Time.deltaTime * attackSpeed;
            //from 0 to 1 and after from 1 to 0
            float interpolation = (-percent * percent + percent) *4;
            transform.position = Vector3.Lerp(originPos, attackPos, interpolation);
            yield return null;
        }
        skinMaterial.color = originalColor;
        currentState = bufState;
        pathfinder.enabled = true;

      
    }


        IEnumerator UpdatePath()
    {

        //update every 1 sec
        float refreshRate = .25f;
        while(hasTarget)
        {
            if(currentState != State.Attacking)
            {
                float sqrDistToTarget = (target.position - transform.position).sqrMagnitude;
                //if in range
                if (sqrDistToTarget < chaseDistanceThreshold * chaseDistanceThreshold)
                {
                    currentState = State.Chasing;
                }
                else
                {
                    if(currentState != State.Idle)
                    {
                        currentState = State.Idle;
                    }
                }
            }
            if (currentState == State.Chasing)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                //not to enter each other's rigidbodies + dist to attack
                Vector3 targetPos = target.position - dirToTarget * (collisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
                    if (!isDead)
                    {
                        pathfinder.SetDestination(targetPos);
                    }
            }
           
            //no need to create a new class with IEnum to use

            yield return new WaitForSeconds(refreshRate);
        }
    }
    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);
        if(damage >= health)
        {
            //if dead - fire an event
            if (OnDeathStatic != null)
                OnDeathStatic();
            //quaterion cvt pos to rotation
            //So if the variable in question is not the type required, it will return null.
            //The as keyword also requires the type to be nullable, where as explicit (type) casting does not.
            //This is important because ParticleSystem in Unity 5 is not derived from a GameObject. 
            //Thus, casting it with (GameObject)someParticleSystem will throw an exception
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.main.startLifetimeMultiplier);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }
}
