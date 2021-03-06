﻿using System.Collections;

using Unity.Mathematics;
using UnityEngine;

using MeshBuilderTest;
using MeshBuilder;

using DataVolume = MeshBuilder.Volume<MeshBuilder.Tile.Data>;
using Extents = MeshBuilder.Extents;

using RenderInfo = MeshBuilder.MeshBuilderDrawer.RenderInfo;
using Dir = MeshBuilder.Tile.Direction;

public class AdjacentChunksScene : MonoBehaviour
{
    private const int GroundIndex = 1;
    private const int WaterIndex = 2;
    private const int RockIndex = 3;

    [Header("render info")]
    [SerializeField]
    private RenderInfo ground = null;
    [SerializeField]
    private RenderInfo grass = null;
    [SerializeField]
    private RenderInfo water = null;
    [SerializeField]
    private RenderInfo rock = null;

    [Header("generation info")]
    [SerializeField]
    private TileThemePalette palette = null;

    [SerializeField]
    private Vector3 cellSize = new Vector3(1, 1, 1);

    [SerializeField]
    private Texture2D groundHeightMap = null;

    [SerializeField]
    private float groundMaxHeight = 1f;

    private Extents dataExtents;
    private DataVolume dataVolume1;
    private DataVolume dataVolume2;

    private Chunk[] chunks;

    void Awake()
    {
        dataExtents = new Extents(16, 4, 16);
        dataVolume1 = new DataVolume(dataExtents);
        FillData1(dataVolume1);

        dataVolume2 = new DataVolume(dataExtents);
        FillData2(dataVolume2);

        var info = Chunk.CreateInfo(palette, cellSize, groundHeightMap, groundMaxHeight, ground, grass, water, rock);

        float offset = 2f;

        chunks = new Chunk[4];
        chunks[0] = new Chunk(new float3(0, 0, 0), new float3(0 - offset, 0, 0-offset), transform, dataVolume1, info);
        chunks[1] = new Chunk(new float3(0, 0, 16), new float3(0 - offset, 0, 16 + offset), transform, dataVolume2, info);
        chunks[2] = new Chunk(new float3(16, 0, 0), new float3(16 + offset, 0, 0 - offset), transform, dataVolume2, info);
        chunks[3] = new Chunk(new float3(16, 0, 16), new float3(16 + offset, 0, 16 + offset), transform, dataVolume1, info);
    
        foreach (var chunk in chunks)
        {
            foreach (var adj in chunks)
            {
                if (chunk == adj) continue;

                byte dir = 0;
                if (adj.Position.x > chunk.Position.x) dir |= (byte)Dir.XPlus;
                if (adj.Position.x < chunk.Position.x) dir |= (byte)Dir.XMinus;
                if (adj.Position.z > chunk.Position.z) dir |= (byte)Dir.ZPlus;
                if (adj.Position.z < chunk.Position.z) dir |= (byte)Dir.ZMinus;
                chunk.SetAdjacent(adj.Data, dir);
            }
            chunk.StartBuilding();
        }

        StartCoroutine(CompleteMesh());
    }

    public void OnOffsetChange(float value)
    {
        foreach(var chunk in chunks)
        {
            chunk.OffsetRatio = value;
        }
    }

    private IEnumerator CompleteMesh()
    {
        yield return new WaitForEndOfFrame();

        foreach (var chunk in chunks)
        {
            chunk.Complete();
        }

        yield return null;
    }

    private void OnDestroy()
    {
        dataVolume1.Dispose();
        dataVolume2.Dispose();
        foreach(var chunk in chunks)
        {
            chunk.Dispose();
        }
    }

    private void FillData1(DataVolume data)
    {
        data.SetLayer(GroundIndex, 0);
        data.SetRect(GroundIndex, P(2, 1, 2), 3, 11);
        data.SetRect(GroundIndex, P(12, 1, 2), 2, 7);
        data.SetCube(GroundIndex, P(4, 0, 3), Column);
        data.SetCube(GroundIndex, P(12, 0, 4), Column);
        data.SetCube(GroundIndex, P(2, 0, 10), Column);

        data.SetRect(WaterIndex, P(10, 0, 10), 4, 5);
        data.SetRect(WaterIndex, P(6, 0, 7), 5, 4);
        data.SetRect(WaterIndex, P(6, 0, 2), 1, 8);
        data.SetRect(WaterIndex, P(10, 0, 3), 1, 8);
        data.SetRect(WaterIndex, P(14, 0, 15), 2, 1);

        data.SetCube(GroundIndex, P(8, 0, 10), Column);

        data.SetCube(RockIndex, P(2, 0, 1), S(4, 2, 2));
        data.SetCube(RockIndex, P(2, 0, 1), S(2, 4, 1));
        data.SetCube(RockIndex, P(12, 0, 5), S(4, 2, 4));
        data.SetCube(RockIndex, P(12, 0, 7), S(4, 4, 2));
        data.SetCube(RockIndex, P(12, 0, 15), Column);
        data.SetCube(RockIndex, P(0, 0, 0), S(1, 2, 2));
    }

