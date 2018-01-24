using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace LandmarkServer

{
    class PoseInfo
    {
        public double fx;
        public double fz;
        public double dirx;
        public double dirz;
        public void Write(BinaryWriter writer)
        {
            writer.Write(fx);
            writer.Write(fz);
            writer.Write(dirx);
            writer.Write(dirz);
        }
    }
    class LocationDetector
    {
        public LocationDetector()
        {
            memStream = new MemoryStream(256);
            writer = new BinaryWriter(memStream);
        }
        MemoryStream memStream;
        BinaryWriter writer;
        PoseInfo GetPoseInfo()
        {
            return new PoseInfo();
        }
        public byte[] getLocationData()
        {
            memStream.SetLength(0);
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(0);
            PoseInfo info = GetPoseInfo();
            info.Write(writer);
            return memStream.ToArray();
        }
    }
}
