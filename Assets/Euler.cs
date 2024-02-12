using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct EulerState
{
    public Vector2 pos; // Position
    public Vector2 vel; // Velocity
    public Vector2 force; // Force accumulator

    public void integrate()
    {
        vel += force * Time.fixedDeltaTime;
        pos += vel * Time.fixedDeltaTime;
        force = Vector2.zero;
    }

    public void addForce(Vector2 f)
    {
        force += f;
    }
}

public class Euler : MonoBehaviour
{
    public EulerState state = new EulerState();

    void Start()
    {
        state.pos = transform.position;
        state.vel = Vector2.zero;
        state.force = Vector2.zero;

    }

    void Update()
    {
        // GetKeyDown is unreliable in FixedUpdate, so do this here.
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            state.vel.y = 10.0f;
        }

    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            state.addForce(new Vector2(-7.0f, 0));
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            state.addForce(new Vector2(7.0f, 0));
        }

        // Add gravity
        state.addForce(new Vector2(0, -9.8f));

        // Perform Euler Integration
        state.integrate();

        // If the object goes below the ground, stop it.
        if (state.pos.y < -3.5f)
        {
            state.pos.y = -3.5f;
            state.vel.y = 0;
        }

        // Update gameobject using the state data
        transform.position = state.pos;
    }
}
