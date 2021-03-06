﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace PointCloud.LasFormat
{
    class ThreadLoadExecutor : IDisposable
    {
        private FileReader reader;
        private PublicHeaderBlock header;
        private MeshGenerator meshGenerator;
        private int reductionParam;
        private System.Threading.Thread thread;

#if DEBUG_PROFILING
        private CustomSampler waitForCustomSampler = CustomSampler.Create("waitFor");
        private CustomSampler execPointCustomSampler = CustomSampler.Create("ExecutePoint");
#endif


        private Vector3Double offsetPos;
        private bool isSetOffset = false;

        public Vector3Double OffsetPosition
        {
            get { return offsetPos; }
            set
            {
                offsetPos = value;
                isSetOffset = true;
            }
        }
        public bool IsSetOffset
        {
            get { return isSetOffset; }
        }


        public ThreadLoadExecutor(FileReader r, ref PublicHeaderBlock h, MeshGenerator mg, int rParam)
        {
            this.reader = r;
            this.header = h;
            this.meshGenerator = mg;
            this.reductionParam = rParam-1;
            if ( this.reductionParam < 0) { this.reductionParam = 0; }
        }

        public void StartExecute()
        {
            thread = new System.Threading.Thread(this.Execute);
            thread.Priority = System.Threading.ThreadPriority.AboveNormal;
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
            Color32 col;

            PointDataFormat pointData = new PointDataFormat();

            var readFunc = PointDataFormat.GetReadAction(format);

            for (ulong i = 0; i < num; ++i)
            {
#if DEBUG_PROFILING
                execPointCustomSampler.Begin();
#endif

                readFunc(ref pointData,reader);
                if (!isSetOffset)
                {
                    LasLoadBehaviour.CalcOffsetForFloatPrecision(out offsetPos,ref header, ref pointData);
                    this.isSetOffset = true;
                }

                LasLoadBehaviour.GetPointData(ref header, ref pointData,ref offsetPos, out point, out col);
                // append 出来るまで実行
                while (!meshGenerator.AddPointData(point, col))
                {
#if DEBUG_PROFILING
                    waitForCustomSampler.Begin();
#endif
                    System.Threading.Thread.Sleep(2);
#if DEBUG_PROFILING
                    waitForCustomSampler.End();
#endif
                }
                // reduction
                if (this.reductionParam > 0)
                {
                    reader.Skip(this.reductionParam * header.pointDataRecordLength);
                    i += (ulong)this.reductionParam;
                }

#if DEBUG_PROFILING
                execPointCustomSampler.End();
#endif
            }

        }
    }


}