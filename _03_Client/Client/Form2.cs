using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace _49_NetworkStreamMultiThreadFormClient
{
    public partial class Form2 : Form
    {
        private OracleConnection conn;
        private string dbConnInfo = @"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))
                                    (CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=xe))); User Id = aiameta; Password = aiameta;";

        public Form2()
        {
            InitializeComponent();

            this.Load += Form2_Load;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            conn = new OracleConnection(dbConnInfo);
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_Join_Click(object sender, EventArgs e)
        {
            try
            {
                InsertAddress();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"DB Error : {ex.Message}");
            }
        }

        public void InsertAddress()
        {
            string insertAddressSql = @"INSERT INTO AIAMEMBERS(ID, PASSWORD)" +
                            $"VALUES('{textBox_ID.Text}','{textBox_Password.Text}')";
            try
            {

                OracleCommand cmd = new OracleCommand(dbConnInfo, conn);
                cmd.Connection = conn;

                if (textBox_ID.Text != "" && textBox_Password.Text != "")
                {
                    cmd.CommandText = insertAddressSql;
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("회원가입완료!");
                }
                else
                    MessageBox.Show("아이디와 패스워드를 입력해주세요");


            }
            catch (Exception ex)
            {
                conn = null;
                Console.WriteLine($"DB Error : {ex.Message}");
            }
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
