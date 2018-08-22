using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    
	void Update ()
    {
        transform.rotation *= Quaternion.Euler(Vector3.one * Time.deltaTime * 5);   	
	}
}
