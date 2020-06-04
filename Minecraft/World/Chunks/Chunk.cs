﻿using System.Collections.Generic;
using System;

namespace Minecraft
{
    class Chunk
    {
        public Dictionary<Vector3i, BlockState> TickableBlocks { get; set; } = new Dictionary<Vector3i, BlockState>();
        public Dictionary<Vector3i, BlockState> LightSourceBlocks { get; set; } = new Dictionary<Vector3i, BlockState>();
        public Section[] Sections { get; set; } = new Section[Constants.NUM_SECTIONS_IN_CHUNKS];
        public int GridX { get; private set; }
        public int GridZ { get; private set; }
        public LightMap LightMap { get; private set; } = new LightMap();
        public int[,] TopMostBlocks { get; private set; } = new int[16, 16];

        public Chunk(int gridX, int gridZ)
        {
            GridX = gridX;
            GridZ = gridZ;
        }

        public override string ToString()
        {
            return "Chunk[" + GridX + "," + GridZ + "]";
        }

        public void Tick(float deltaTime, World world)
        {
            foreach(KeyValuePair<Vector3i, BlockState> kp in TickableBlocks)
            {
                kp.Value.GetBlock().OnTick(kp.Value, world, kp.Key, deltaTime);
            }
        }

        public BlockState GetBlockAt(int localX, int worldY, int localZ)
        {
            return GetBlockAt(new Vector3i(localX, worldY, localZ));
        }

        public BlockState GetBlockAt(Vector3i localPos)
        {
            if(localPos.Y < 0 || localPos.Y >= Constants.MAX_BUILD_HEIGHT)
            {
                return Blocks.AirState;
            }

            int sectionHeight = localPos.Y / 16;
            if(Sections[sectionHeight] == null)
            {
                return Blocks.AirState;
            }

            BlockState block = Sections[sectionHeight].GetBlockAt(localPos.X, localPos.Y & 15, localPos.Z);
            if(block == null)
            {
                return Blocks.AirState;
            }
            return block;
        }

        public void RemoveBlockAt(int localX, int worldY, int localZ)
        {
            if(worldY < 0 || worldY > Constants.MAX_BUILD_HEIGHT - 1)
            {
                throw new ArgumentOutOfRangeException("Removing block at y level " + worldY + " in chunk (" + GridX + ", " + GridZ + ")");
            }

            int sectionHeight = worldY / 16;
            int sectionLocalY = worldY - sectionHeight * 16;
            Vector3i blockPos = new Vector3i(localX + GridX * 16, worldY, localZ + GridZ * 16);

            TopMostBlocks[localX, localZ] = FindNewTopMostBlockAt(localX, localZ);

            Sections[sectionHeight].RemoveBlockAt(localX, sectionLocalY, localZ);
            TickableBlocks.Remove(blockPos);
            LightSourceBlocks.Remove(blockPos);
        }

        private int FindNewTopMostBlockAt(int localX, int localZ)
        {
            int previousTopY = TopMostBlocks[localX, localZ];
            for(int y = previousTopY - 1; y >= 0; y--)
            {
                if(GetBlockAt(localX, y, localZ).GetBlock() != Blocks.Air)
                {
                    return y;
                }
            }
            return 0;
        }

        public void AddBlockAt(int localX, int worldY, int localZ, BlockState blockstate)
        {
            if(blockstate.GetBlock() == Blocks.Air)
                throw new ArgumentException("Cannot add air.");

            if (worldY < 0 || worldY > Constants.MAX_BUILD_HEIGHT - 1)
            {
                throw new ArgumentOutOfRangeException("Adding block at y level " + worldY + " in chunk (" + GridX + ", " + GridZ + ")");
            }

            int sectionHeight = worldY / 16;
            if(Sections[sectionHeight] == null)
            {
                Sections[sectionHeight] = new Section(GridX, GridZ, (byte)sectionHeight);
            }

            Vector3i blockPos = new Vector3i(localX + GridX * 16, worldY, localZ + GridZ * 16);
            Block block = blockstate.GetBlock();
            int sectionLocalY = worldY - sectionHeight * 16;
            if(block == Blocks.Air)
            {
                throw new ArgumentException("Can't add air to remove. Call removeblock instead!");
            }

            Sections[sectionHeight].AddBlockAt(localX, sectionLocalY, localZ, blockstate);

            if (block.IsTickable)
            {
                if(TickableBlocks.ContainsKey(blockPos))
                {
                    TickableBlocks.Remove(blockPos);
                }
                TickableBlocks.Add(blockPos, blockstate);
            }
            if(blockstate is ILightSource)
            {
                if(LightSourceBlocks.ContainsKey(blockPos))
                {
                    LightSourceBlocks.Remove(blockPos);
                }
                LightSourceBlocks.Add(blockPos, blockstate);
            }

            if(TopMostBlocks[localX, localZ] < worldY)
            {
                TopMostBlocks[localX, localZ] = worldY;
            }
        }

        public uint GetLowestEmptySectionAfterEachOtherFromTop()
        {
            uint lowestSection = Constants.NUM_SECTIONS_IN_CHUNKS - 1;
            Section section = null;
            for(int i = Constants.NUM_SECTIONS_IN_CHUNKS - 1; i >= 0; i--)
            {
                section = Sections[i];
                if(section == null || (section != null && section.IsFullAir))
                {
                    lowestSection = (uint)i;
                } else
                {
                    break;
                }                  
            }
            return lowestSection;
        }

        public int GetPayloadSize()
        {
            int size = 0;
            for(int i = 0; i < Constants.NUM_SECTIONS_IN_CHUNKS; i++)
            {
                Section section = Sections[i];
                size++;
                if(section == null)
                {
                    continue;
                }
                for(int x = 0; x < 16; x++)
                {
                    for(int y = 0; y < 16; y++)
                    {
                        for(int z = 0; z < 16; z++)
                        {
                            BlockState state = section.GetBlockAt(x, y, z);
                            size += 2;
                            if(state != null)
                            {
                                size += state.PayloadSize();
                            }
                        }
                    }
                }
            }
            return size;
        }
    }
}
