using UnityEngine;
using Unity.Collections;
using PointCloud.LasFormat;
using UnityEngine.Rendering;
using Unity.Collections.LowLevel.Unsafe;

namespace PointCloud
{
    public class MeshGenerator : System.IDisposable
    {
        public struct Config
        {
            public ulong pointNum;
            public int bufferPoint;
            public int polyNum;
            public System.Action<GameObject> onInstansiatePart;


            public void SetPointNum(ulong num,int reduction)
            {
                if (reduction > 1)
                {
                    this.pointNum = num / (ulong)reduction;
                }
                else
                {
                    this.pointNum = num;
                }
            }

            public void Validate()
            {
                if (this.bufferPoint <= 0)
                {
                    this.bufferPoint = 50000;
                }
                if (this.polyNum < 3)
                {
                    this.polyNum = 3;
                }
                if (this.pointNum < (ulong)this.bufferPoint)
                {
                    this.bufferPoint = (int)this.pointNum;
                }
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct VertexParam
        {
            public Vector3 pos;
            public float normalX, normalY;
            public Color32 color;

            public void SetData( ref Vector3 p,float nx,float ny,ref Color32 c)
            {
                pos = p;
                normalX = nx;
                normalY = ny;
                color = c;
            }
        }

        private NativeArray<VertexParam> vertexBuffer;

        private NativeArray<int> indexBuffer;

        private ulong currentPointNum = 0;
        private int currentVertPos = 0;
        private int currentIndexPos = 0;
        private int vertBufferSize;

        private Transform parentTransform;
        private Material drawMaterial;
        private Config config;
        private Vector3[] normalVals;
        private bool isComplete = false;

        static VertexAttributeDescriptor[] vertLayout = new[] {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 2),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
        };


        public bool IsComplete
        {
            get { return this.isComplete; }
        }

        public MeshGenerator(Transform parent, Material mat, Config conf)
        {
            conf.Validate();
            this.drawMaterial = mat;
            this.parentTransform = parent;
            this.config = conf;

            InitNormalVals(conf.polyNum);

            this.vertBufferSize = conf.bufferPoint * conf.polyNum;

            // initBuffer
            this.vertexBuffer = new NativeArray<VertexParam>(vertBufferSize, Allocator.Persistent);
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

        public bool AddPointData(Vector3 point, Color32 col)
        {
            if (this.isComplete) { return true; }
            if (this.currentPointNum >= this.config.pointNum)
            {
                return true;
            }
            if (this.vertBufferSize <= currentVertPos)
            {
                return false;
            }
            VertexParam vertexParam;
            for (int i = 0; i < this.config.polyNum; ++i)
            {
                vertexParam = new VertexParam()
                {
                    pos = point,
                    color = col,
                    normalX = normalVals[i].x,
                    normalY = normalVals[i].y,
                };
                vertexBuffer[currentVertPos] = vertexParam;
                ++currentVertPos;
            }
            this.currentIndexPos += (this.config.polyNum-2)*3;

            ++this.currentPointNum;
            return true;
        }

        public void UpdateFromMainThread(bool forceComplete = false)
        {
            if(this.isComplete) { return; }
            if (forceComplete)
            {
                CreateObject();
                // complete
                this.isComplete = true;
                this.ReleaseBuffers();
            }else if (this.vertBufferSize <= currentVertPos)
            {
                CreateObject();
            }
            else if (this.currentPointNum >= this.config.pointNum)
            {
                CreateObject();
                // complete
                this.isComplete = true;
                this.ReleaseBuffers();
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
            this.config.onInstansiatePart?.Invoke(gmo);


            this.currentIndexPos = 0;
            this.currentVertPos = 0;
        }

        private Mesh GenerateMesh()
        {


            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertexBufferParams(this.currentVertPos, vertLayout);
            mesh.SetVertexBufferData(this.vertexBuffer, 0, 0, this.currentVertPos);
            /*
            mesh.SetVertices(pointBuffer, 0, this.currentVertPos);
            mesh.SetNormals(normalBuffer, 0, this.currentVertPos);
            mesh.SetColors(colorBuffer, 0, this.currentVertPos);
            */

            mesh.SetIndices(indexBuffer, 0, this.currentIndexPos, MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();
            mesh.UploadMeshData(true);
            return mesh;
        }

        public void Dispose()
        {
            ReleaseBuffers();
        }
        private void ReleaseBuffers() {
            if (vertexBuffer.IsCreated)
            {
                vertexBuffer.Dispose();
            }
            if (indexBuffer.IsCreated)
            {
                indexBuffer.Dispose();
            }
        }
    }

}