using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Threading;

namespace PNServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Append("Program started...");
        }

        private PNHostRunner m_runner = null;
        PNClientRunner client = null;

        private void Append(string str)
        {
            textBox1.AppendText("\r\n" + str);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                m_runner = new PNHostRunner();
                m_runner.PNDataReceived += new PNWCFLib.PNDataReceivedEventHandler(m_runner_PNDataReceived);
                Thread t = new Thread(m_runner.StartHost);
                t.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }

        private delegate void _DataReceived(object sender, PNWCFLib.PNDataReceivedEventArgs e);
        void m_runner_PNDataReceived(object sender, PNWCFLib.PNDataReceivedEventArgs e)
        {
            if (textBox1.InvokeRequired)
            {
                _DataReceived d = new _DataReceived(m_runner_PNDataReceived);
                textBox1.Invoke(d, new object[] { sender, e });
            }
            else
            {
                Append(e.Data + " " + DateTime.Now.ToString("HH:mm:ss:fff"));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (m_runner != null)
            {
                m_runner.StopHost();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            client = new PNClientRunner();
            client.PNDataReceived += new PNWCFLib.PNDataReceivedEventHandler(client_PNDataReceived);
            client.PNDataError += new PNWCFLib.PNDataErrorEventHandler(client_PNDataError);
            // Returns a list of ipaddress configuration
            IPHostEntry ips = Dns.GetHostEntry(Dns.GetHostName());
            // Select the first entry. I hope it's this maschines IP
            IPAddress _ipAddress = ips.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            Thread t = new Thread(() => client.RunClient("hello",_ipAddress.ToString(), "27351"));

            t.Start();
        }

        void client_PNDataError(object sender, PNWCFLib.PNDataErrorEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }

        void client_PNDataReceived(object sender, PNWCFLib.PNDataReceivedEventArgs e)
        {
            if (textBox2.InvokeRequired)
            {
                
                _DataReceived d = new _DataReceived(client_PNDataReceived);
                textBox2.Invoke(d, new object[] { sender, e });
            }
            else
            {
                Append2(e.Data + " "+DateTime.Now.ToString("HH:mm:ss:fff"));
            }
        }

        private void Append2(string str)
        {
            textBox2.AppendText("\r\n" + str);
        }
    }
}
