using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] Mesh mesh;
    [SerializeField] Material material;

    [Header("Torus Shape")]
    public float shapeRadius = 4;
    public int shapeSides = 5;
    public float sideSize = 0.5f;
    public float radius = 15;

    [Header("Cubes Animation")]
    public int rings = 40;
    public int cubesPerRing = 16;
    public float speed = 5;

    public static Bootstrap Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        EntityManager entityManager = World.Active.EntityManager;
        NativeArray<Entity> entities = new NativeArray<Entity>(cubesPerRing * rings, Allocator.Temp);

        var entity = entityManager.CreateEntity(
            ComponentType.ReadWrite<Translation>(),
            ComponentType.ReadWrite<Rotation>(),
            ComponentType.ReadWrite<Scale>(),
            ComponentType.ReadWrite<FloatMaterialPropertyBlock>(),
            ComponentType.ReadWrite<LocalToWorld>(),
            ComponentType.ReadWrite<RenderMesh>());

        entityManager.Instantiate(entity, entities);

        for (int i = 0; i < entities.Length; i++)
        {
            entityManager.SetSharedComponentData(entities[i], new RenderMesh { mesh = mesh, material = material });
        }

        entityManager.DestroyEntity(entity);
        entities.Dispose();
    }
}
