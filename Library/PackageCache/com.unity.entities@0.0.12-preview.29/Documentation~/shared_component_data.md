---
uid: ecs-shared-component-data
---
# Shared ComponentData

`IComponentData` is appropriate for data that varies between entities, such as storing a `World` position. `ISharedComponentData` is useful when many entities have something in common, for example in the `Boid` demo we instantiate many entities from the same [Prefab](https://docs.unity3d.com/Manual/Prefabs.html) and thus the `RenderMesh` between many `Boid` entities is exactly the same. 

```cs
[System.Serializable]
public struct RenderMesh : ISharedComponentData
{
    public Mesh                 mesh;
    public Material             material;

    public ShadowCastingMode    castShadows;
    public bool                 receiveShadows;
}
```

The great thing about `ISharedComponentData` is that there is literally zero memory cost on a per entity basis.

We use `ISharedComponentData` to group all entities using the same `InstanceRenderer` data together and then efficiently extract all matrices for rendering. The resulting code is simple & efficient because the data is laid out exactly as it is accessed.

- `RenderMeshSystemV2` (see file:  _Packages/com.unity.entities/Unity.Rendering.Hybrid/RenderMeshSystemV2.cs_)

## Some important notes about SharedComponentData:

- Entities with the same `SharedComponentData` are grouped together in the same [Chunks](chunk_iteration.md). The index to the `SharedComponentData` is stored once per `Chunk`, not per entity. As a result `SharedComponentData` have zero memory overhead on a per entity basis. 
- Using `ComponentGroup` we can iterate over all entities with the same type.
- Additionally we can use `ComponentGroup.SetFilter()` to iterate specifically over entities that have a specific `SharedComponentData` value. Due to the data layout this iteration has low overhead.
- Using `EntityManager.GetAllUniqueSharedComponents` we can retrieve all unique `SharedComponentData` that is added to any alive entities.
- `SharedComponentData` are automatically [reference counted](https://en.wikipedia.org/wiki/Reference_counting).
- `SharedComponentData` should change rarely. Changing a `SharedComponentData` involves using [memcpy](https://msdn.microsoft.com/en-us/library/aa246468(v=vs.60).aspx) to copy all `ComponentData` for that entity into a different `Chunk`.

