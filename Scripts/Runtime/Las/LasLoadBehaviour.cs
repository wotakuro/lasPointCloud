using UnityEngine;

//
// https://www.asprs.org/divisions-committees/lidar-division/laser-las-file-format-exchange-activities

namespace PointCloud.LasFormat
{

    public class LasLoadBehaviour : MonoBehaviour
    {
        private Material material;

        private PublicHeaderBlock header;
        private MeshGenerator meshGenerator;
        private ThreadLoadExecutor threadLoadExecutor;
        // double前提だとデータが壊れるので……最初に出てきたものにオフセットさせます
        private Vector3Double offsetPos;
        private bool isSetOffset = false;
        private bool isAlreadyRequest = false;

        private System.Action<LasLoadBehaviour> onComplete;

        public Vector3Double OffsetPosition
        {
            get { return offsetPos; }
            set {
                offsetPos = value;
                isSetOffset = true; 
            }
        }

        public PublicHeaderBlock HeaderInfo
        {
            get { return header; }
        }

        

        public void SetMaterial(Material mat)
        {
            this.material = mat;
        }

        private void Update()
        {
            if (!this.isSetOffset && threadLoadExecutor != null 
                && threadLoadExecutor.IsSetOffset)
            {
                this.OffsetPosition = threadLoadExecutor.OffsetPosition;
            }
            if (this.meshGenerator != null)
            {
                this.meshGenerator.UpdateFromMainThread();
                if (this.meshGenerator.IsComplete && onComplete != null)
                {
                    this.onComplete(this);
                    this.onComplete = null;
                }
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


        public void LoadDataSync(string path,ref MeshGenerator.Config conf, int reductionParam)
        {
            if (isAlreadyRequest) { return; }
            FileReader reader = new FileReader(path);
            ReadHeader(reader);
            ReadBody(reader, ref header,ref conf,reductionParam);
            isAlreadyRequest = true;
        }
        public void LoadDataAsync(string path, ref MeshGenerator.Config conf,int reductionParam,
            System.Action<LasLoadBehaviour> completeCallback = null)
        {
            if (isAlreadyRequest) { return; }
            FileReader reader = new FileReader(path);
            ReadHeader(reader);
            this.onComplete = completeCallback;

            byte format = header.pointDatRecordFormat;
            ulong num = header.legacyNumofPointRecords;

            conf.SetPointNum( num ,reductionParam);
            this.meshGenerator = new MeshGenerator(transform, material, conf);
            this.threadLoadExecutor = new ThreadLoadExecutor(reader, ref this.header, meshGenerator,reductionParam);
            this.threadLoadExecutor.StartExecute();
            isAlreadyRequest = true;
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
                if (!isSetOffset)
                {
                    CalcOffsetVector3Double(out offsetPos, ref header, ref pointData);
                    isSetOffset = true;
                }
                GetPointData(ref header, ref pointData,ref offsetPos, out point, out col);
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
            meshGenerator.UpdateFromMainThread(true);

        }
        public static void CalcOffsetVector3Double(out Vector3Double offset, 
            ref PublicHeaderBlock header, ref PointDataFormat pointDataFormat)
        {
            double x = (header.xScaleFactor * pointDataFormat.baseData.x);
            double y = (header.yScaleFactor * pointDataFormat.baseData.y);
            double z = (header.zScaleFactor * pointDataFormat.baseData.z);

            offset = new Vector3Double(x, y, z);

        }

        public static void GetPointData(ref PublicHeaderBlock header,ref PointDataFormat pointDataFormat,
            ref Vector3Double offset, out Vector3 pos,out Color col)
        {
            double x = (header.xScaleFactor * pointDataFormat.baseData.x) - offset.x;
            double y = (header.yScaleFactor * pointDataFormat.baseData.y) - offset.y;
            double z = (header.zScaleFactor * pointDataFormat.baseData.z) - offset.z;

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