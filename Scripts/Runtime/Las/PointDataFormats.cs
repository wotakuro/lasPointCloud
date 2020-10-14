using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PointCloud.LasFormat
{
    public struct PointDataFormatBase
    {

        public int x;
        public int y;
        public int z;
        public ushort intensity;
        public byte flagsExt;
        public byte flags; // 1 or 2byte
        public byte classification;
        public byte scanAngleRank;
        public byte userdata;
        public ushort pointSourceId;


        public bool ScanDirectionFlag
        {
            get {
                var bit = (1 << 6);
                return (flags & bit) == bit;
            }
        }
        public unsafe byte* Parse(byte* ptr, bool flagOneByte)
        {
            int* iPtr = (int*)ptr;
            {
                x = *iPtr;
                ptr += 4;
            }
            {
                iPtr = (int*)ptr;
                y = *iPtr;
                ptr += 4;
            }
            {
                iPtr = (int*)ptr;
                z = *iPtr;
                ptr += 4;
            }

            ushort* usPtr = (ushort*)ptr;
            intensity = *usPtr;
            ptr += 2;

            if (flagOneByte)
            {
                this.flags = *ptr;
                ++ptr;
            }
            else
            {
                this.flagsExt = *ptr;
                ++ptr;
                this.flags = *ptr;
                ++ptr;
            }


            this.classification = *ptr;
            ++ptr;
            this.scanAngleRank = *ptr;
            ++ptr;
            this.userdata = *ptr;
            ++ptr;
            {
                usPtr = (ushort*)ptr;
                this.pointSourceId = *usPtr;
                ptr += 2;
            }
            return ptr;
        }

        public void Read(FileReader reader,bool flagOneByte)
        {
            x = reader.ReadInt();
            y = reader.ReadInt();
            z = reader.ReadInt();            
            intensity = reader.ReadUshort();
            if(flagOneByte)
            {
                this.flags = reader.ReadByte();
            }
            else
            {
                this.flagsExt = reader.ReadByte();
                this.flags = reader.ReadByte();
            }
            this.classification = reader.ReadByte();
            this.scanAngleRank = reader.ReadByte();
            this.userdata = reader.ReadByte();
            this.pointSourceId = reader.ReadUshort();
        }
    }

    public struct PointColorInfo
    {
        // format2 - formart3
        public ushort red;
        public ushort green;
        public ushort blue;
        public void Read(FileReader reader)
        {
            red = reader.ReadUshort();
            green = reader.ReadUshort();
            blue = reader.ReadUshort();
        }
        public unsafe byte* Parse(byte* ptr)
        {
            ushort* usPtr = (ushort*)ptr;
            {
                this.red = *usPtr;
                ptr += 2;
            }
            {
                usPtr = (ushort*)ptr;
                this.green = *usPtr;
                ptr += 2;
            }
            {
                usPtr = (ushort*)ptr;
                this.blue = *usPtr;
                ptr += 2;
            }
            return ptr;
        }
    }
    public struct WaveInfo
    {
        public byte wavePacketeDescriptorIndex;
        public ulong byteOffsetToWaveFormData;
        public uint waveformPacketSizeInBytes;
        public float returnPointWaveFormLocation;

        public void Read(FileReader reader)
        {
            wavePacketeDescriptorIndex = reader.ReadByte();
            byteOffsetToWaveFormData = reader.ReadUlong();
            waveformPacketSizeInBytes = reader.ReadUint();
            returnPointWaveFormLocation = reader.ReadFloat();
        }
    }

    public struct ParametricInfo
    {
        public float dx;
        public float dy;
        public float dz;

        public void Read(FileReader reader)
        {
            dx = reader.ReadFloat();
            dy = reader.ReadFloat();
            dz = reader.ReadFloat();
        }
    }

    public unsafe struct PointDataFormat
    {
        public delegate int PointReadDelegatge(ref PointDataFormat obj, FileReader reader);

        public PointDataFormatBase baseData;
        public double GPSTime;
        public PointColorInfo colorInfo;
        public ParametricInfo parametric;
        public WaveInfo waveInfo;

        private fixed byte buffer[96];

        public static int GetExpectedSize(byte format)
        {
            switch (format)
            {
                case 0:
                    return 20;
                case 1:
                    return 28;
                case 2:
                    return 26;
                case 3:
                    return 34;
                case 4:
                    return 57;
                case 5:
                    return 63;
                case 6:
                    return 30;
                case 7:
                    return 36;
                case 8:
                    return 38;
                case 9:
                    return 59;
                case 10:
                    return 67;
            }
            return 0;
        }

        public static PointReadDelegatge GetReadAction(byte format)
        {
            switch (format)
            {
                case 0:
                    return ReadAsFormat0;
                case 1:
                    return ReadAsFormat1;
                case 2:
                    return ReadAsFormat2;
                case 3:
                    return ReadAsFormat3;
                case 4:
                    return ReadAsFormat4;
                case 5:
                    return ReadAsFormat5;
                case 6:
                    return ReadAsFormat6;
                case 7:
                    return ReadAsFormat7;
                case 8:
                    return ReadAsFormat8;
                case 9:
                    return ReadAsFormat9;
                case 10:
                    return ReadAsFormat10;
            }
            return ReadAsFormat0;
        }

        private static int ReadAsFormat0(ref PointDataFormat obj, FileReader reader)
        {
            fixed (byte* ptr = &obj.buffer[0])
            {
                reader.ReadBytes(ptr, 20);
                obj.baseData.Parse(ptr, true);
            }
            return 0;
        }

        private static int ReadAsFormat1(ref PointDataFormat obj, FileReader reader)
        {
            fixed (byte* ptr = &obj.buffer[0])
            {
                reader.ReadBytes(ptr, 28);
                double* nextPtr = (double*)obj.baseData.Parse(ptr, true);
                obj.GPSTime = *nextPtr;
            }
            return 0;
        }
        private static int ReadAsFormat2(ref PointDataFormat obj, FileReader reader)
        {
#if true
            fixed (byte* ptr = &obj.buffer[0])
            {

                reader.ReadBytes(ptr, 26);
                byte *nextPtr = obj.baseData.Parse(ptr, true);
                obj.colorInfo.Parse(nextPtr);
            }
#else

            obj.baseData.Read(reader, true);
            obj.colorInfo.Read(reader);
#endif
            return 0;
        }
        private static int ReadAsFormat3(ref PointDataFormat obj, FileReader reader)
        {
            fixed (byte* ptr = &obj.buffer[0])
            {
                reader.ReadBytes(ptr, 34);
                byte* nextPtr = obj.baseData.Parse(ptr, true);
                double* dPtr = (double*)nextPtr;
                obj.GPSTime = *dPtr;
                nextPtr += 8;
                obj.colorInfo.Parse(nextPtr);
            }

            return 0;
        }
        private static int ReadAsFormat4(ref PointDataFormat obj, FileReader reader)
        {
            obj.baseData.Read(reader, true);
            obj.GPSTime = reader.ReadDouble();
            obj.parametric.Read(reader);
            obj.waveInfo.Read(reader);
            return 0;
        }
        private static int ReadAsFormat5(ref PointDataFormat obj, FileReader reader)
        {
            obj.baseData.Read(reader, true);
            obj.GPSTime = reader.ReadDouble();
            obj.colorInfo.Read(reader);
            obj.parametric.Read(reader);
            obj.waveInfo.Read(reader);
            return 0;
        }
        private static int ReadAsFormat6(ref PointDataFormat obj, FileReader reader)
        {
            obj.baseData.Read(reader, false);
            obj.GPSTime = reader.ReadDouble();
            return 0;
        }
        private static int ReadAsFormat7(ref PointDataFormat obj, FileReader reader)
        {
            obj.baseData.Read(reader, false);
            obj.colorInfo.Read(reader);
            return 0;
        }
        private static int ReadAsFormat8(ref PointDataFormat obj, FileReader reader)
        {
            obj.baseData.Read(reader, false);
            obj.GPSTime = reader.ReadDouble();
            obj.colorInfo.Read(reader);
            return 0;
        }
        private static int ReadAsFormat9(ref PointDataFormat obj, FileReader reader)
        {
            obj.baseData.Read(reader, false);
            obj.GPSTime = reader.ReadDouble();
            obj.parametric.Read(reader);
            obj.waveInfo.Read(reader);
            return 0;
        }
        private static int ReadAsFormat10(ref PointDataFormat obj, FileReader reader)
        {
            obj.baseData.Read(reader, false);
            obj.GPSTime = reader.ReadDouble();
            obj.colorInfo.Read(reader);
            obj.parametric.Read(reader);
            obj.waveInfo.Read(reader);
            return 0;
        }


    }
}