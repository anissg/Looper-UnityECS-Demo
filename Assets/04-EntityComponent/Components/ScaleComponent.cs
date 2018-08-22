using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Scale : IComponentData
{
    public float3 Value;

    public Scale(float3 scale)
    {
        Value = scale;
    }
}

public class ScaleComponent : ComponentDataWrapper<Scale> { }