using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{

    public float lerpSpeed = 5.0f;
    private float z = 0;
    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        z = transform.position.z;
    }

    public void UpdatePosition()
    {
        transform.position = target.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null) {
            transform.position = new Vector3(Mathf.Lerp(transform.position.x, target.position.x, lerpSpeed * Time.deltaTime), Mathf.Lerp(transform.position.y, target.position.y, lerpSpeed * Time.deltaTime), z);
        }
    }
}
