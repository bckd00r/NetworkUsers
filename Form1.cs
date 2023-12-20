using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkApp
{
    public partial class Form1 : Form
    {
        private bool _IsCollectedNetworkDatas = false;

        private string[] _IPAddressParameters;
        private List<IPHostEntry> _CollectedNetworkIPs = new List<IPHostEntry>();

        public bool IsCollectedNetworkDatas
        {
            get { return _IsCollectedNetworkDatas; }
            set { _IsCollectedNetworkDatas = value; }
        }

        public string[] IPAddressParameters
        { // if you want to port for other classes
            get { return _IPAddressParameters; }
            set { _IPAddressParameters = value; }
        }

        public List<IPHostEntry> CollectedNetworkIPs
        { // if you want to port for other classes
            get { return _CollectedNetworkIPs; }
            set { _CollectedNetworkIPs = value; }
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == NetworkInterfaceType.Ethernet && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            IPAddressParameters = ip.Address.ToString().Split('.');
                            MessageBox.Show("Current IP found " + string.Join(".", IPAddressParameters));
                            //output = ip.Address.ToString();
                        }
                    }
                }
            }

            if (!IsCollectedNetworkDatas)
            {

                string main_ip = string.Format("{0}.{1}.{2}.", IPAddressParameters.First(), IPAddressParameters[1], IPAddressParameters[2]);
                MessageBox.Show("Collecting network data");
                for (int i = 0; i <= 255; i++)
                {
                    Dns.BeginGetHostEntry(main_ip + i, new AsyncCallback(Stop), "test");
                    //Debug.WriteLine(i.ToString() + " Passed");
                    if (i == 255)
                        IsCollectedNetworkDatas = true;
                }
                MessageBox.Show("Network data successfully collected");
            }
        }

        public void Stop(IAsyncResult ar)
        {
            IPHostEntry ip_info = Dns.EndResolve(ar);
            if (ip_info == null || ip_info.AddressList.Length == 0)
                return;
            Debug.WriteLine(ip_info.HostName);
            CollectedNetworkIPs.Add(ip_info);

            Invoke(new Action(() =>
            {
                label2.Text = string.Format("Total found network users: {0}", CollectedNetworkIPs.Count());

            }));
            // label2.Text = string.Format("Total found network users: {0}", CollectedNetworkIPs.Count());

            foreach (string adres in ip_info.Aliases)
            {
                Debug.WriteLine(adres);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
             * Create a thread for loops and then continue the array per 30 ticks that would increase the performance
             */
            try
            {
                listBox1.Items.Clear();
                if (listBox1.Items.Count == 0)
                {
                    foreach (IPHostEntry ip_info in CollectedNetworkIPs)
                    {
                        string ips_on_host = string.Empty;
                        foreach (var ip in ip_info.AddressList)
                            ips_on_host += " - " + ip;

                        bool same_name_as_ip = ip_info.HostName.ToString().Equals(ip_info.AddressList.First().ToString());
                        if (same_name_as_ip)
                            continue;
                        /*
                         * You can colorize the same names if you want currently it seems not neccessary
                         * or you can filter same names as ip because it might be something else's ip address (router, virtual address etc...)
                         */

                        listBox1.Items.Add(string.Format("Name: {0} - {1}", same_name_as_ip ? "Name same as IP" : ip_info.HostName, ips_on_host));
                        //MessageBox.Show("Host name: " + ip_info.HostName + "\n- IP: " +  ip_info.AddressList.First() + "\n- Address List" + ip_info.AddressList.Length + "\n- Aliases" + ip_info.Aliases.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }
    }
}
