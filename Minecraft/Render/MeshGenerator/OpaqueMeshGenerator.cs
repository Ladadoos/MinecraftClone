﻿using OpenTK;
using System;
using System.Linq;

namespace Minecraft
{
    class OpaqueMeshGenerator : MeshGenerator
    {
        private const uint staticTopLight = 15;
        private const uint staticBottomLight = 9;
        private const uint staticSideXLight = 13;
        private const uint staticSideZLight = 11;

        public OpaqueMeshGenerator(BlockModelRegistry blockModelRegistry) : base (blockModelRegistry)
        {           
        }

        protected override ChunkBufferLayout GenerateMesh(World world, Chunk chunk)
        {
            world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ), out Chunk cXNeg);
            world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ), out Chunk cXPos);
            world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ - 1), out Chunk cZNeg);
            world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ + 1), out Chunk cZPos);

            for (int sectionHeight = 0; sectionHeight < chunk.Sections.Length; sectionHeight++)
            {
                Section section = chunk.Sections[sectionHeight];
                if (section == null)
                {
                    continue;
                }

                for (int localX = 0; localX < 16; localX++)
                {
                    for (int localZ = 0; localZ < 16; localZ++)
                    {
                        for (int sectionLocalY = 0; sectionLocalY < 16; sectionLocalY++)
                        {
                            BlockState state = section.GetBlockAt(localX, sectionLocalY, localZ);
                            if (state == null)
                            {
                                continue;
                            }

                            if (!blockModelRegistry.Models.TryGetValue(state.GetBlock(), out BlockModel blockModel))
                            {
                                throw new System.Exception("Could not find model for: " + state.GetBlock().GetType());
                            }

                            Vector3i localChunkBlockPos = new Vector3i(localX, sectionLocalY + sectionHeight * 16, localZ);
                            Vector3i globalBlockPos = new Vector3i(localX + chunk.GridX * 16, sectionLocalY + sectionHeight * 16, localZ + chunk.GridZ * 16);

                            uint averageRed = 0;
                            uint averageBlue = 0;
                            uint averageGreen = 0;
                            int numAverages = 0;

                            if (ShouldAddEastFaceOfBlock(cXPos, section, localX, sectionLocalY, localZ))
                            {
                                Light light = new Light();

                                if(localChunkBlockPos.X + 1 > 15)
                                {
                                    if(cXPos != null)
                                    {
                                        light = cXPos.LightMap.GetLightColorAt(0, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z);
                                        light.SetSunlight(cXPos.LightMap.GetSunLightIntensityAt(0, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z));
                                    }
                                } else
                                {
                                    light = chunk.LightMap.GetLightColorAt((uint)localChunkBlockPos.X + 1, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z);
                                    light.SetSunlight(chunk.LightMap.GetSunLightIntensityAt((uint)localChunkBlockPos.X + 1, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z));
                                }

                                light.SetBrightness(staticSideZLight);

                                averageRed += light.GetRedChannel();
                                averageGreen += light.GetGreenChannel();
                                averageBlue += light.GetBlueChannel();
                                numAverages++;

                                BuildMeshForSide(Direction.Right, state, localChunkBlockPos, globalBlockPos, blockModel, light);
                            }
                            if (ShouldAddWestFaceOfBlock(cXNeg, section, localX, sectionLocalY, localZ))
                            {
                                Light light = new Light();

                                if(localChunkBlockPos.X - 1 < 0)
                                {
                                    if(cXNeg != null)
                                    {
                                        light = cXNeg.LightMap.GetLightColorAt(15, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z);
                                        light.SetSunlight(cXNeg.LightMap.GetSunLightIntensityAt(15, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z));
                                    }
                                } else
                                {
                                    light = chunk.LightMap.GetLightColorAt((uint)localChunkBlockPos.X - 1, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z);
                                    light.SetSunlight(chunk.LightMap.GetSunLightIntensityAt((uint)localChunkBlockPos.X - 1, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z));
                                }

                                light.SetBrightness(staticSideZLight);

                                averageRed += light.GetRedChannel();
                                averageGreen += light.GetGreenChannel();
                                averageBlue += light.GetBlueChannel();
                                numAverages++;

                                BuildMeshForSide(Direction.Left, state, localChunkBlockPos, globalBlockPos, blockModel, light);
                            }
                            if (ShouldAddSouthFaceOfBlock(cZNeg, section, localX, sectionLocalY, localZ))
                            {
                                Light light = new Light();

                                if(localChunkBlockPos.Z - 1 < 0)
                                {
                                    if(cZNeg != null)
                                    {
                                        light = cZNeg.LightMap.GetLightColorAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, 15);
                                        light.SetSunlight(cZNeg.LightMap.GetSunLightIntensityAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, 15));
                                    }
                                } else
                                {
                                    light = chunk.LightMap.GetLightColorAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z - 1);
                                    light.SetSunlight(chunk.LightMap.GetSunLightIntensityAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z - 1));
                                }
                                
                                light.SetBrightness(staticSideXLight);
                                
                                averageRed += light.GetRedChannel();
                                averageGreen += light.GetGreenChannel();
                                averageBlue += light.GetBlueChannel();
                                numAverages++;

                                BuildMeshForSide(Direction.Back, state, localChunkBlockPos, globalBlockPos, blockModel, light);
                            }
                            if (ShouldAddNorthFaceOfBlock(cZPos, section, localX, sectionLocalY, localZ))
                            {
                                Light light = new Light();

                                if(localChunkBlockPos.Z + 1 > 15)
                                {
                                    if(cZPos != null)
                                    {
                                        light = cZPos.LightMap.GetLightColorAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, 0);
                                        light.SetSunlight(cZPos.LightMap.GetSunLightIntensityAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, 0));
                                    }
                                } else
                                {
                                    light = chunk.LightMap.GetLightColorAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z + 1);
                                    light.SetSunlight(chunk.LightMap.GetSunLightIntensityAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z + 1));
                                }

                                light.SetBrightness(staticSideXLight);

                                averageRed += light.GetRedChannel();
                                averageGreen += light.GetGreenChannel();
                                averageBlue += light.GetBlueChannel();
                                numAverages++;

                                BuildMeshForSide(Direction.Front, state, localChunkBlockPos, globalBlockPos, blockModel, light);
                            }
                            if (ShouldAddTopFaceOfBlock(chunk, section, localX, sectionLocalY, localZ))
                            {
                                uint chunkLocalX = (uint)localChunkBlockPos.X;
                                uint worldY = (uint)Math.Min(localChunkBlockPos.Y + 1, 255);
                                uint chunkLocalZ = (uint)localChunkBlockPos.Z;

                                Light light = chunk.LightMap.GetLightColorAt(chunkLocalX, worldY, chunkLocalZ);
                                light.SetBrightness(staticTopLight);
                                light.SetSunlight(chunk.LightMap.GetSunLightIntensityAt(chunkLocalX, worldY, chunkLocalZ));

                                averageRed += light.GetRedChannel();
                                averageGreen += light.GetGreenChannel();
                                averageBlue += light.GetBlueChannel();
                                numAverages++;

                                BuildMeshForSide(Direction.Top, state, localChunkBlockPos, globalBlockPos, blockModel, light);
                            }
                            if (ShouldAddBottomFaceOfBlock(chunk, section, localX, sectionLocalY, localZ))
                            {
                                uint chunkLocalX = (uint)localChunkBlockPos.X;
                                uint worldY = (uint)Math.Max(localChunkBlockPos.Y - 1, 0);
                                uint chunkLocalZ = (uint)localChunkBlockPos.Z;

                                Light light = chunk.LightMap.GetLightColorAt(chunkLocalX, worldY, chunkLocalZ);
                                light.SetBrightness(staticBottomLight);
                                light.SetSunlight(chunk.LightMap.GetSunLightIntensityAt(chunkLocalX, worldY, chunkLocalZ));

                                averageRed += light.GetRedChannel();
                                averageGreen += light.GetGreenChannel();
                                averageBlue += light.GetBlueChannel();
                                numAverages++;

                                BuildMeshForSide(Direction.Bottom, state, localChunkBlockPos, globalBlockPos, blockModel, light);
                            }

                            Light lightAlwaysyVisibleFaces = new Light();
                            if(!state.GetBlock().IsOpaque)
                            {
                                lightAlwaysyVisibleFaces = chunk.LightMap.GetLightColorAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z);
                            } else if(numAverages != 0)
                            {
                                lightAlwaysyVisibleFaces.SetRedChannel((uint)(averageRed / numAverages));
                                lightAlwaysyVisibleFaces.SetGreenChannel((uint)(averageGreen / numAverages));
                                lightAlwaysyVisibleFaces.SetBlueChannel((uint)(averageBlue / numAverages));
                            }
                            lightAlwaysyVisibleFaces.SetSunlight(chunk.LightMap.GetSunLightIntensityAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z));
                            lightAlwaysyVisibleFaces.SetBrightness(15);

                            BlockFace[] faces = blockModel.GetAlwaysVisibleFaces(state, globalBlockPos);                           
                            if(blockModel.DoubleSidedFaces)
                            {
                                AddFacesToMeshDualSided(faces, localChunkBlockPos, lightAlwaysyVisibleFaces);
                            } else
                            {
                                AddFacesToMeshFromFront(faces, localChunkBlockPos, lightAlwaysyVisibleFaces);
                            }
                        }
                    }
                }
            }

            return new ChunkBufferLayout()
            {
                positions = vertexPositions.ToArray(),
                textureCoordinates = textureUVs.ToArray(),
                lights = illuminations.ToArray(),
                normals = normals.ToArray(),
                indicesCount = indicesCount
            };                   
        }

        private void BuildMeshForSide(Direction direction, BlockState state, Vector3i chunkLocalPos, Vector3i globalPos, BlockModel model, Light light)
        {
            BlockFace[] faces = model.GetPartialVisibleFaces(state, globalPos, direction);
            AddFacesToMeshFromFront(faces, chunkLocalPos, light);
        }

        private bool ShouldAddWestFaceOfBlock(Chunk westChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localX - 1 < 0)
            {
                if (westChunk == null)
                    return true;

                Section westSection = westChunk.Sections[currentSection.Height];
                if (westSection == null)
                    return true;

                BlockState blockWest = westSection.GetBlockAt(16 - 1, localY, localZ);
                if (blockWest == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockWest.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Right);
            } else
            {
                BlockState blockWest = currentSection.GetBlockAt(localX - 1, localY, localZ);
                if (blockWest == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockWest.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Right);
            }
            return false;
        }

        private bool ShouldAddEastFaceOfBlock(Chunk eastChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localX + 1 >= 16)
            {
                if (eastChunk == null)
                    return true;

                Section eastSection = eastChunk.Sections[currentSection.Height];
                if (eastSection == null)
                    return true;

                BlockState blockEast = eastSection.GetBlockAt(0, localY, localZ);
                if (blockEast == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockEast.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Left);
            } else
            {
                BlockState blockEast = currentSection.GetBlockAt(localX + 1, localY, localZ);
                if (blockEast == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockEast.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Left);
            }
            return false;
        }

        private bool ShouldAddNorthFaceOfBlock(Chunk northChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localZ + 1 >= 16)
            {
                if (northChunk == null)
                    return true;

                Section northSection = northChunk.Sections[currentSection.Height];
                if (northSection == null)
                    return true;

                BlockState blockNorth = northSection.GetBlockAt(localX, localY, 0);
                if (blockNorth == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockNorth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Back);
            } else
            {
                BlockState blockNorth = currentSection.GetBlockAt(localX, localY, localZ + 1);
                if (blockNorth == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockNorth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Back);
            }
            return false;
        }

        private bool ShouldAddSouthFaceOfBlock(Chunk southChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localZ - 1 < 0)
            {
                if (southChunk == null)
                    return true;

                Section southSection = southChunk.Sections[currentSection.Height];
                if (southSection == null)
                    return true;

                BlockState blockSouth = southSection.GetBlockAt(localX, localY, 16 - 1);
                if (blockSouth == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockSouth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Front);
            } else
            {
                BlockState blockSouth = currentSection.GetBlockAt(localX, localY, localZ - 1);
                if (blockSouth == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockSouth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Front);
            }
            return false;
        }

        private bool ShouldAddTopFaceOfBlock(Chunk currentChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localY + 1 >= 16)
            {
                if (currentSection.Height == 16 - 1)
                    return true;

                Section sectionAbove = currentChunk.Sections[currentSection.Height + 1];
                if (sectionAbove == null)
                    return true;

                BlockState blockAbove = sectionAbove.GetBlockAt(localX, 0, localZ);
                if (blockAbove == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockAbove.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Bottom);
            } else
            {
                BlockState blockAbove = currentSection.GetBlockAt(localX, localY + 1, localZ);
                if (blockAbove == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockAbove.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Bottom);
            }
            return false;
        }

        private bool ShouldAddBottomFaceOfBlock(Chunk currentChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localY - 1 < 0)
            {
                if (currentSection.Height == 0)
                    return true;

                Section sectionBelow = currentChunk.Sections[currentSection.Height - 1];
                if (sectionBelow == null)
                    return true;

                BlockState blockBottom = sectionBelow.GetBlockAt(localX, 16 - 1, localZ);
                if (blockBottom == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockBottom.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Top);
            } else
            {
                BlockState blockBottom = currentSection.GetBlockAt(localX, localY - 1, localZ);
                if (blockBottom == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockBottom.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Top);
            }
            return false;
        }
    }
}
