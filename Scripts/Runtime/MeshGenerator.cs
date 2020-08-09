using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;

namespace PointCloud
{
    public class MeshGenerator : System.IDisposable
    {
        public struct Config
        {
            public ulong pointNum;
            public int bufferPoint;
            public int polyNum;
        }

        private NativeArray<Vector3> pointBuffer;
        private NativeArray<Vector3> normalBuffer;
        private NativeArray<Color> colorBuffer;
        private NativeArray<int> indexBuffer;

        private ulong currentPointNum = 0;
        private int currentVertPos = 0;
        private int currentIndexPos = 0;
        private int vertBufferSize;

        private Transform parentTransform;
        private Material drawMaterial;
        private Config config;

        private Vector3[] normalVals;

        public MeshGenerator(Transform parent, Material mat, Config conf)
        {
            if (conf.pointNum < (ulong)conf.bufferPoint)
            {
                conf.bufferPoint = (int)conf.pointNum;
            }
            this.drawMaterial = mat;
            this.parentTransform = parent;
            this.config = conf;

            ulong pointNum = conf.pointNum * 3;
            InitNormalVals(conf.polyNum);

            this.vertBufferSize = conf.bufferPoint * conf.polyNum;

            // initBuffer
            this.pointBuffer = new NativeArray<Vector3>(vertBufferSize, Allocator.Persistent);
            this.normalBuffer = new NativeArray<Vector3>(vertBufferSize, Allocator.Persistent);
            this.colorBuffer = new NativeArray<Color>(vertBufferSize, Allocator.Persistent);
            this.InitIndexBuffer();
        }

        private void InitIndexBuffer()
        {
            int indexSize = config.bufferPoint * 3 * (config.polyNum-2);
            int idx = 0;

            this.indexBuffer = new NativeArray<int>(indexSize, Allocator.Persistent);

            for( int i = 0;i < config.bufferPoint; ++i)
            {
                int startPoint = i * config.polyNum;
                for (int j=0;j < config.polyNum - 2; ++j)
                {
                    indexBuffer[idx] = startPoint + 0;
                    ++idx;
                    indexBuffer[idx] = startPoint + j +1;
                    ++idx;
                    indexBuffer[idx] = startPoint + j +2;
                    ++idx;
                }

            }
        }

        private void InitNormalVals(int polyNum)
        {
            normalVals = new Vector3[polyNum];
            for( int i = 0; i < polyNum; ++i)
            {
                float angle = (i / (float)polyNum) * Mathf.PI * 2.0f;
                normalVals[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f);
            }
        }

        public bool AddPointData(Vector3 point, Color col)
        {
            if (this.vertBufferSize <= currentVertPos)
            {
                return false;
            }
            if( this.currentPointNum >= this.config.pointNum)
            {
                return false;
            }
            for (int i = 0; i < this.config.polyNum; ++i)
            {
                pointBuffer[currentVertPos] = point;
                normalBuffer[currentVertPos] = normalVals[i];
                colorBuffer[currentVertPos] = col;
                ++currentVertPos;
            }
            this.currentIndexPos += (this.config.polyNum-2)*3;

            ++this.currentPointNum;
            return true;
        }

        public void UpdateFromMainThread()
        {
            if (this.vertBufferSize <= currentVertPos)
            {
                CreateObject();
                this.currentIndexPos = 0;
                this.currentVertPos = 0;
            }
            else if (this.currentPointNum >= this.config.pointNum)
            {
                CreateObject();
                this.currentIndexPos = 0;
                this.currentVertPos = 0;
            }
        }



        private void CreateObject()
        {
            if(this.currentVertPos == 0) { return; }
            var gmo = new GameObject();
            gmo.transform.parent = this.parentTransform;
            var filter = gmo.AddComponent<MeshFilter>();
            filter.mesh = GenerateMesh();
            var renderer = gmo.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = this.drawMaterial;
        }

        private Mesh GenerateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(pointBuffer, 0, this.currentVertPos);
            mesh.SetNormals(normalBuffer, 0, this.currentVertPos);
            mesh.SetColors(colorBuffer, 0, this.currentVertPos);
            mesh.SetIndices(indexBuffer, 0, this.currentIndexPos, MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();
            mesh.UploadMeshData(true);
            return mesh;
        }

        public void Dispose()
        {
            normalBuffer.Dispose();
            pointBuffer.Dispose();
            colorBuffer.Dispose();
            indexBuffer.Dispose();
        }
    }

}