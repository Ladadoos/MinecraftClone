﻿using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class VAOModel
    {
        public int vaoId { get; private set; }
        public int indicesCount { get; private set; }
        public List<int> buffers = new List<int>();

        public VAOModel(float[] positions, float[] textureCoordinates, float[] lights, float[] normals, int indicesCount)
        {
            this.indicesCount = indicesCount;
            CreateVAO();
            Bind();
            CreateVBO(3, positions);
            CreateVBO(3, normals);
            CreateVBO(2, textureCoordinates);
            CreateVBO(1, lights);
            Unbind();
        }

        public VAOModel(ChunkBufferLayout cbl)
        {
            this.indicesCount = cbl.indicesCount;
            CreateVAO();
            Bind();
            CreateVBO(3, cbl.positions);
            CreateVBO(3, cbl.normals);
            CreateVBO(2, cbl.textureCoordinates);
            CreateVBO(1, cbl.lights);
            Unbind();
        }

        public VAOModel(float[] positions, int[] indices)
        {
            this.indicesCount = indices.Length;
            CreateVAO();
            Bind();
            CreateVBO(3, positions);
            CreateIBO(indices);
            Unbind();
        }

        public VAOModel(float[] positions, float[] textureCoordinates, int indicesCount)
        {
            this.indicesCount = indicesCount;
            CreateVAO();
            Bind();
            CreateVBO(3, positions);
            CreateVBO(2, textureCoordinates);
            Unbind();
        }

        public void OnCloseGame()
        {
            foreach (int buffer in buffers)
            {
                GL.DeleteBuffer(buffer);
            }
            GL.DeleteVertexArray(vaoId);
            buffers.Clear();
        }

        /// <summary> Creates an index buffer object and buffers the given indices. </summary>
        private void CreateIBO(int[] indices)
        {
            int vboID = GL.GenBuffer();
            buffers.Add(vboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);
        }

        /// <summary>
        /// Creates a vertex bufffer object and buffers the given float values. The integer specifies the number of elements in the datastructure.
        /// A Vector3 would for example have this integer set to 3 (X, Y, Z)
        /// </summary>
        private void CreateVBO(int nrOfElementsInStructure, float[] data)
        {
            int vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * sizeof(float)), data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(buffers.Count, nrOfElementsInStructure, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.EnableVertexAttribArray(buffers.Count);
            buffers.Add(vboID);
        }

        /// <summary> Creates a vertex array object </summary>
        private void CreateVAO()
        {
            vaoId = GL.GenVertexArray();
        }

        public void Bind()
        {
            GL.BindVertexArray(vaoId);
        }

        public void Unbind()
        {
            GL.BindVertexArray(0);
        }
    }
}