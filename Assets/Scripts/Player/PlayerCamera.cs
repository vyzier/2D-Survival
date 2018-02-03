using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform target;
    public float speed = 10;

    public bool enableXMovement, enableYMovement;

    private void FixedUpdate()
    {
        if (target != null)
        {
            Vector3 pos = transform.position;

            if (enableXMovement)
                pos.x = Mathf.Lerp(pos.x, target.position.x, speed * Time.deltaTime);

            if (enableYMovement)
                pos.y = Mathf.Lerp(pos.y, target.position.y, speed * Time.deltaTime);

            transform.position = pos;
        }
    }
}