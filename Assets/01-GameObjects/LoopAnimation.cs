using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LoopAnimation : MonoBehaviour
{
    List<Transform> cubes = new List<Transform>();

    [SerializeField] bool customMaterial;
    [SerializeField] Material material;

    [Header("Torus Shape")]
    [SerializeField] float shapeRadius = 4;
    [SerializeField] int shapeSides = 5;
    [SerializeField] float sideSize = 0.5f;
    [SerializeField] float Radius = 15;

    [Header("Cubes Animation")]
    [SerializeField] int rings = 40;
    [SerializeField] int cubesPerRing = 16;
    [SerializeField] float speed = 5;

    float t = 0;

    void Start()
    {
        for (int i = 0; i < rings; i++)
        {
            GameObject ring = new GameObject();
            ring.transform.parent = transform;
            cubes.Add(ring.transform);
            for (int j = 0; j < cubesPerRing; j++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (customMaterial)
                    cube.GetComponent<MeshRenderer>().sharedMaterial = material;
                Destroy(cube.GetComponent<BoxCollider>());
                cube.transform.parent = ring.transform;
            }
        }
    }

    void Update()
    {
        t += speed * Time.deltaTime;
        for (int i = 0; i < rings; i++)
        {
            Transform ring = cubes[i];
            float angle1 = i * 2 * Mathf.PI / rings;
            ring.transform.localRotation = Quaternion.Euler(0, angle1 * 180 / Mathf.PI, 0);
            ring.transform.localPosition = new Vector3(Radius * Mathf.Sin(angle1), 0, Radius * Mathf.Cos(angle1));
            for (int j = 0; j < cubesPerRing; j++)
            {
                Transform cube = ring.GetChild(j);
                float angle2 = j * 2 * Mathf.PI / cubesPerRing;
                float angle3 = angle1 + angle2 + t;
                float d = shapeRadius + sideSize * Mathf.Sin(shapeSides * angle3);
                cube.transform.localPosition = new Vector3(0, d * Mathf.Cos(angle3 + angle1), d * Mathf.Sin(angle3 + angle1));
                cube.transform.localRotation = Quaternion.Euler(angle3 * 180 / Mathf.PI, 0, 0);
                cube.transform.localScale = Vector3.one * (1.5f + 1.5f * Mathf.Sin(angle1 * 2 + j * 2 * Mathf.PI / cubesPerRing + t*10));
                if (customMaterial)
                    cube.GetComponent<MeshRenderer>().material.SetFloat("_Hue", 2f * Mathf.Cos(angle1 + j * 2 * Mathf.PI / cubesPerRing + t * 20));
            }
        }
    }
}
