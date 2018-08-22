using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class LoopAnimationJobSystem : MonoBehaviour
{
    [SerializeField] Mesh mesh;
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
    Matrix4x4[] renderBatchMat;
    float[] instMatParamsArray;
    int renderBatchSize = 1020;

    //NativeArray<Vector3> ringPositions;
    NativeArray<Vector3> cubePositions;
    //NativeArray<Quaternion> ringRotations;
    NativeArray<Quaternion> cubeRotations;
    NativeArray<Vector3> cubeScales;
    NativeArray<Matrix4x4> renderMatrices;
    NativeSlice<Matrix4x4> renderMatricesBatch;
    NativeArray<float> materialParams;
    NativeSlice<float> materialParamsBatch;

    struct AnimationJob : IJobParallelFor
    {
        public float Time;
        public float ShapeRadius;
        public int ShapeSides;
        public float SideSize;
        public float Radius;
        public int Rings;
        public int CubesPerRing;
        //public NativeArray<Vector3> RingPositions;
        public NativeArray<Vector3> CubePositions;
        //public NativeArray<Quaternion> RingRotations;
        public NativeArray<Quaternion> CubeRotations;
        public NativeArray<Vector3> CubeScales;
        public NativeArray<float> MaterialParams;

        public void Execute(int index)
        {
            float radDeg = 180 / Mathf.PI;

            int i = index / CubesPerRing;
            int j = index % CubesPerRing;

            float angle1 = i * 2 * Mathf.PI / Rings;

            Quaternion ringRot = Quaternion.Euler(0, angle1 * radDeg, 0);
            //Vector3 ringPos = new Vector3(Radius * Mathf.Sin(angle1), 0, Radius * Mathf.Cos(angle1));

            float angle2 = j * 2 * Mathf.PI / CubesPerRing;
            float angle3 = angle1 + angle2 + Time;
            float d = ShapeRadius + SideSize * Mathf.Sin(ShapeSides * angle3);

            CubePositions[index] = new Vector3(
                (Radius + d * Mathf.Cos(angle3 + angle1)) * Mathf.Sin(angle1),
                d * Mathf.Sin(angle3 + angle1),
                (Radius + d * Mathf.Cos(angle3 + angle1)) * Mathf.Cos(angle1));

            CubeRotations[index] = Quaternion.Euler(angle3 * radDeg, 0, 0) * ringRot;

            CubeScales[index] = Vector3.one * (1.5f + 1.5f * Mathf.Sin(angle1 * 2 + angle2 + Time * 10));

            MaterialParams[index] = 2f * Mathf.Cos(angle1 + angle2 + Time / 20);
        }
    }

    struct TransformJob : IJobParallelFor
    {
        public Matrix4x4 WorldMatrix;
        //[ReadOnly] public NativeArray<Vector3> RingPositions;
        [ReadOnly] public NativeArray<Vector3> CubePositions;
        //[ReadOnly] public NativeArray<Quaternion> RingRotations;
        [ReadOnly] public NativeArray<Quaternion> CubeRotations;
        [ReadOnly] public NativeArray<Vector3> CubeScales;
        public NativeArray<Matrix4x4> RenderMatrices;

        public void Execute(int index)
        {
            //operations apply right to left
            //Matrix4x4 ringMat = Matrix4x4.TRS(RingPositions[index], RingRotations[index], Vector3.one);
            Matrix4x4 cubeMat = Matrix4x4.TRS(CubePositions[index], CubeRotations[index], CubeScales[index]);
            RenderMatrices[index] = cubeMat;
        }
    }

    void Start()
    {
        renderBatchMat = new Matrix4x4[renderBatchSize];
        instMatParamsArray = new float[renderBatchSize];

        //ringPositions = new NativeArray<Vector3>(cubesPerRing * rings, Allocator.Persistent);
        cubePositions = new NativeArray<Vector3>(cubesPerRing * rings, Allocator.Persistent);
        //ringRotations = new NativeArray<Quaternion>(cubesPerRing * rings, Allocator.Persistent);
        cubeRotations = new NativeArray<Quaternion>(cubesPerRing * rings, Allocator.Persistent);
        cubeScales = new NativeArray<Vector3>(cubesPerRing * rings, Allocator.Persistent);
        renderMatrices = new NativeArray<Matrix4x4>(cubesPerRing * rings, Allocator.Persistent);
        materialParams = new NativeArray<float>(cubesPerRing * rings, Allocator.Persistent);
    }

    

    void Update()
    {
        t += speed * Time.deltaTime;

        var animJob = new AnimationJob()
        {
            Time = t,
            ShapeRadius = shapeRadius,
            ShapeSides = shapeSides,
            SideSize = sideSize,
            Radius = Radius,
            Rings = rings,
            CubesPerRing = cubesPerRing,
            //RingPositions = ringPositions,
            CubePositions = cubePositions,
            //RingRotations = ringRotations,
            CubeRotations = cubeRotations,
            CubeScales = cubeScales,
            MaterialParams = materialParams
        };
        
        var animJobDep = animJob.Schedule(cubesPerRing * rings, 32);

        var transformJob = new TransformJob()
        {
            WorldMatrix = transform.localToWorldMatrix,
            //RingPositions = ringPositions,
            CubePositions = cubePositions,
            //RingRotations = ringRotations,
            CubeRotations = cubeRotations,
            CubeScales = cubeScales,
            RenderMatrices = renderMatrices,
        };

        var transformJobDep = transformJob.Schedule(cubesPerRing * rings, 32, animJobDep);

        transformJobDep.Complete();

        int objectCount = rings * cubesPerRing;
        int loopRenderBatchSize;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        for (int i = 0; i < objectCount; i += renderBatchSize)
        {
            if ((i + renderBatchSize) < objectCount)
                loopRenderBatchSize = renderBatchSize;
            else
                loopRenderBatchSize = objectCount - i;

            if (renderBatchMat.Length != loopRenderBatchSize)
            {
                renderBatchMat = new Matrix4x4[loopRenderBatchSize];
                instMatParamsArray = new float[loopRenderBatchSize];
            }

            renderMatricesBatch = new NativeSlice<Matrix4x4>(renderMatrices, i, loopRenderBatchSize);
            renderMatricesBatch.CopyTo(renderBatchMat);
            materialParamsBatch = new NativeSlice<float>(materialParams, i, loopRenderBatchSize);
            materialParamsBatch.CopyTo(instMatParamsArray);

            mpb.SetFloatArray("_Hue", instMatParamsArray);

            Graphics.DrawMeshInstanced(mesh, 0, material, renderBatchMat, loopRenderBatchSize, mpb);
        }
    }

    private void OnDisable()
    {
        //ringPositions.Dispose();
        cubePositions.Dispose();
        //ringRotations.Dispose();
        cubeRotations.Dispose();
        cubeScales.Dispose();
        renderMatrices.Dispose();
        materialParams.Dispose();
    }
}
