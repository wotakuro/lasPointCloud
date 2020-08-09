using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PointCloud.LasFormat
{
    class ThreadLoadExecutor : IDisposable
    {
        private FileReader reader;
        private PublicHeaderBlock header;
        private MeshGenerator meshGenerator;
        private System.Threading.Thread thread;
        public ThreadLoadExecutor(FileReader r, ref PublicHeaderBlock h, MeshGenerator mg)
        {
            this.reader = r;
            this.header = h;
            this.meshGenerator = mg;
        }

        public void StartExecute()
        {
            thread = new System.Threading.Thread(this.Execute);
            thread.Start();
        }
        public void Dispose()
        {
            if(thread != null)
            {
                thread.Abort();
            }
            thread = null;
        }

        private void Execute()
        {
            byte format = header.pointDatRecordFormat;
            ulong num = header.legacyNumofPointRecords;
            Vector3 point;
            Color col;

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
                        LasFileLoader.GetPointData(ref header, ref pointData, out point, out col);
                        // append 出来るまで実行
                        while (!meshGenerator.AddPointData(point, col))
                        {
                            System.Threading.Thread.Sleep(2);
                        }
                    }
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
    }


}