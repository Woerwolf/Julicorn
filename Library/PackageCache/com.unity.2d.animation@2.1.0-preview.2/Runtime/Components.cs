using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

namespace UnityEngine.Experimental.U2D.Animation
{
    [InternalBufferCapacity(0)]
    struct Vertex : IBufferElementData
    {
        public float3 Value;
    }

    [InternalBufferCapacity(0)]
    struct BoneTransform : IBufferElementData
    {
        public float4x4 Value;
    }

    struct WorldToLocal : IComponentData
    {
        public float4x4 Value;
    }

    struct SpriteComponent : ISharedComponentData
    {
        public Sprite Value;
    }

    struct BoundsComponent : IComponentData
    {
        public float4 center;
        public float4 extents;
    }
}