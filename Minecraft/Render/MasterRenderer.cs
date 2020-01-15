﻿using OpenTK;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;
using System;
using System.Threading;

namespace Minecraft
{
    class ChunkBufferLayout
    {
        public float[] positions;
        public float[] textureCoordinates;
        public float[] lights;
        public float[] normals;
        public int indicesCount;
    }

    class MasterRenderer
    {
        private float colorClearR = 0.57F;
        private float colorClearG = 0.73F;
        private float colorClearB = 1.0F;

        private ShaderBasic basicShader;
        private EntityShader entityShader;
        private CameraController cameraController;
        private WireframeRenderer wireframeRenderer;
        private DebugHelper debugHelper;
        private PlayerHoverBlockRenderer playerBlockRenderer;
        private TextureAtlas textureAtlas;
        private BlockModelRegistry blockModelRegistry;
        private EntityMeshRegistry entityMeshRegistry;
        private ScreenQuad screenQuad;
        private UIRenderer uiRenderer;
        private UICanvasIngame ingameCanvas;

        private Dictionary<Vector2, RenderChunk> toRenderChunks = new Dictionary<Vector2, RenderChunk>();
        private HashSet<Chunk> toRemeshChunks = new HashSet<Chunk>();
        private OpaqueMeshGenerator blocksMeshGenerator;

        private Game game;

        public MasterRenderer(Game game)
        {
            this.game = game;
            basicShader = new ShaderBasic();
            entityShader = new EntityShader();
            cameraController = new CameraController(game.window);
            SetActiveCamera(game.player.camera);

            int textureAtlasId = TextureLoader.LoadTexture("../../Resources/texturePack.png");
            textureAtlas = new TextureAtlas(textureAtlasId, 256, 16);
            blockModelRegistry = new BlockModelRegistry(textureAtlas);
            blocksMeshGenerator = new OpaqueMeshGenerator(blockModelRegistry);
            entityMeshRegistry = new EntityMeshRegistry(textureAtlas);
            screenQuad = new ScreenQuad(game.window);
            wireframeRenderer = new WireframeRenderer(cameraController.camera);
            debugHelper = new DebugHelper(game, wireframeRenderer);
            playerBlockRenderer = new PlayerHoverBlockRenderer(wireframeRenderer, game.player);

            uiRenderer = new UIRenderer(game.window, cameraController);
            ingameCanvas = new UICanvasIngame(game);
            AddCanvas(ingameCanvas);

            EnableDepthTest();
            EnableCulling();

            Thread meshThread = new Thread(MeshGenerator);
            meshThread.IsBackground = true;
            meshThread.Start();
        }

        public void SetActiveCamera(Camera camera)
        {
            if (cameraController.camera != null)
            {
                cameraController.camera.OnProjectionChangedHandler -= OnPlayerCameraProjectionChanged;
            }
            camera.OnProjectionChangedHandler += OnPlayerCameraProjectionChanged;
            cameraController.ControlCamera(camera);
            UploadProjectionMatrix();
        }

        public void AddCanvas(UICanvas canvas) => uiRenderer.AddCanvas(canvas);
        public void RemoveCanvas(UICanvas canvas) => uiRenderer.RemoveCanvas(canvas);
        public Font GetFont(FontType type) => uiRenderer.GetFont(type);

