using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] GameObject model;
    Vector3 direction;
    float distance;
    float dt;
    float timePassed = 0;

    public void Create(Vector3 targetPosition)
    {
        // Calculate distance and direction
        direction = (targetPosition - gameObject.transform.position).normalized;
        distance = (targetPosition - gameObject.transform.position).magnitude;

        // Rotate to face the target (in all 3 directions)
        gameObject.transform.LookAt(targetPosition, Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        // Move the projectile if Create() was called already
        if (direction != null)
        {
            // Update delta time
            dt = Time.deltaTime;
            timePassed += dt;

            // Move the projectile
            gameObject.transform.position += direction * speed * dt;

            // Stop the projectile if its reached its end
            if (timePassed * speed >= distance) // the projectile has traveled its full distance
            {
                ReachTarget();
            }
        }

        // Delete automatically after 10 seconds
        if (timePassed > 600)
        {
            Destroy(gameObject);
        }
    }

    // Get rid of the projectile
    private void ReachTarget()
    {
        // TODO: Turn it off and store for later use probably

        // turn off the particle system if there is one
        if (particleSystem != null) // There is a particle system
        {
            particleSystem.Stop();
        }

        // Make the model invisible if there is one
        if (model != null) // There is a model
        {
            model.SetActive(false);
        }
    }
}
