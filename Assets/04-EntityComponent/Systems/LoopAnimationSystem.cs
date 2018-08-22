using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class LoopAnimationSystem : JobComponentSystem
{
    struct TransformTintGroup
    {
        public ComponentDataArray<Position> positions;
        public ComponentDataArray<Rotation> rotations;
        public ComponentDataArray<Scale> scales;
        public ComponentDataArray<Tint> tints;
        public readonly int Length;
    }

    [Inject]
    TransformTintGroup entitiesTransformGroup;
    
    float t = 0;

    [BurstCompile]
    struct LoopAnimationJob : IJobParallelFor
    {
        public float Time;
        public float ShapeRadius;
        public int ShapeSides;
        public float SideSize;
        public float Radius;
        public int Rings;
        public int CubesPerRing;
        public ComponentDataArray<Position> positions;
        public ComponentDataArray<Rotation> rotations;
        public ComponentDataArray<Scale> scales;
        public ComponentDataArray<Tint> tints;

        public void Execute(int index)
        {
            //float radDeg = 180 / Mathf.PI;

            int i = index / CubesPerRing;
            int j = index % CubesPerRing;

            float angle1 = i * 2 * Mathf.PI / Rings;

            quaternion ringRot = quaternion.euler(0, angle1 /** radDeg*/, 0);
            
            float angle2 = j * 2 * Mathf.PI / CubesPerRing;
            float angle3 = angle1 + angle2 + Time;
            float d = ShapeRadius + SideSize * math.sin(ShapeSides * angle3);

            positions[index] = new Position(new float3(
                (Radius + d * math.cos(angle3 + angle1)) * math.sin(angle1),
                d * math.sin(angle3 + angle1),
                (Radius + d * math.cos(angle3 + angle1)) * math.cos(angle1)));

            rotations[index] = new Rotation(math.mul(ringRot, quaternion.euler(angle3 /** radDeg*/, 0, 0)));

            scales[index] = new Scale(new float3(1, 1, 1) * (1.5f + 1.5f * math.sin(angle1 * 2 + angle2 + Time * 10)));

            tints[index] = new Tint(2f * math.cos(angle1 + angle2 + Time / 20));
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
            CubesPerRing = Bootstrap.Instance.cubesPerRing,
            positions = entitiesTransformGroup.positions,
            rotations = entitiesTransformGroup.rotations,
            scales = entitiesTransformGroup.scales,
            tints = entitiesTransformGroup.tints
        };
        return job.Schedule(entitiesTransformGroup.Length, 64, inputDeps);
    }
}
