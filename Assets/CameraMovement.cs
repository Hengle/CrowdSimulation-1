﻿using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float edge = 10f;
    public float speed = 1f;
    private Vector3 right;
    private Vector3 forward;

    // Update is called once per frame
    void Update()
    {
        if (RayCast.IsPointerOverUIObject()) return;


        right = transform.right;
        forward = transform.forward;
        right.y = 0;
        forward.y = 0;
        right.Normalize();
        forward.Normalize();

        var mp = Input.mousePosition;
        //if (mp.y > Screen.height || mp.y < 0 || mp.x > Screen.width || mp.x < 0) return;
        if (mp.x > Screen.width - edge)
        {
            transform.position += (mp.x - (Screen.width - edge)) / (edge) * right * Time.deltaTime * speed;
        }
        if (mp.x < edge)
        {
            transform.position += (edge - mp.x) / (edge) * -right * Time.deltaTime * speed;
        }

        if (mp.y > Screen.height - edge)
        {
            transform.position += (mp.y - (Screen.height - edge)) / (edge) * forward * Time.deltaTime * speed;
        }
        if (mp.y < edge)
        {
            transform.position += (edge - mp.y) / (edge) * -forward * Time.deltaTime * speed;
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += forward * Time.deltaTime * speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= forward * Time.deltaTime * speed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= right * Time.deltaTime * speed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += right * Time.deltaTime * speed;
        }
        var scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0f)
        {
            if (!Camera.main.orthographic)
            {
                transform.position += transform.forward * scroll * speed * Time.deltaTime;
            } else
            {
                Camera.main.orthographicSize -= scroll * speed * Time.deltaTime;
            }
            
        }
    }
}
