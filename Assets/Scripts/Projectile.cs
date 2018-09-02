using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    float speed = 10;
    float damage = 1;
    float lifeTime = 1;
    public Color trailColor;

    //compensate the effect that enemy s moving too. If it moves and raycast spawns inside - no effect. This is the fix
    float skinWidth = .1f;
    //with which obj can collide
    public LayerMask collisionMask; 
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        //all collids proj intersection with
        Collider[] initCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if(initCollisions.Length > 0)
        {
            OnHitObject(initCollisions[0],transform.position);
        }
        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
    }
    // Update is called once per frame
    void Update () {
        /*currentLife += Time.deltaTime;
        if(!isDead && currentLife >= lifeTime)
        {
            isDead = true;
            Destroy(gameObject);
        }*/

            float moveDist = speed * Time.deltaTime;
            CheckCollisions(moveDist);
            transform.Translate(Vector3.forward * moveDist);

	}

    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        //QueryTrigger - set if can collide with triggers
        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point);
        }
    }

   
    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        IDamageable damageableObj = c.GetComponent<IDamageable>();
        if (damageableObj != null)
        {
            //print(hit.collider.gameObject.name);
            //transform forward - direction
            damageableObj.TakeHit(damage, hitPoint, transform.forward);
        }

        //register hit
        //destroy
        GameObject.Destroy(gameObject);
    }
}
