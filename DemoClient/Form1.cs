using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
           
        }

        private TcpClient m_client;
        private NetworkStream m_stream;
        private BinaryWriter m_cmdWriter;
        private BinaryReader m_datareader;
        private void timeForQueryPose(object sender, EventArgs e)
        {
            // cmd 2 位查询命令 
            // cmd 2 查询全局考场坐标和方位
            m_cmdWriter.Write((byte)2);
            // 先得到区域ID,用于标志 考场的类型,比如是倒车入库,还是侧方位停车 ....
            int areaid = m_datareader.ReadInt32();
            // 得到位置
            double x = m_datareader.ReadDouble();
            double y = m_datareader.ReadDouble();
            // 得到方位
            double fx = m_datareader.ReadDouble();
            double fy = m_datareader.ReadDouble();
            labelPose.Text = String.Format("areaid {0} ,({1},{2}) dir ({3},{4})", areaid, x, y, fx, fy);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // 连接本地
            if (m_client == null )
            {
                // 连接本地的10001端口
                m_client = new TcpClient("127.0.0.1",10001);
                m_stream = m_client.GetStream();
                labelStatus.Text = "服务器已经连接";
                m_cmdWriter = new BinaryWriter(m_stream);
                m_datareader = new BinaryReader(m_stream);
                timerQuery.Enabled = true;
                timerQuery.Tick += new EventHandler(this.timeForQueryPose);
                timerQuery.Start();
            }
           
        }
    }
}
