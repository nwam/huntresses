using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float speed = 15f;

    // Use this for initialization
    void Start()
    {
        speed = 1.5f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        transform.position += speed * transform.up * Time.deltaTime;
    }
}
