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

    public struct PointDataFormat
    {
        public PointDataFormatBase baseData;
        public double GPSTime;
        public PointColorInfo colorInfo;
        public ParametricInfo parametric;
        public WaveInfo waveInfo;


        public void ReadAsFormat0(FileReader reader)
        {
            baseData.Read(reader, true);
        }

        public void ReadAsFormat1(FileReader reader)
        {
            baseData.Read(reader, true);
            GPSTime = reader.ReadDouble();
        }
        public void ReadAsFormat2(FileReader reader)
        {
            baseData.Read(reader, true);
            colorInfo.Read(reader);
        }
        public void ReadAsFormat3(FileReader reader)
        {
            baseData.Read(reader, true);
            GPSTime = reader.ReadDouble();
            colorInfo.Read(reader);
        }
        public void ReadAsFormat4(FileReader reader)
        {
            baseData.Read(reader, true);
            GPSTime = reader.ReadDouble();
            parametric.Read(reader);
            waveInfo.Read(reader);
        }
        public void ReadAsFormat5(FileReader reader)
        {
            baseData.Read(reader, true);
            GPSTime = reader.ReadDouble();
            colorInfo.Read(reader);
            parametric.Read(reader);
            waveInfo.Read(reader);
        }
        public void ReadAsFormat6(FileReader reader)
        {
            baseData.Read(reader, false);
            GPSTime = reader.ReadDouble();
        }
        public void ReadAsFormat7(FileReader reader)
        {
            baseData.Read(reader, false);
            colorInfo.Read(reader);
        }
        public void ReadAsFormat8(FileReader reader)
        {
            baseData.Read(reader, false);
            GPSTime = reader.ReadDouble();
            colorInfo.Read(reader);
        }
        public void ReadAsFormat9(FileReader reader)
        {
            baseData.Read(reader, false);
            GPSTime = reader.ReadDouble();
            parametric.Read(reader);
            waveInfo.Read(reader);
        }
        public void ReadAsFormat10(FileReader reader)
        {
            baseData.Read(reader, false);
            GPSTime = reader.ReadDouble();
            colorInfo.Read(reader);
            parametric.Read(reader);
            waveInfo.Read(reader);
        }


    }
}