using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float turnSpeed = 2f;
    [SerializeField]
    private int health = 3;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        bool turning = false;
        if (this.transform.position.y >= 5)
        {
            this.transform.SetPositionAndRotation(new Vector3(this.transform.position.x, 5, 0), this.transform.rotation);
            this.transform.Rotate(this.transform.forward, 180 * turnSpeed * Time.deltaTime);
            if (this.transform.rotation.z > 0)
            {
                turning = true;
            }
            else
            {
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 180));
            }
        }
        else if (this.transform.position.y <= -5)
        {
            this.transform.SetPositionAndRotation(new Vector3(this.transform.position.x, -5, 0), this.transform.rotation);
            this.transform.Rotate(this.transform.forward, 180 * turnSpeed * Time.deltaTime);
            if (this.transform.rotation.z < 0)
            {
                turning = true;
            }
            else
            {
                transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, 0));
            }
        }

        if (!turning)
        {
            this.transform.position += speed * this.transform.up * Time.deltaTime;
        }
    }
}
