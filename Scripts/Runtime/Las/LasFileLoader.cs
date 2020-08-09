using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//
// https://www.asprs.org/divisions-committees/lidar-division/laser-las-file-format-exchange-activities

namespace PointCloud.LasFormat
{
    public class LasFileLoader : MonoBehaviour
    {
        public string filaname;
        private Material material;

        private PublicHeaderBlock header;
        private MeshGenerator meshGenerator;
        private ThreadLoadExecutor threadLoadExecutor;

        public static GameObject InstantiateAsync(string path,Material mat, int polyNum = 3,int bufferPoint= 200000)
        {
            var conf = new MeshGenerator.Config { pointNum = 0, polyNum = polyNum, bufferPoint = bufferPoint };

            GameObject gmo = new GameObject();
            var lasLoader = gmo.AddComponent<LasFileLoader>();
            lasLoader.material = mat;
            lasLoader.LoadDataAsync(path,ref conf);
            return gmo;
        }

        public static GameObject Instantiate(string path, Material mat, int polyNum = 3, int bufferPoint = 200000)
        {
            var conf = new MeshGenerator.Config { pointNum = 0, polyNum = polyNum, bufferPoint = bufferPoint };

            GameObject gmo = new GameObject();
            var lasLoader = gmo.AddComponent<LasFileLoader>();
            lasLoader.material = mat;
            lasLoader.LoadData(path,ref conf);
            return gmo;
        }


        private void Update()
        {
            if(this.meshGenerator != null)
            {
                this.meshGenerator.UpdateFromMainThread();
            }
        }
        private void OnDestroy()
        {
            if(threadLoadExecutor != null)
            {
                threadLoadExecutor.Dispose();
            }
            threadLoadExecutor = null;
            if (meshGenerator != null)
            {
                this.meshGenerator.Dispose();
            }
            meshGenerator = null;
        }


        private void LoadData(string path,ref MeshGenerator.Config conf)
        {
            FileReader reader = new FileReader(path);
            ReadHeader(reader);
            ReadBody(reader, ref header,ref conf);
        }
        private void LoadDataAsync(string path, ref MeshGenerator.Config conf)
        {
            FileReader reader = new FileReader(path);
            ReadHeader(reader);

            byte format = header.pointDatRecordFormat;
            ulong num = header.legacyNumofPointRecords;

            conf.pointNum = num;
            this.meshGenerator = new MeshGenerator(transform, material, conf);
            this.threadLoadExecutor = new ThreadLoadExecutor(reader, ref this.header, meshGenerator);
            this.threadLoadExecutor.StartExecute();
        }

        private void ReadHeader(FileReader reader)
        {
            this.header = new PublicHeaderBlock();
            VariableLengthRecords variableLengthRecords = new VariableLengthRecords();

            this.header.Read(reader);
            if (this.header.offsetToPointData != this.header.headerSize)
            {
                variableLengthRecords.Read(reader);
            }
            reader.Position = header.offsetToPointData;
        }


        private void ReadBody(FileReader reader,ref PublicHeaderBlock header, ref MeshGenerator.Config conf)
        {
            byte format = header.pointDatRecordFormat;
            ulong num = header.legacyNumofPointRecords;
            Vector3 point;
            Color col;

            conf.pointNum = num;
            this.meshGenerator = new MeshGenerator(transform,material, conf);

            PointDataFormat pointData = new PointDataFormat();
            switch (format)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    for (ulong i = 0; i < num; ++i)
                    {
                        pointData.ReadAsFormat3(reader);
                        GetPointData(ref header, ref pointData, out point, out col);
                        if(!meshGenerator.AddPointData(point, col))
                        {
                            meshGenerator.UpdateFromMainThread();
                            meshGenerator.AddPointData(point, col);
                        }
                    }
                    meshGenerator.UpdateFromMainThread();
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
                case 8:
                    break;
                case 9:
                    break;
                case 10:
                    break;
            }

        }


        public static void GetPointData(ref PublicHeaderBlock header,ref PointDataFormat pointDataFormat,
            out Vector3 pos,out Color col)
        {
            double x = (header.xScaleFactor * pointDataFormat.baseData.x);
            double y = (header.yScaleFactor * pointDataFormat.baseData.y);
            double z = (header.zScaleFactor * pointDataFormat.baseData.z);

            if (!pointDataFormat.baseData.ScanDirectionFlag)
            {
                y = -y;
            }

            pos = new Vector3( (float)y, (float)z, (float)x);
            col = new Color( (pointDataFormat.colorInfo.red >> 8) / 256.0f,
                (pointDataFormat.colorInfo.green >> 8) / 256.0f,
                (pointDataFormat.colorInfo.blue >> 8) / 256.0f );
        }

    }
}