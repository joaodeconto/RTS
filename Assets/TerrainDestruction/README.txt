Terrain Destruction
-------------------


Step One:

Assign the TerrainDeformer script to your terrain.

Step Two:

Assign the crater texture to the crater field on the TerrainDeformer
component.

Step Three:

Call TerrainDeformer.Damage(Vector3 position, float depth) to create a crater
on your terrain!

Tips:

- Always use a dirt or rock type texture in the first slot on your terrain
textures.

- Always use the same resolution for your heightmap and control texture.
