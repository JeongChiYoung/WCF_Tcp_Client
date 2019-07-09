using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceModel;
using WCF_Tcp_Client.WCFTcpServer;

namespace WCF_Tcp_Client
{
    public partial class WCFTcpClientForm : Form
    {
        WCFTcpServerClient serverHost = null;
        static WCFTcpClientForm MainForm = null;

        public WCFTcpClientForm()
        {
            InitializeComponent();

            MainForm = this;
        }

        void ConnectWCFService()
        {
            NetTcpBinding bindig = new NetTcpBinding();
            bindig.Security.Mode = SecurityMode.None;
            bindig.ReliableSession.Enabled = false;

            //콜백 인스턴스를 구현한 클래스를 아래와 같이 적용한다;
            InstanceContext ict = new InstanceContext(new ServerCallbackHandler());

            /*SomeClient 는 서비스참조에 의해 자동으로 생성된 클래스로서
                DuplexClientBase<ISome> 을 상속하고
                ISome 을 구현 한다
                (정의로 이동) 하여 내용을 볼수 있다. */
            serverHost = new WCFTcpServerClient(
                ict,
                bindig,
                new EndpointAddress("net.tcp://150.1.13.166/WCFTcpServer"));

            //호스트에 접속한다. 만약 호스트가 실행중이 아니라면 Exception..
            serverHost.Open();
            serverHost.StartService();
        }

        void DisconnectWCFService()
        {
            if (serverHost == null)
            {
                return;
            }

            if (serverHost.State == CommunicationState.Opened)
            {
                serverHost.StopService();
                serverHost.Close();
            }

            serverHost = null;
        }
        
        /// <summary>
        /// 호스트 로 부터 메시지를 받을 콜백 클래스
        /// </summary>>
        class ServerCallbackHandler : WCFTcpServer.IWCFTcpServerCallback
        {
            public void SetDataToClient(WCFMessageKind messageKind, object data)
            {
                MainForm.Invoke(new MethodInvoker(delegate ()
                {
                    switch (messageKind)
                    {
                        case WCFMessageKind.Message:
                            MainForm.textBoxMessage.Text = data.ToString();
                            break;
                    }
                }));
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (serverHost == null)
            {
                return;
            }

            serverHost.SetDataToServer(textBoxMessage.Text);
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            ConnectWCFService();
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectWCFService();
        }
    }
}
