using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public Team team;
    public float speed = 5f;
    public float acceleration = 5f;
    public float rotationSpeed = 1f;
    public Rigidbody rb;
    private bool released = false;
    public float weight = 20f;
    public AudioSource collisionAudioSource;
    public AudioClip collisionClip;  
    public float maxCollisionVelocity = 10f;
    public float maxCollisionVolume = 1.0f; 

    public BoxCollider safeZoneCollider; // Reference to the safe zone collider

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.mass = weight;
    }

    void Update()
    {
        if(!released)
        {
            float verticalInput = Input.GetAxis("Vertical");
            if (verticalInput > 0)
            {
                rb.AddForce(transform.forward * acceleration * verticalInput, ForceMode.Acceleration);
            }
        }
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        if(released && !IsInSafeZone())
        {
           Destroy(gameObject); // Destroy the stone if it leaves the safe zone
        }
    }

    private bool IsInSafeZone()
    {
        return safeZoneCollider.bounds.Contains(transform.position);
    }

    private bool StoneReleased()
    {
        return released;
    }

    public void SetDrag(float drag)
    {
        rb.drag = drag;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeadZone"))
        {
            Destroy(gameObject); // Destroy the stone on impact
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Stone"))
        {
            float collisionStrength = collision.relativeVelocity.magnitude;
            float volume = Mathf.Clamp01(collisionStrength / maxCollisionVelocity) * maxCollisionVolume; // Adjust volume based on collision strength
            collisionAudioSource.PlayOneShot(collisionClip, volume); // Play the collision sound
        }
    }

}
