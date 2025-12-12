using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class SteeringAgent : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 5f;
    public float maxForce = 10f; // Limit how "fast" we can change
                                 //direction(turning radius)
    [Header("Arrive")]
    public float slowingRadius = 3f;
    [Header("Separation")]
    public float separationRadius = 1.5f;
    public float separationStrength = 5f;
    [Header("Weights")]
    public float arriveWeight = 1f;
    public float separationWeight = 1.5f;
    public float cohesionWeight = 0.5f;
    public float alignmentWeight = 0.7f;
    [Header("Debug")]
    public bool drawDebug = true;
    private Vector3 velocity = Vector3.zero;
    // Optional target for Seek / Arrive
    public Transform target;
    // Static list so agents can find each other
    public static List<SteeringAgent> allAgents = new
    List<SteeringAgent>();
    private void OnEnable()
    {
        allAgents.Add(this);
    }
    private void OnDisable()
    {
        allAgents.Remove(this);
    }
    void Update()
    {
        Vector3 totalSteering = Vector3.zero;
        // 1. Arrive (or Seek) towards target, if any
        if (target != null)
        {
            totalSteering += Arrive(target.position, slowingRadius) *
            arriveWeight;
        }
        // 2. Separation: only if there are neighbours
        if (allAgents.Count > 1)
        {
            totalSteering += Separation(separationRadius,
            separationStrength) * separationWeight;
            totalSteering += Cohesion(separationRadius) * cohesionWeight;
            totalSteering += Alignment(separationRadius) * alignmentWeight;
        }
        // 3. Clamp total force (agents have finite strength)
        totalSteering = Vector3.ClampMagnitude(totalSteering, maxForce);
        // 4. Integration (same as before)
        velocity += totalSteering * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;
        if (velocity.sqrMagnitude > 0.0001f)
        {
            transform.forward = velocity.normalized;
        }
    }
    // -- BEHAVIOUR STUBS --
    public Vector3 Seek(Vector3 targetPosition)
    {
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;
        // If we are already there, stop steering
        if (toTarget.sqrMagnitude < 0.0001f)
            return Vector3.zero;
        // Desired Velocity: Full speed towards target
        Vector3 desired = toTarget.normalized * maxSpeed;
        // Reynolds' Steering Formula
        return desired - velocity;
    }
    public Vector3 Arrive(Vector3 targetPosition, float slowingRadius)
    {
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;
        if (dist < 0.0001f)
            return Vector3.zero;
        float desiredSpeed = maxSpeed;
        // Ramp down speed if within radius
        if (dist < slowingRadius)
        {
            desiredSpeed = maxSpeed * (dist / slowingRadius);
        }
        Vector3 desired = toTarget.normalized * desiredSpeed;
        return desired - velocity;
    }
    public Vector3 Separation(float separationRadius, float
    separationStrength)
    {
        Vector3 force = Vector3.zero;
        int neighbourCount = 0;
        foreach (SteeringAgent other in allAgents)
        {
            if (other == this) continue;
            Vector3 toMe = transform.position -
            other.transform.position;
            toMe.y = 0f;
            float dist = toMe.magnitude;
            // If they are within my personal space
            if (dist > 0f && dist < separationRadius)
            {
                force += toMe.normalized / dist;
                neighbourCount++;
            }
        }
        if (neighbourCount > 0)
        {
            force /= neighbourCount;
            force = force.normalized * maxSpeed;
            force = force - velocity;
            force *= separationStrength;
        }
        return force;
    }
    public Vector3 Cohesion(float radius)
    {
        Vector3 sumPositions = Vector3.zero;
        int neighbourCount = 0;

        foreach(SteeringAgent agent in allAgents)
        {
            if (agent == this) continue;
            float distance = Vector3.Distance(transform.position, agent.transform.position);

            if(distance > 0f && distance < radius)
            {
                sumPositions += agent.transform.position;
                neighbourCount++;
            }
        }

        if(neighbourCount == 0) return Vector3.zero;

        Vector3 averagePos = sumPositions / neighbourCount;
        averagePos.y = transform.position.y;
        return Arrive(averagePos, radius);
    }

    public Vector3 Alignment(float radius)
    {
        Vector3 sumVelocities = Vector3.zero;
        int neighbourCount = 0;

        foreach(SteeringAgent agent in allAgents)
        {
            if(agent == this) continue;

            float distance = Vector3.Distance(transform.position, agent.transform.position);

            if(distance> 0f && distance < radius)
            {
                sumVelocities += agent.velocity;
                neighbourCount++;
            }
        }

        if (neighbourCount == 0) return Vector3.zero;

        Vector3 averageVelocity = sumVelocities / neighbourCount;
        averageVelocity.y = 0;
        Vector3 desired = averageVelocity.normalized * maxSpeed;

        return desired - velocity;
    }
    private void OnDrawGizmosSelected()
    {
        if (!drawDebug) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position +
        velocity);
    }
}