    private void FillData2(DataVolume data)
    {
        data.SetLayer(GroundIndex, 0);
        data.SetRect(GroundIndex, P(12, 1, 2), 2, 7);
        data.SetCube(GroundIndex, P(4, 0, 3), Column);
        data.SetCube(GroundIndex, P(12, 0, 4), Column);
        data.SetCube(GroundIndex, P(2, 0, 10), Column);

        data.SetRect(WaterIndex, P(10, 0, 10), 4, 5);
        data.SetRect(WaterIndex, P(2, 0, 7), 5, 4);
        data.SetRect(WaterIndex, P(6, 0, 2), 1, 8);
        data.SetRect(WaterIndex, P(10, 0, 5), 1, 8);
        data.SetRect(WaterIndex, P(0, 0, 14), 1, 2);

        data.SetCube(GroundIndex, P(8, 0, 10), Column);

        data.SetCube(RockIndex, P(2, 0, 1), S(4, 2, 2));
        data.SetCube(RockIndex, P(2, 0, 1), S(2, 4, 1));
        data.SetCube(RockIndex, P(0, 0, 5), S(4, 2, 4));
        data.SetCube(RockIndex, P(12, 0, 7), S(4, 4, 2));
        data.SetCube(RockIndex, P(12, 0, 15), Column);
        data.SetCube(RockIndex, P(15, 0, 0), Column);
    }

    private int3 Column = new int3(1, 4, 1);

    private static int3 P(int x, int y, int z) { return new int3(x, y, z); }
    private static int3 S(int x, int y, int z) { return new int3(x, y, z); }

    private class Chunk
    {
        private MeshBuilderDrawerComponent drawerComponent;
        public DataVolume Data { get; private set; }
        public bool IsBuilding { get; private set; }
        public float3 Position { get => drawerComponent.transform.position; }

        public Transform Transform { get; private set; }

        private TileMesher3D ground;

        private float3 originalPosition;
        private float3 offsetPosition;

        public float OffsetRatio { set { drawerComponent.transform.position = Vector3.Lerp(originalPosition, offsetPosition, value); } }

        public Chunk(float3 pos, float3 offsetPos, Transform root, DataVolume data, CreationInfo info)
        {
            Data = data;

            originalPosition = pos;
            offsetPosition = offsetPos;

            Transform = new GameObject("chunk").transform;
            Transform.SetParent(root);
            Transform.position = pos;

            drawerComponent = Transform.gameObject.AddComponent<MeshBuilderDrawerComponent>();

            var groundMesher = new TileMesher3D();
            groundMesher.Init(data, 0, info.palette, info.cellSize,
                new TileMesher3D.Settings()
                {
                    filledBoundaries = Dir.None,
                    skipDirections = Dir.YAxis,
                });
            Add(groundMesher, info.ground);

            var topGroundMesher = new GridMesher();
            topGroundMesher.Init(data, GroundIndex, info.cellSize, 3, GridMesher.UVMode.Normalized);
            topGroundMesher.InitHeightMapScaleFromHeightAvgOffset(info.groundHeightMap, info.groundMaxHeight);
            Add(topGroundMesher, info.grass);

            var waterMesher = new GridMesher();
            waterMesher.Init(data, WaterIndex, info.cellSize, 1, GridMesher.UVMode.NoScaling, new float3(0, -0.2f, 0));
            Add(waterMesher, info.water);

            var rockMesher = new TileMesher3D();
            rockMesher.Init(data, 1, info.palette, info.cellSize);
            Add(rockMesher, info.rock);
        }

        private void Add(Builder builder, RenderInfo renderInfo)
        {
            drawerComponent.AddDrawer(new MeshBuilderDrawer(renderInfo, builder));
        }
        
        public void SetAdjacent(DataVolume data, byte direction)
        {
            foreach (var drawer in drawerComponent.Drawers)
            {
                var mesher = drawer.MeshBuilder as TileMesher3D;
                if (mesher != null)
                {
                    mesher.SetAdjacent(data, direction);
                }
            }
        }

        public void StartBuilding()
        {
            foreach (var drawer in drawerComponent.Drawers)
            {
                drawer.StartBuilder();
            }

            IsBuilding = true;
        }

        public void Complete()
        {
            foreach (var drawer in drawerComponent.Drawers)
            {
                if (drawer.IsBuilderGenerating)
                {
                    drawer.CompleteBuilder();
                }
            }
            IsBuilding = false;
        }

        public void Dispose()
        {
            foreach (var drawer in drawerComponent.Drawers)
            {
                drawer.Dispose();
            }
            drawerComponent.RemoveAll();
        }

        public static CreationInfo CreateInfo(TileThemePalette palette, float3 cellSize, Texture2D groundHeightmap, float groundMaxHeight, RenderInfo ground, RenderInfo grass, RenderInfo water, RenderInfo rock)
        {
            return new CreationInfo()
            {
                palette = palette,
                cellSize = cellSize,
                groundHeightMap = groundHeightmap,
                groundMaxHeight = groundMaxHeight,
                ground = ground,
                grass = grass,
                water = water,
                rock = rock
            };
        }

        internal class CreationInfo
        {
            public RenderInfo ground = null;
            public RenderInfo grass = null;
            public RenderInfo water = null;
            public RenderInfo rock = null;

            public TileThemePalette palette;

            public float3 cellSize;
            public Texture2D groundHeightMap;
            public float groundMaxHeight;
        }
    }
}
