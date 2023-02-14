using Microsoft.Win32.SafeHandles;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _49_NetworkStreamMultiThreadFormClient
{
    public partial class Form1 : Form
    {
        private bool IsRun = false;
        private OracleConnection conn;
        private string dbConnInfo = @"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))
                                    (CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=xe))); User Id = aiameta; Password = aiameta;";

        Socket clientSocket;
        IPEndPoint ipep;        // 서버의 접속 주소
        bool isRunRecv = true;  // 수신 스레드의 계속 동작 여부

        NetworkStream ns;
        StreamWriter sw;
        StreamReader sr;

        delegate void AddMsgLogData(string data);
        AddMsgLogData addMsgLogData = null;


        delegate void StopState();
        StopState stopButtonState = null;

        public Form1()
        {
            InitializeComponent();

            this.Load += Form1_Load;
            this.FormClosed += Form1_FormClosed;

            this.button_Login.Click += Button_Login_Click;
        }

        private void Button_Login_Click(object sender, EventArgs e)
        {
            conn = new OracleConnection(dbConnInfo);
            conn.Open();
            IsRun = true;
            string LOGINID = "SELECT * FROM AIAMEMBERS ";
            LOGINID += $"WHERE ID = '{textBox_ID.Text}'";
            try
            {
                
               
              
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;
                cmd.CommandText = LOGINID;
                OracleDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    
                    if (textBox_Password.Text != reader["PASSWORD"].ToString())
                        MessageBox.Show("삐빅! 로그인실패!");
                    MessageBox.Show("삐빅! 로그인성공!");
                    Login_Log(sender, e);
                }
                conn.Close();
                conn.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERROR : {ex.Message}");
            }
            conn.Close();
            conn.Dispose();
        }

        public string LoginID
        {
            get;
            private set;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.addMsgLogData = AddMsgLogListBox;
            this.stopButtonState = StopButtonState;

            button_Disconnect.Enabled = false;
            textBox_SendData.Enabled = false;
            conn = new OracleConnection(dbConnInfo);
            try
            {
                conn.Open();
                if (conn.State == ConnectionState.Open)
                {
                    MessageBox.Show("오라클 서버연동!!");
                }
            }
            catch (Exception ex)
            {
                conn = null;
                MessageBox.Show($"서버연동실패{ex.Message}");
            }
            conn.Close();
            conn.Dispose();
        }

        public void InsertAddress()
        {
            string insertAddressSql = @"INSERT INTO AIAMEMBERS(ID, PASSWORD) VALUES(:ID, :PASSWORD)";

            // 자동으로 데이터베이스를 연결한다.
            try
            {
                OracleCommand cmd = new OracleCommand(insertAddressSql, conn);
                cmd.Connection = conn;
                cmd.CommandText = insertAddressSql;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                conn = null;
                Console.WriteLine($"DB Error : {ex.Message}");
            }
            conn.Close();
            conn.Dispose();

        }

        void AddMsgLogListBox(string data)
        {
            if (listBox_Log.InvokeRequired)
            {
                Invoke(addMsgLogData, new object[] { data });
            }
            else
            {
                listBox_Log.Items.Add(data);
                listBox_Log.SelectedIndex = listBox_Log.Items.Count - 1;
            }
        }

        private void button_Clear_Click(object sender, EventArgs e)
        {
            listBox_Log.Items.Clear();
        }

        void ThreadRecv()
        {
            while (this.isRunRecv)
            {
                string data = null;
                try
                {
                    data = sr.ReadLine();
                    if (data == null)
                        this.isRunRecv = false;
                    else
                    {
                        CmdPacket cmd = JsonSerializer.Deserialize<CmdPacket>(data);
                        switch (cmd.CMD)
                        {
                            // 명령을 확인한 후 어떤 클래스로 변환할 지 결정함
                            case 'C':
                                ChatPacket cp = JsonSerializer.Deserialize<ChatPacket>(data);
                                AddMsgLogListBox($"{cp.ID} >> {cp.PASSWORD}");
                                break;
                        }
                    }
                }
                catch (JsonException ex)
                {
                    AddMsgLogListBox($"Json Error : {ex.Message}");
                    AddMsgLogListBox($"Json Data : {data}");
                }
                catch (Exception ex)
                {
                    AddMsgLogListBox($"Recv Error : {ex.Message}");

                    this.isRunRecv = false;
                }
            }

            Invoke(stopButtonState, null);
        }
        void StopButtonState()
        {
            button_Connect.Enabled = true;
            button_Disconnect.Enabled = false;
            textBox_SendData.Enabled = false;
        }

        private void button_Connect_Click(object sender, EventArgs e)
        {
            this.clientSocket = new Socket(AddressFamily.InterNetwork,
                                            SocketType.Stream,
                                            ProtocolType.Tcp);
            /*
            this.ipep = new IPEndPoint(IPAddress.Parse(textBox_ServerIP.Text),
                                        Int32.Parse(textBox_Port.Text));
            */
            this.ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9000);


            AddMsgLogListBox("서버 접속 시도...");
            this.clientSocket.Connect(ipep);
            AddMsgLogListBox("서버 접속 연결!!!");

            this.ns = new NetworkStream(this.clientSocket);
            this.sw = new StreamWriter(this.ns);
            this.sr = new StreamReader(this.ns);

            this.isRunRecv = true;
            Thread threadRecv = new Thread(new ThreadStart(ThreadRecv));
            threadRecv.Start();

            button_Connect.Enabled = false;
            button_Disconnect.Enabled = true;
            textBox_SendData.Enabled = true;
        }

        private void button_Disconnect_Click(object sender, EventArgs e)
        {
            if (this.clientSocket != null &&
                this.clientSocket.Connected)
            {
                this.clientSocket.Close();
            }

            button_Connect.Enabled = true;
            button_Disconnect.Enabled = false;
            textBox_SendData.Enabled = false;
        }

        private void textBox_SendData_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.Enter:

                    break;
            }
        }
        private void Login_Log(object sender, EventArgs e)
        {
            ChatPacket cp = new ChatPacket();
            cp.CMD = 'C';
            cp.ID = textBox_ID.Text;
            cp.PASSWORD = textBox_Password.Text;
            // json문자열로 변환
            string data = JsonSerializer.Serialize(cp);
            this.sw.WriteLine(data);
            this.sw.Flush();
            ThreadRecv();
            AddMsgLogListBox($"전송 : {cp.ID} {cp.PASSWORD}");
            //AddMsgLogListBox($"전송 : {data}");
            textBox_SendData.Clear();
        }
        private void label4_Click(object sender, EventArgs e)
        {

        } //필요없음

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        } //필요없음

        private void button_Join_Click(object sender, EventArgs e)
        {
            IsRun = true;
            conn = new OracleConnection(dbConnInfo);
            try
            {
                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    MessageBox.Show("회원가입 창으로 이동합니다");
                }
            }
            catch (Exception ex)
            {
                conn = null;
                MessageBox.Show($"서버연동실패{ex.Message}");
            }
            Form2 signup = new Form2();
            signup.ShowDialog();
        }

        private void button_Login_Click(object sender, EventArgs e)
        {


        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }

    class CmdPacket
    {
        public char CMD { get; set; }
    }
    class ChatPacket : CmdPacket
    {
        public string ID { get; set; }
        public string PASSWORD { get; set; }


    }

}
