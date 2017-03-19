using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour {

    public Vector3 center = Vector3.zero;
    public Vector3 axis = Vector3.up;
    public float speed = 1;

    //private float angle = 0;

	// Update is called once per frame
	void Update () {
        //angle += Time.deltaTime * speed;
        transform.RotateAround(center, axis, Time.deltaTime * speed);
        transform.LookAt(center);
	}
}
