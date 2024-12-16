using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotControl : MonoBehaviour
{
    public Transform target;
    public float speed = 0.5f;
    public CanvasGroup systemCanvas;

    void Start()
    {
        if (target == null || systemCanvas == null)
        {
            Debug.LogError("Missing required components!");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (systemCanvas.alpha > 0) return;

        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) movement.y += 1;
        if (Input.GetKey(KeyCode.S)) movement.y -= 1;
        if (Input.GetKey(KeyCode.A)) movement.x -= 1;
        if (Input.GetKey(KeyCode.D)) movement.x += 1;
        if (Input.GetKey(KeyCode.Q)) movement.z += 1;
        if (Input.GetKey(KeyCode.E)) movement.z -= 1;

        if (movement != Vector3.zero)
        {
            movement.Normalize();
            target.position += movement * speed * Time.deltaTime;
        }
    }
}
