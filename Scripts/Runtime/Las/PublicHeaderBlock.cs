using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PointCloud.LasFormat
{
    public unsafe struct PublicHeaderBlock
    {
        enum GlobalEncodingBit
        {
            GPSTimeType = 0,
            WaveFormDataPacketInternal = 1,
            WaveFormDataPacketExternal = 2,
            SyntheticReturnNumbers = 3,
            WKT = 4,
        }

        private static byte[] LasSignature = new byte[]
        {
           // LASF
            0x4c,0x41,0x53,0x46
        };
        public fixed byte signature[4];
        public ushort fileSourceId;
        public ushort globalEncoding;
        public fixed byte projectID[16];// = new byte[16];
        public byte versionMajaor;
        public byte versionMinor;
        public fixed byte systemIdentifier[32];// = new byte[32];
        public fixed byte generatingSoftwar[32];// = new byte[32];
        public ushort createDayOfYear;
        public ushort createYear;

        public ushort headerSize;
        public uint offsetToPointData;
        public uint numberOfLengthRecord;
        public byte pointDatRecordFormat;
        public ushort pointDataRecordLength;
        public uint legacyNumofPointRecords;
        public fixed uint legacyNumberPointByRet[5];// = new uint[5];
        public double xScaleFactor;
        public double yScaleFactor;
        public double zScaleFactor;
        public double xOffset;
        public double yOffset;
        public double zOffset;
        public double maxX;
        public double maxY;
        public double maxZ;
        public double minX;
        public double minY;
        public double minZ;
        public ulong startOfWaveFormDataPacketRecord;
        public ulong startOfFirstExtendedVariable;
        public ulong numberOfExtendedVariableLength;

        public ulong numberOfPointRecords;
        public fixed ulong numberOfPointsByReturn[15];// = new ulong[15];


        public bool Read(FileReader reader)
        {
            fixed (byte* ptr = &signature[0])
            {
                reader.ReadBytes(ptr, 4);
            }
            if (!ValidateSignature())
            {
                return false;
            }

            this.fileSourceId = reader.ReadUshort();
            this.globalEncoding = reader.ReadUshort();
            fixed (byte* ptr = &projectID[0])
            {
                reader.ReadBytes(ptr, 16);
            }

            this.versionMajaor = reader.ReadByte();
            this.versionMinor = reader.ReadByte();

            fixed (byte* ptr = &systemIdentifier[0])
            {
                reader.ReadBytes(ptr, 32);
            }
            fixed (byte* ptr = &generatingSoftwar[0])
            {
                reader.ReadBytes(ptr, 32);
            }

            this.createDayOfYear = reader.ReadUshort();
            this.createYear = reader.ReadUshort();

            this.headerSize = reader.ReadUshort();
            this.offsetToPointData = reader.ReadUint();
            this.numberOfLengthRecord = reader.ReadUint();
            this.pointDatRecordFormat = reader.ReadByte();
            this.pointDataRecordLength = reader.ReadUshort();
            this.legacyNumofPointRecords = reader.ReadUint();


            fixed (void* ptr = &legacyNumberPointByRet[0])
            {
                reader.ReadBytes( (byte*)ptr, 4*5);
            }

            this.xScaleFactor = reader.ReadDouble();
            this.yScaleFactor = reader.ReadDouble();
            this.zScaleFactor = reader.ReadDouble();

            this.xOffset = reader.ReadDouble();
            this.yOffset = reader.ReadDouble();
            this.zOffset = reader.ReadDouble();

            this.maxX = reader.ReadDouble();
            this.minX = reader.ReadDouble();

            this.maxY = reader.ReadDouble();
            this.minY = reader.ReadDouble();

            this.maxZ = reader.ReadDouble();
            this.minZ = reader.ReadDouble();

            // from 1.3
            if(this.versionMajaor == 1 && this.versionMinor < 3)
            {
                return true;
            }
            this.startOfWaveFormDataPacketRecord = reader.ReadUlong();
            // from 1.4
            if (this.versionMajaor == 1 && this.versionMinor < 4)
            {
                return true;
            }
            this.startOfFirstExtendedVariable = reader.ReadUlong();
            this.numberOfExtendedVariableLength = reader.ReadUlong();


            numberOfPointRecords = reader.ReadUlong();
            fixed (void* ptr = &numberOfPointsByReturn[0])
            {
                reader.ReadBytes((byte*)ptr, 8 * 15);
            }
            return true;
        }

        private bool ValidateSignature()
        {
            for( int i = 0; i < 4; ++i)
            {
                if(this.signature[i] != LasSignature[i]) { return false; }
            }
            return true;
        }
#if DEBUG
        public void DebugWriteToFile(string path)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(1024);
            sb.Append("version:").Append(versionMajaor).Append(".").Append(versionMinor).Append("\n");
            sb.Append("headerSize:").Append(headerSize).Append("\n");
            sb.Append("format:").Append(pointDatRecordFormat).Append("\n");

            sb.Append("offsetToPointData:").Append(offsetToPointData).Append("\n");
            sb.Append("numberOfLengthRecord:").Append(numberOfLengthRecord).Append("\n");
            sb.Append("pointDataRecordLength:").Append(pointDataRecordLength).Append("\n");


            sb.Append("xScaleFactor:").Append(xScaleFactor).Append("\n");
            sb.Append("yScaleFactor:").Append(yScaleFactor).Append("\n");
            sb.Append("zScaleFactor:").Append(zScaleFactor).Append("\n");

            sb.Append("xOffset:").Append(xOffset).Append("\n");
            sb.Append("yOffset:").Append(yOffset).Append("\n");
            sb.Append("zOffset:").Append(zOffset).Append("\n");

            sb.Append("XRange:").Append(minX).Append(" ").Append(maxX).Append("\n");
            sb.Append("YRange:").Append(minY).Append(" ").Append(maxY).Append("\n");
            sb.Append("ZRange:").Append(minZ).Append(" ").Append(maxZ).Append("\n");


            sb.Append("pointDataRecordLength:").Append(pointDataRecordLength).Append("\n");
            sb.Append("legacyNumofPointRecords:").Append(legacyNumofPointRecords).Append("\n");


            sb.Append("startOfWaveFormDataPacketRecord:").Append(startOfWaveFormDataPacketRecord).Append("\n");
            sb.Append("startOfFirstExtendedVariable:").Append(startOfFirstExtendedVariable).Append("\n");
            sb.Append("numberOfExtendedVariableLength:").Append(numberOfExtendedVariableLength).Append("\n");
            System.IO.File.WriteAllText(path, sb.ToString());
        }
#endif
    }
}
