using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopAnimationInstancedGPU : MonoBehaviour
{
    List<Matrix4x4> cubes;
    List<float> matParams;
    Matrix4x4[] renderBatchMat;
    float[] instMatParamsArray;
    int renderBatchSize = 1020;

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

    void Start()
    {
        cubes = new List<Matrix4x4>();
        matParams = new List<float>();
        renderBatchMat = new Matrix4x4[renderBatchSize];
        instMatParamsArray = new float[renderBatchSize];

        for (int i = 0; i < rings; i++)
        {
            for (int j = 0; j < cubesPerRing; j++)
            {
                cubes.Add(new Matrix4x4());
                matParams.Add(0);
            }
        }
    }

    void Update()
    {
        Matrix4x4 ringMat;
        Matrix4x4 cubeMat;
        Vector3 ringPos, cubePos;
        Quaternion ringRot, cubeRot;
        Vector3 cubeScale;

        t += speed * Time.deltaTime;
        for (int i = 0; i < rings; i++)
        {
            float angle1 = i * 2 * Mathf.PI / rings;
            ringRot = Quaternion.Euler(0, angle1 * 180 / Mathf.PI, 0);
            ringPos = new Vector3(Radius * Mathf.Sin(angle1), 0, Radius * Mathf.Cos(angle1));
            
            for (int j = 0; j < cubesPerRing; j++)
            {
                float angle2 = j * 2 * Mathf.PI / cubesPerRing;
                float angle3 = angle1 + angle2 + t;
                float d = shapeRadius + sideSize * Mathf.Sin(shapeSides * angle3);
                cubePos = new Vector3(0, d * Mathf.Cos(angle3 + angle1), d * Mathf.Sin(angle3 + angle1));
                cubeRot = Quaternion.Euler(angle3 * 180 / Mathf.PI, 0, 0);
                cubeScale = Vector3.one * (1.5f + 1.5f * Mathf.Sin(angle1 * 2 + j * 2 * Mathf.PI / cubesPerRing + t * 10));
                //operations apply right to left
                ringMat = Matrix4x4.Translate(ringPos) * Matrix4x4.Rotate(ringRot);
                cubeMat = Matrix4x4.Translate(cubePos) * Matrix4x4.Rotate(cubeRot) * Matrix4x4.Scale(cubeScale);
                cubes[i * cubesPerRing + j] = transform.localToWorldMatrix * ringMat * cubeMat;
                matParams[i * cubesPerRing + j] = 2f * Mathf.Cos(angle1 + j * 2 * Mathf.PI / cubesPerRing + t * 20);
            }
        }

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
            renderBatchMat = cubes.GetRange(i, loopRenderBatchSize).ToArray();
            instMatParamsArray = matParams.GetRange(i, loopRenderBatchSize).ToArray();

            mpb.SetFloatArray("_Hue", instMatParamsArray);

            Graphics.DrawMeshInstanced(mesh, 0, material, renderBatchMat, loopRenderBatchSize, mpb);
        }
    }
}
