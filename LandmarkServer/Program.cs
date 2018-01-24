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
          
            PositionServer server = new PositionServer();
            server.StartServer(10001);
            Console.ReadKey();

        }
    }
}
