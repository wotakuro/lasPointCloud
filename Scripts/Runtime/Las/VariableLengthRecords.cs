using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PointCloud.LasFormat
{
    public unsafe struct VariableLengthRecords
    {
        public ushort reserved;
        public fixed byte userId[16];
        public ushort recordID;
        public ushort recorderLengthAfterHeader;
        public fixed byte description[32];

        public void Read(FileReader reader)
        {
            reserved = reader.ReadUshort();

            fixed (byte* ptr = &userId[0])
            {
                reader.ReadBytes(ptr, 16);
            }
            recordID = reader.ReadUshort();
            recorderLengthAfterHeader = reader.ReadUshort();

            fixed (byte* ptr = &description[0])
            {
                reader.ReadBytes(ptr, 32);
            }
        }
    }
}