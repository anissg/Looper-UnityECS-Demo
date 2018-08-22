using System;
using Unity.Entities;

[Serializable]
public struct Tint : IComponentData
{
    public float Value;

    public Tint(float tint)
    {
        Value = tint;
    }
}

public class TintComponent : ComponentDataWrapper<Tint> { }