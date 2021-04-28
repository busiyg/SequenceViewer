using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x;
        float z;
        float y;
        y = 0;

        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.Q))
        {
            y = 1;
        }

        if (Input.GetKey(KeyCode.E))
        {
            y = -1;
        }

        Vector3 move = transform.right * x + transform.forward * z+ transform.up * y;
        transform.position += move* speed;
    }
}
