Mesh Builder Examples
--------

Example repository for [MeshBuilder](https://github.com/hbence/MeshBuilder/). 

Scenes
=========

1. **3D Template Pieces**

    ![3D Template Pieces](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/01.jpg)

    A simple scene rendering a volume with the template mesh pieces.

2. **Simple Chunk**

    ![Simple Chunk](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/02.jpg)

    A chunk rendering a volume with three different tiles. The ground is a combination of a heightmapped grid and tiles, the water is generated as a grid, the rocks are made from tiles. The tiles are rendered with simple MeshRenderers on separate game objects.

3. **Chunk From Code**

    ![Chunk From Code](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/03.jpg)

    The same scene as the previous, except everything is built and rendered from code (with Graphics.DrawMesh), there are no separate game objects.

4. **Adjacent Chunks**

    ![Adjacent Chunks](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/04.jpg)

    Adjacent chunks are aware of each other so the are built correctly. The chunks use separate game objects.

5. **Grid Generation / Frustum Culling**

    ![Frustum Culling](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/05.jpg)

    A bigger grid of adjacent chunks. Chunks are turned off outside the camera frustum.

6. **Lattice Deformation**

    ![Lattice Deformation](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/06.jpg)

    A generated mesh is deformed by a lattice.

7. **Terrain Lattice**

    ![Terrain Lattice](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/07.jpg)

    A lattice deformation applied to a chunk. There is a slider to see the effect of the deformation.

8. **Merge Test**

    ![Merge Test](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/08.jpg)

    Some game objects are randomly placed / enabled. Then merged into a single mesh.

9. **Nine Scale Mesh Drawer**

    ![Nine Scale Mesh Drawer](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/09.jpg)

    Two examples for the nine scaler, they can be resized with sliders. Instanced rendering can be switch on.

10. **Nine Scale Mesh Building**

    ![Nine Scale Mesh Building](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/10.jpg)

    A nine scaler fills out the target box with a merged mesh.

11. **2D Templates**

    ![2D Templates](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/11.jpg)

    A simple scene rendering the 2d piece templates.

12. **Dungeon Scene**

    ![Dungeon Scene](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/12.jpg)

    A simple scene with multiple tiles using the 2d tile meshers.

13. **Sample Editor**

    ![Sample Editor](https://github.com/hbence/MeshBuilderExamples/blob/readme/img/13.jpg)

    A barebones editor using 2D Tile Meshers, Grid Meshers, a randomized Lattice Modifier for the tiles, MeshCombinationBuilder to merge the placed objects.


