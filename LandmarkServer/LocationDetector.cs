using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Math;

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
        private double radiusMove = 10.0;
        private int startTick;
        public LocationDetector()
        {
            startTick = System.Environment.TickCount;
            memStream = new MemoryStream(256);
            writer = new BinaryWriter(memStream);
        }
        MemoryStream memStream;
        BinaryWriter writer;
        PoseInfo GetPoseInfo()
        {
            PoseInfo info = new PoseInfo();
            int nowTick = System.Environment.TickCount - startTick;
            double nowTime = nowTick / 1000.0f;
            double theta = nowTime * 3.14 *2.0 / 5.0;
            info.fx = Math.Cos(theta) * radiusMove;
            info.fz = Math.Sin(theta) * radiusMove;
            info.dirx = Math.Cos(theta + 3.14 / 2.0);
            info.dirz = Math.Sin(theta + 3.14 / 2.0);
            return info;
        }
        public byte[] GetLocationData()
        {
            memStream.SetLength(0);
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(0);
            PoseInfo info = GetPoseInfo();
            info.Write(writer);
            return memStream.ToArray();
        }
        PoseInfo TransformPositionByAreaId(int areaid,PoseInfo infoL)
        {
            return infoL;
        }
        public byte[] GetGlobalLocationData()
        {
            memStream.SetLength(0);
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(0);
            PoseInfo info = GetPoseInfo();
            info = TransformPositionByAreaId(0, info);
            info.Write(writer);
            return memStream.ToArray();
        }
    }
}
