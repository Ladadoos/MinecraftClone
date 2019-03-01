﻿
using Minecraft.Physics;
using OpenTK;
using Minecraft.World.Chunks;

namespace Minecraft.World.Blocks
{
    class Cube
    {
        /* 
         * Back   --- Negative Z
         * Front  --- Positive Z
         * Top    --- Positive Y
         * Bottom --- Negative Y
         * Left   --- Negative X
         * Right  --- Positive X
         */

        public static float[] GetOriginCubeVertices()
        {
            return new float[] {
                 //Back
                 1, 0, 0,
                 0, 0, 0,
                 0, 1, 0,
                 1, 1, 0,

                 //Right
                 1, 0, 1,
                 1, 0, 0,
                 1, 1, 0,
                 1, 1, 1,

                 //Front
                 0, 0, 1,
                 1, 0, 1,
                 1, 1, 1,
                 0, 1, 1,

                 //Left
                 0, 0, 0,
                 0, 0, 1,
                 0, 1, 1,
                 0, 1, 0,

                 //Top
                 0, 1, 1,
                 1, 1, 1,
                 1, 1, 0,
                 0, 1, 0,

                 //Bottom
                 0, 0, 0,
                 1, 0, 0,
                 1, 0, 1,
                 0, 0, 1,
            };
        }

        public static float[] GetCubeBackVertices(float x, float y, float z)
        {
            return new float[] {
                 //Back
                 x + 1, y + 0, z + 0, // 0 - 1
                 x + 0, y + 0, z + 0, // 2 - 3
                 x + 0, y + 1, z + 0, // 4 - 5
                 x + 1, y + 1, z + 0, // 6 - 7
              };
        }

        public static float[] GetCubeRightVertices(float x, float y, float z)
        {
            return new float[] {
                 //Right
                 x + 1, y + 0, z + 1, // 8 - 9
                 x + 1, y + 0, z + 0, // 10 - 11
                 x + 1, y + 1, z + 0, // 12 - 13
                 x + 1, y + 1, z + 1, // 14 - 15
              };
        }

        public static float[] GetCubeFrontVertices(float x, float y, float z)
        {
            return new float[] {
                 //Front
                 x + 0, y + 0, z + 1, // 16 - 17
                 x + 1, y + 0, z + 1, // 18 - 19
                 x + 1, y + 1, z + 1, // 20 - 21
                 x + 0, y + 1, z + 1, // 22 - 23
              };
        }

        public static float[] GetCubeLeftVertices(float x, float y, float z)
        {
            return new float[] {
                 //Left
                 x + 0, y + 0, z + 0, // 24 - 25
                 x + 0, y + 0, z + 1, // 26 - 27
                 x + 0, y + 1, z + 1, // 28 - 29
                 x + 0, y + 1, z + 0, // 30 - 31
              };
        }

        public static float[] GetCubeTopVertices(float x, float y, float z)
        {
            return new float[] {
                 //Top
                 x + 0, y + 1, z + 1, // 32 - 33
                 x + 1, y + 1, z + 1, // 34 - 35
                 x + 1, y + 1, z + 0, // 36 - 37
                 x + 0, y + 1, z + 0, // 38 - 39
              };
        }

        public static float[] GetCubeBottomVertices(float x, float y, float z)
        {
            return new float[] {
                 //Bottom
                 x + 0, y + 0, z + 0, // 40 - 41
                 x + 1, y + 0, z + 0, // 42 - 43
                 x + 1, y + 0, z + 1, // 44 - 45
                 x + 0, y + 0, z + 1, // 46 - 47
              };
        }

        public static AABB GetAABB(float x, float y, float z)
        {
            return new AABB(new Vector3(x, y, z), new Vector3(x + Constants.CUBE_SIZE, y + Constants.CUBE_SIZE, z + Constants.CUBE_SIZE));
        }

        public static float[] GetCubeVerticesForSide(BlockSide side, float x, float y, float z)
        {
            if (side == BlockSide.BACK)
            {
                return GetCubeBackVertices(x, y, z);
            }
            else if (side == BlockSide.RIGHT)
            {
                return GetCubeRightVertices(x, y, z);
            }
            else if (side == BlockSide.FRONT)
            {
                return GetCubeFrontVertices(x, y, z);
            }
            else if (side == BlockSide.LEFT)
            {
                return GetCubeLeftVertices(x, y, z);
            }
            else if (side == BlockSide.TOP)
            {
                return GetCubeTopVertices(x, y, z);
            }
            else if (side == BlockSide.BOTTOM)
            {
                return GetCubeBottomVertices(x, y, z);
            }
            return null;
        }

        public static float[] GetCubeVertices(float x, float y, float z)
        {
            return new float[] {
                 //Back
                 x + 1, y + 0, z + 0, // 0 - 1
                 x + 0, y + 0, z + 0, // 2 - 3
                 x + 0, y + 1, z + 0, // 4 - 5
                 x + 1, y + 1, z + 0, // 6 - 7

                 //Right
                 x + 1, y + 0, z + 1, // 8 - 9
                 x + 1, y + 0, z + 0, // 10 - 11
                 x + 1, y + 1, z + 0, // 12 - 13
                 x + 1, y + 1, z + 1, // 14 - 15

                 //Front
                 x + 0, y + 0, z + 1, // 16 - 17
                 x + 1, y + 0, z + 1, // 18 - 19
                 x + 1, y + 1, z + 1, // 20 - 21
                 x + 0, y + 1, z + 1, // 22 - 23

                 //Left
                 x + 0, y + 0, z + 0, // 24 - 25
                 x + 0, y + 0, z + 1, // 26 - 27
                 x + 0, y + 1, z + 1, // 28 - 29
                 x + 0, y + 1, z + 0, // 30 - 31

                 //Top
                 x + 0, y + 1, z + 1, // 32 - 33
                 x + 1, y + 1, z + 1, // 34 - 35
                 x + 1, y + 1, z + 0, // 36 - 37
                 x + 0, y + 1, z + 0, // 38 - 39

                 //Bottom
                 x + 0, y + 0, z + 0, // 40 - 41
                 x + 1, y + 0, z + 0, // 42 - 43
                 x + 1, y + 0, z + 1, // 44 - 45
                 x + 0, y + 0, z + 1, // 46 - 47
              };
        }

        public static int[] GetOriginIndices()
        {
            return new int[] {
                 0, 1, 2,
                 2, 3, 0,

                 4, 5, 6,
                 6, 7, 4,

                 8, 9, 10,
                 10, 11, 8,

                 12, 13, 14,
                 14, 15, 12,

                 16, 17, 18,
                 18, 19, 16,

                 20, 21, 22,
                 22, 23, 20
             };
        }
    }
}
