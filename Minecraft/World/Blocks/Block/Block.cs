﻿using OpenTK;

namespace Minecraft
{
    abstract class Block
    {
        public int id { get; private set; }
        public bool isTickable { get; protected set; }
        public bool isInteractable { get; protected set; }
        protected readonly AxisAlignedBox[] emptyAABB = new AxisAlignedBox[0];

        public Block(int id)
        {
            this.id = id;
        }

        public abstract BlockState GetNewDefaultState();

        public virtual void OnInteract(BlockState blockstate, World world) { }

        public virtual bool CanAddBlockAt(World world, Vector3i blockPos)
        {
            return true;
        }

        public virtual void OnTick(BlockState blockState, World world, float deltaTime) { }

        public virtual void OnAdded(BlockState blockstate, World world) { }

        public virtual AxisAlignedBox[] GetCollisionBox(BlockState state, Vector3i blockPos)
        {
            return new AxisAlignedBox[] { GetFullBlockCollision(blockPos) };
        }

        public static AxisAlignedBox GetFullBlockCollision(Vector3i blockPos)
        {
            return new AxisAlignedBox(new Vector3(blockPos.X, blockPos.Y, blockPos.Z),
                new Vector3(blockPos.X + Constants.CUBE_SIZE, blockPos.Y + Constants.CUBE_SIZE, blockPos.Z + Constants.CUBE_SIZE));
        }
    }

    class BlockAir : Block
    {
        public BlockAir(int id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateAir();
        }

        public override AxisAlignedBox[] GetCollisionBox(BlockState state, Vector3i blockPos)
        {
            return emptyAABB;
        }
    }

}
