using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class LoopAnimationJobSystemTransform : MonoBehaviour
{
    //[SerializeField] Mesh mesh;
    [SerializeField] Material material;

    [Header("Torus Shape")]
    [SerializeField] float shapeRadius = 4;
    [SerializeField] int shapeSides = 5;
    [SerializeField] float sideSize = 0.5f;
    [SerializeField] float radius = 15;

    [Header("Cubes Animation")]
    [SerializeField] int rings = 40;
    [SerializeField] int cubesPerRing = 16;
    [SerializeField] float speed = 5;

    float t = 0;
    //Matrix4x4[] renderBatchMat;
    //float[] instMatParamsArray;
    //int renderBatchSize = 1020;

    //NativeArray<Vector3> ringPositions;
    //NativeArray<Vector3> cubePositions;
    //NativeArray<Quaternion> ringRotations;
    //NativeArray<Quaternion> cubeRotations;
    //NativeArray<Vector3> cubeScales;
    //NativeArray<Matrix4x4> renderMatrices;
    //NativeSlice<Matrix4x4> renderMatricesBatch;
    NativeArray<float> materialParams;
    //NativeSlice<float> materialParamsBatch;

    //TransformAccessArray ringsTransforms;
    TransformAccessArray cubesTransforms;

    //struct TransformRingsJob : IJobParallelForTransform
    //{
    //    public float Radius;
    //    public int Rings;
    //    public int CubesPerRing;
        
    //    public void Execute(int index, TransformAccess transform)
    //    {
    //        float radDeg = 180 / Mathf.PI;

    //        float angle1 = index * 2 * Mathf.PI / Rings;

    //        transform.rotation = Quaternion.Euler(0, angle1 * radDeg, 0);
    //        transform.position = new Vector3(Radius * Mathf.Sin(angle1), 0, Radius * Mathf.Cos(angle1));
    //    }
    //}

    struct TransformCubesJob : IJobParallelForTransform
    {
        public float Time;
        public float ShapeRadius;
        public int ShapeSides;
        public float SideSize;
        public int Rings;
        public int CubesPerRing;

        public float Radius;

        public NativeArray<float> MaterialParams;

        public void Execute(int index, TransformAccess transform)
        {
            float radDeg = 180 / Mathf.PI;

            int i = index / CubesPerRing;
            int j = index % CubesPerRing;

            float angle1 = i * 2 * Mathf.PI / Rings;

            float angle2 = j * 2 * Mathf.PI / CubesPerRing;
            float angle3 = angle1 + angle2 + Time;
            float d = ShapeRadius + SideSize * Mathf.Sin(ShapeSides * angle3);

            //Vector3 ringPos = new Vector3(Radius * Mathf.Sin(angle1), 0, Radius * Mathf.Cos(angle1));
            Quaternion ringRot = Quaternion.Euler(0, angle1 * radDeg, 0);

            transform.position = new Vector3(
                (Radius + d * Mathf.Cos(angle3 + angle1)) * Mathf.Sin(angle1), 
                d * Mathf.Sin(angle3 + angle1), 
                (Radius + d * Mathf.Cos(angle3 + angle1)) * Mathf.Cos(angle1));

            transform.rotation = Quaternion.Euler(angle3 * radDeg, 0, 0) * ringRot;

            transform.localScale = Vector3.one * (1.5f + 1.5f * Mathf.Sin(angle1 * 2 + angle2 + Time * 10));

            MaterialParams[index] = 2f * Mathf.Cos(angle1 + angle2 + Time * 20);
        }
    }


    void Start()
    {
        materialParams = new NativeArray<float>(cubesPerRing * rings, Allocator.Persistent);
        
        cubesTransforms = new TransformAccessArray(rings * cubesPerRing);

        for (int i = 0; i < rings; i++)
        {
            for (int j = 0; j < cubesPerRing; j++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.GetComponent<MeshRenderer>().material = material;
                Destroy(cube.GetComponent<BoxCollider>());
                cubesTransforms.Add(cube.transform);
            }
        }
    }
    
    void Update()
    {
        t += speed * Time.deltaTime;

        var animJob = new TransformCubesJob()
        {
            Time = t,
            ShapeRadius = shapeRadius,
            ShapeSides = shapeSides,
            SideSize = sideSize,
            Rings = rings,
            CubesPerRing = cubesPerRing,
            MaterialParams = materialParams,
        
            Radius = radius
        };

        var animTransformJobDep = animJob.Schedule(cubesTransforms);

        animTransformJobDep.Complete();

    }

    private void OnDisable()
    {
        cubesTransforms.Dispose();
        materialParams.Dispose();
    }
}
