using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkServer
{
    class Program
    {
        static void Main(string[] args)
        {
            MemoryStream memStream = new MemoryStream(256);
            BinaryWriter writer = new BinaryWriter(memStream);
            
            
            writer.Write(5L);
            byte[] bigbuf = memStream.ToArray();
            System.Console.WriteLine("the bytes is " + bigbuf.Length);

            writer.Write(1.0f);
            memStream.Seek(0, SeekOrigin.Begin);
            writer.Seek(0, SeekOrigin.Begin);
            memStream.SetLength(0);
            byte[] floatbuf = memStream.ToArray();
            System.Console.WriteLine("the bytes is " + floatbuf.Length);
            

            PositionServer server = new PositionServer();
            server.StartServer(10001);
            Console.ReadKey();

        }
    }
}