        public void Render(World world)
        {
            GL.Enable(EnableCap.DepthTest);
            screenQuad.Bind();
            GL.ClearColor(colorClearR, colorClearG, colorClearB, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            basicShader.Start();
            basicShader.LoadTexture(basicShader.location_TextureAtlas, 0, textureAtlas.textureId);
            basicShader.LoadMatrix(basicShader.location_ViewMatrix, cameraController.camera.currentViewMatrix);

            lock (meshLock)
            {
                foreach (KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
                {
                    if (chunkToRender.Value.hardBlocksModel == null) continue;

                    Vector3 min = new Vector3(chunkToRender.Key.X * 16, 0, chunkToRender.Key.Y * 16);
                    Vector3 max = min + new Vector3(16, 256, 16);
                    if (!cameraController.camera.viewFrustum.IsAABBInFrustum(new AxisAlignedBox(min, max)))
                    {
                        continue;
                    }

                    chunkToRender.Value.hardBlocksModel.Bind();
                    basicShader.LoadMatrix(basicShader.location_TransformationMatrix, chunkToRender.Value.transformationMatrix);
                    GL.DrawArrays(PrimitiveType.Quads, 0, chunkToRender.Value.hardBlocksModel.indicesCount);
                }
            }
            
            entityShader.Start();
            entityShader.LoadTexture(entityShader.location_TextureAtlas, 0, textureAtlas.textureId);
            entityShader.LoadMatrix(entityShader.location_ViewMatrix, cameraController.camera.currentViewMatrix);
            foreach (Entity entity in world.loadedEntities.Values)
            {
                if (entityMeshRegistry.models.TryGetValue(entity.entityType, out VAOModel entityMeshModel))
                {
                    entityMeshModel.Bind();
                    entityShader.LoadMatrix(entityShader.location_TransformationMatrix, Matrix4.Identity * Matrix4.CreateTranslation(entity.position));
                    GL.DrawArrays(PrimitiveType.Quads, 0, entityMeshModel.indicesCount);
                }
            }

            playerBlockRenderer.RenderSelection();
            debugHelper.UpdateAndRender();
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            uiRenderer.Render();
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.CullFace);

            screenQuad.Unbind();
            screenQuad.RenderToScreen();
        }

        struct Test {
            public RenderChunk rr;
            public ChunkBufferLayout cbl;
        }
        
        private Queue<Test> toProcess = new Queue<Test>();
        private object meshLock = new object();
        private void MeshGenerator()
        {
            while (true)
            {
                Thread.Sleep(5);

                lock (meshLock)
                {
                    Chunk toRemove = null;
                    foreach (Chunk chunk in toRemeshChunks)
                    {
                        Vector2 gridPosition = new Vector2(chunk.gridX, chunk.gridZ);

                        if (!toRenderChunks.TryGetValue(gridPosition, out RenderChunk renderChunk))
                        {
                            renderChunk = new RenderChunk(chunk.gridX, chunk.gridZ);
                            toRenderChunks.Add(gridPosition, renderChunk);
                        }

                        toProcess.Enqueue(new Test()
                        {
                            rr =renderChunk,
                            cbl = blocksMeshGenerator.GenerateMeshFor(game.world, chunk)
                        });

                        toRemove = chunk;
                        break;
                    }
                    // toRemeshChunks.Clear();
                    toRemeshChunks.Remove(toRemove);
                }
            }
        }

        //private bool firstPass = false;
        public void EndFrameUpdate(World world)
        {
            lock (meshLock)
            {
               /* Chunk toRemove = null;
                foreach (Chunk chunk in toRemeshChunks)
                {
                    Vector2 gridPosition = new Vector2(chunk.gridX, chunk.gridZ);

                    if (!toRenderChunks.TryGetValue(gridPosition, out RenderChunk renderChunk))
                    {
                        renderChunk = new RenderChunk(chunk.gridX, chunk.gridZ);
                        toRenderChunks.Add(gridPosition, renderChunk);
                    }

                    toProcess.Enqueue(new Test()
                    {
                        rr = renderChunk,
                        cbl = blocksMeshGenerator.GenerateMeshFor(game.world, chunk.DeepClone())
                    });

                    toRemove = chunk;
                    break;
                }
                // toRemeshChunks.Clear();
                toRemeshChunks.Remove(toRemove);*/

                if (toProcess.Count > 0)
                {
                    Test t = toProcess.Dequeue();
                    t.rr.hardBlocksModel?.OnCloseGame();
                    t.rr.hardBlocksModel = new VAOModel(t.cbl);
                }
            }


            //var start = DateTime.Now;

            /*foreach (Chunk chunk in toRemeshChunks)
            {
                Vector2 gridPosition = new Vector2(chunk.gridX, chunk.gridZ);

                if (!toRenderChunks.TryGetValue(gridPosition, out RenderChunk renderChunk))
                {
                    renderChunk = new RenderChunk(chunk.gridX, chunk.gridZ);
                    toRenderChunks.Add(gridPosition, renderChunk);
                }

                renderChunk.hardBlocksModel = blocksMeshGenerator.GenerateMeshFor(world, chunk);
            }*/

            /*var now2 = DateTime.Now - start;
            if (!firstPass)
            {
                Logger.Info("Generating initial meshes took: " + now2);
            }

            toRemeshChunks.Clear();*/
            //firstPass = true;

           /* foreach (Chunk chunk in toRemeshChunks)
            {
                Vector2 gridPosition = new Vector2(chunk.gridX, chunk.gridZ);

                if (!toRenderChunks.TryGetValue(gridPosition, out RenderChunk renderChunk))
                {
                    renderChunk = new RenderChunk(chunk.gridX, chunk.gridZ);
                    toRenderChunks.Add(gridPosition, renderChunk);
                }

                renderChunk.hardBlocksModel?.OnCloseGame();
                renderChunk.hardBlocksModel = blocksMeshGenerator.GenerateMeshFor(game.world, chunk.DeepClone());
            }
            toRemeshChunks.Clear();*/

            cameraController.Update();
        }

        public void OnChunkLoaded(Chunk chunk)
        {
            MeshChunk(chunk);

            if (game.world.loadedChunks.TryGetValue(new Vector2(chunk.gridX - 1, chunk.gridZ), out Chunk cXNeg))
            {
                MeshChunk(cXNeg);
            }
            if (game.world.loadedChunks.TryGetValue(new Vector2(chunk.gridX + 1, chunk.gridZ), out Chunk cXPos))
            {
                MeshChunk(cXPos);
            }
            if (game.world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ - 1), out Chunk cZNeg))
            {
                MeshChunk(cZNeg);
            }
            if (game.world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ + 1), out Chunk cZPos))
            {
                MeshChunk(cZPos);
            }
        }

        public void OnBlockPlaced(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState)
        {
            MeshChunkAndSurroundings(world, chunk, blockPos, newState);
        }

        public void OnBlockRemoved(World world, Chunk chunk, Vector3i blockPos, BlockState oldState)
        {
            MeshChunkAndSurroundings(world, chunk, blockPos, oldState);
        }

        private void MeshChunk(Chunk chunk)
        {
            lock (meshLock)
            {
                if (!toRemeshChunks.Contains(chunk))
                {
                    toRemeshChunks.Add(chunk);
                }
            }
        }

        private void MeshChunkAndSurroundings(World world, Chunk chunk, Vector3i blockPos, BlockState state)
        {
            int localX = blockPos.X & 15;
            int localZ = blockPos.Z & 15;
            MeshChunk(chunk);

            if (localX == 0 && world.loadedChunks.TryGetValue(new Vector2(chunk.gridX - 1, chunk.gridZ), out Chunk cXNeg))
            {
                MeshChunk(cXNeg);
            }
            if (localX == 15 && world.loadedChunks.TryGetValue(new Vector2(chunk.gridX + 1, chunk.gridZ), out Chunk cXPos))
            {
                MeshChunk(cXPos);
            }
            if (localZ == 0 && world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ - 1), out Chunk cZNeg))
            {
                MeshChunk(cZNeg);
            }
            if (localZ == 15 && world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ + 1), out Chunk cZPos))
            {
                MeshChunk(cZPos);
            }
        }

        private void OnPlayerCameraProjectionChanged(ProjectionMatrixInfo pInfo)
        {
            screenQuad.AdjustToWindowSize(pInfo.windowWidth, pInfo.windowHeight);
            UploadProjectionMatrix();
        }

        private void UploadProjectionMatrix()
        {
            basicShader.Start();
            basicShader.LoadMatrix(basicShader.location_ProjectionMatrix, cameraController.camera.currentProjectionMatrix);
            entityShader.Start();
            entityShader.LoadMatrix(entityShader.location_ProjectionMatrix, cameraController.camera.currentProjectionMatrix);
            entityShader.Stop();
        }

        public void OnCloseGame()
        {
            basicShader.OnCloseGame();
            TextureLoader.OnCloseGame();
            wireframeRenderer.OnCloseGame();

            foreach (KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
            {
                chunkToRender.Value.OnCloseGame();
            }
        }

        private void EnableCulling()
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }

        private void DisableCulling()
        {
            GL.Disable(EnableCap.CullFace);
        }

        /// <summary> Renders only the lines formed by connecting the vertices together.
        private void EnableLineModeRendering()
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        }

        /// <summary> Enabling depth test insures that object A behind object B isn't rendered over object B </summary>
        private void EnableDepthTest()
        {
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
