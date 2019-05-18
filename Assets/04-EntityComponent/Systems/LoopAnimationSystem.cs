using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class LoopAnimationSystem : JobComponentSystem
{
    EntityQuery entitiesTransformGroup;

    protected override void OnCreate()
    {
        entitiesTransformGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Rotation>(),
                ComponentType.ReadOnly<Scale>(),
                ComponentType.ReadOnly<FloatMaterialPropertyBlock>()
            },
        });
    }

    float t = 0;

    [BurstCompile]
    struct LoopAnimationJob : IJobForEachWithEntity<Translation, Rotation, Scale, FloatMaterialPropertyBlock>
    {
        public float Time;
        public float ShapeRadius;
        public int ShapeSides;
        public float SideSize;
        public float Radius;
        public int Rings;
        public int CubesPerRing;

        public void Execute(Entity entity, int index, ref Translation translation, ref Rotation rotation, ref Scale scale, ref FloatMaterialPropertyBlock tint)
        {
            int i = index / CubesPerRing;
            int j = index % CubesPerRing;

            float angle1 = i * 2 * Mathf.PI / Rings;

            quaternion ringRot = quaternion.Euler(0, angle1, 0);

            float angle2 = j * 2 * Mathf.PI / CubesPerRing;
            float angle3 = angle1 + angle2 + Time;
            float d = ShapeRadius + SideSize * math.sin(ShapeSides * angle3);

            translation = new Translation
            {
                Value = new float3(
                    (Radius + d * math.cos(angle3 + angle1)) * math.sin(angle1),
                    d * math.sin(angle3 + angle1),
                    (Radius + d * math.cos(angle3 + angle1)) * math.cos(angle1))
            };

            rotation = new Rotation
            {
                Value = math.mul(ringRot, quaternion.Euler(angle3, 0, 0))
            };

            scale = new Scale
            {
                Value = new float3(1, 1, 1) * (1.5f + 1.5f * math.sin(angle1 * 2 + angle2 + Time * 10))
            };

            tint = new FloatMaterialPropertyBlock
            {
                Value = 2f * math.cos(angle1 + angle2 + Time / 20)
            };
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        t += Bootstrap.Instance.speed * Time.deltaTime;

        var job = new LoopAnimationJob()
        {
            Time = t,
            ShapeRadius = Bootstrap.Instance.shapeRadius,
            ShapeSides = Bootstrap.Instance.shapeSides,
            SideSize = Bootstrap.Instance.sideSize,
            Radius = Bootstrap.Instance.radius,
            Rings = Bootstrap.Instance.rings,
            CubesPerRing = Bootstrap.Instance.cubesPerRing
        };

        JobHandle jobHandler = job.Schedule(entitiesTransformGroup, inputDeps);
        entitiesTransformGroup.AddDependency(inputDeps);

        return jobHandler;
    }
}
