using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;

using MeshBuilder;
using TileData = MeshBuilder.Tile.Data;


namespace MeshBuilderTest
{
    public static class VolumeFiller
    {
        static public void SetLayer(this Volume<TileData> volume, byte value, int y)
        {
            SetCube(volume, value, 0, y, 0, volume.XLength, 1, volume.ZLength);
        }

        static public void SetRect(this Volume<TileData> volume, byte value, int3 pos, int width, int height)
        {
            SetCube(volume, value, pos.x, pos.y, pos.z, width, 1, height);
        }

        static public void SetCube(this Volume<TileData> volume, byte value, int3 pos, int3 size)
        {
            SetCube(volume, value, pos.x, pos.y, pos.z, size.x, size.y, size.z);
        }

        static public void SetCube(this Volume<TileData> volume, byte value, int posX, int posY, int posZ, int xLength, int yLength, int zLength)
        {
            int startX = Mathf.Clamp(posX, 0, volume.XLength - 1);
            int startY = Mathf.Clamp(posY, 0, volume.YLength - 1);
            int startZ = Mathf.Clamp(posZ, 0, volume.ZLength - 1);
            int endX = Mathf.Clamp(posX + xLength, startX, volume.XLength);
            int endY = Mathf.Clamp(posY + yLength, startY, volume.YLength);
            int endZ = Mathf.Clamp(posZ + zLength, startZ, volume.ZLength);
            for (int y = startY; y < endY; ++y)
            {
                for (int z = startZ; z < endZ; ++z)
                {
                    for (int x = startX; x < endX; ++x)
                    {
                        Set(volume, value, x, y, z);
                    }
                }
            }
        }

        static public void Set(this Volume<TileData> volume, byte value, int3 pos)
        {
            Set(volume, value, pos.x, pos.y, pos.z);
        }

        static public void Set(this Volume<TileData> volume, byte value, int x, int y, int z)
        {
            if (IsInBounds(volume, x, y, z))
            {
                int i = ToIndex(volume, x, y, z);
                volume[i] = new TileData { themeIndex = value };
            }
        }

        static private int ToIndex<T>(this Volume<T> volume, int3 c) where T : struct
        {
            return Extents.IndexFromCoord(c, volume.XLength * volume.ZLength, volume.XLength);
        }

        static private int ToIndex<T>(this Volume<T> volume, int x, int y, int z) where T : struct
        {
            return Extents.IndexFromCoord(x, y, z, volume.XLength * volume.ZLength, volume.XLength);
        }

        static private bool IsInBounds(int val, int count)
        {
            return val >= 0 && val < count;
        }

        static private bool IsInBounds<T>(this Volume<T> volume, int3 c) where T : struct
        {
            return IsInBounds(volume, c.x, c.y, c.z);
        }

        static private bool IsInBounds<T>(this Volume<T> volume, int x, int y, int z) where T : struct
        {
            return  IsInBounds(x, volume.XLength) && 
                    IsInBounds(y, volume.YLength) && 
                    IsInBounds(z, volume.ZLength);
        }
    }
}
