using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;

namespace PointCloud
{
    public class FileReader:System.IDisposable
    {
        private string filename;
        private FileStream readFs;
        private byte[] buffer = new byte[512];
        

        public long Position
        {
            get
            {
                if (readFs == null) { return 0; }
                return readFs.Position;
            }
            set
            {
                if (readFs == null) { return ; }
                readFs.Position = value;
            }
        }

        public FileReader(string file)
        {
            this.filename = file;
            readFs = File.OpenRead(file);
        }

        public void Skip(int bytes)
        {
            if(readFs != null)
            {
                readFs.Seek(bytes, SeekOrigin.Current);
            }
        }

        public unsafe void ReadBytes(byte* ptr,int length)
        {
            ReadToBuffer(length);
            fixed (byte* bufPtr = &buffer[0])
            {
                byte* rPtr = bufPtr;
                for (int i = 0; i < length; ++i)
                {
                    *ptr = *rPtr;
                    ptr += 1;
                    rPtr += 1;
                }
            }
        }

        public byte ReadByte()
        {
            ReadToBuffer(1);
            return this.buffer[0];
        }

        public unsafe T ReadData<T>() where T: unmanaged
        {
            ReadToBuffer(sizeof(T) );
            fixed (void* ptr = &buffer[0])
            {
                return (T)(*(T*)ptr);
            }
        }
        public unsafe ushort ReadUshort()
        {
            ReadToBuffer(2);
            fixed (void* ptr = &buffer[0])
            {
                return (ushort)(*(ushort*)ptr);
            }
        }
        public unsafe int ReadInt()
        {
            ReadToBuffer(4);
            fixed (void* ptr = &buffer[0])
            {
                return (int)(*(int*)ptr);
            }
        }
        public unsafe uint ReadUint()
        {
            ReadToBuffer(4);
            fixed (void* ptr = &buffer[0])
            {
                return (uint)(*(uint*)ptr);
            }
        }
        public unsafe float ReadFloat()
        {
            ReadToBuffer(4);
            fixed (void* ptr = &buffer[0])
            {
                return (float)(*(float*)ptr);
            }
        }
        public unsafe ulong ReadUlong()
        {
            ReadToBuffer(8);
            fixed (void* ptr = &buffer[0])
            {
                return (ulong)(*(ulong*)ptr);
            }
        }

        public unsafe double ReadDouble()
        {
            ReadToBuffer(8);
            fixed (void* ptr = &buffer[0])
            {
                return (double)(*(double*)ptr);
            }
        }

        private int ReadToBuffer(int length)
        {
            if( length > buffer.Length)
            {
                buffer = new byte[length];
            }
            int read = readFs.Read(buffer, 0, length);
            return read;
        }



        private void CloseFile()
        {
            if (readFs != null)
            {
                readFs.Close();
            }
            readFs = null;
        }


        public void Dispose()
        {
            this.CloseFile();
        }
    }
}
