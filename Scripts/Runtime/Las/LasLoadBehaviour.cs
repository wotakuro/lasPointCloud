using UnityEngine;

//
// https://www.asprs.org/divisions-committees/lidar-division/laser-las-file-format-exchange-activities

namespace PointCloud.LasFormat
{
    public class LasLoader
    {

        public static GameObject Instantiate(string path, Material mat,int reductionParam = 1, MeshGenerator.Config conf = default)
        {

            GameObject gmo = new GameObject();
            var lasLoader = gmo.AddComponent<LasLoadBehaviour>();
            lasLoader.SetMaterial(mat);
            lasLoader.LoadDataAsync(path, ref conf, reductionParam);
            return gmo;
        }

        public static GameObject InstantiateSync(string path, Material mat, int reductionParam = 1, MeshGenerator.Config conf=default)
        {

            GameObject gmo = new GameObject();
            var lasLoader = gmo.AddComponent<LasLoadBehaviour>();
            lasLoader.SetMaterial(mat);
            lasLoader.LoadDataSync(path, ref conf, reductionParam);
            return gmo;
        }
    }

    public class LasLoadBehaviour : MonoBehaviour
    {
        private string filaname;
        private Material material;

        private PublicHeaderBlock header;
        private MeshGenerator meshGenerator;
        private ThreadLoadExecutor threadLoadExecutor;

        internal void SetMaterial(Material mat)
        {
            this.material = mat;
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


        internal void LoadDataSync(string path,ref MeshGenerator.Config conf, int reductionParam)
        {
            FileReader reader = new FileReader(path);
            ReadHeader(reader);
            ReadBody(reader, ref header,ref conf,reductionParam);
        }
        internal void LoadDataAsync(string path, ref MeshGenerator.Config conf,int reductionParam)
        {
            FileReader reader = new FileReader(path);
            ReadHeader(reader);

            byte format = header.pointDatRecordFormat;
            ulong num = header.legacyNumofPointRecords;

            conf.SetPointNum( num ,reductionParam);
            this.meshGenerator = new MeshGenerator(transform, material, conf);
            this.threadLoadExecutor = new ThreadLoadExecutor(reader, ref this.header, meshGenerator,reductionParam);
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


        private void ReadBody(FileReader reader,ref PublicHeaderBlock header, ref MeshGenerator.Config conf,int reductionParam)
        {
            byte format = header.pointDatRecordFormat;
            ulong num = header.legacyNumofPointRecords;
            Vector3 point;
            Color col;

            conf.SetPointNum( num , reductionParam);
            this.meshGenerator = new MeshGenerator(transform,material, conf);

            PointDataFormat pointData = new PointDataFormat();

            var readFunc = PointDataFormat.GetReadAction(format);

            for (ulong i = 0; i < num; ++i)
            {
                readFunc(ref pointData,reader);
                GetPointData(ref header, ref pointData, out point, out col);
                if (!meshGenerator.AddPointData(point, col))
                {
                    meshGenerator.UpdateFromMainThread();
                    meshGenerator.AddPointData(point, col);
                }
                // reduction
                if (reductionParam > 0)
                {
                    reader.Skip(reductionParam * header.pointDataRecordLength);
                    i += (ulong)reductionParam;
                }
            }
            meshGenerator.UpdateFromMainThread();

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