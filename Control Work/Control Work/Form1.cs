using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Control_Work
{
    public partial class Form1 : Form

    {
        string cn_string = ConfigurationManager.ConnectionStrings["Database1"].ConnectionString;
        [Serializable]
        class Mark
        {
            public int mark_id { get; set; }
            public string model_id { get; set; }
        }
        [Serializable]
        class Service
        {
            public int service_id { get; set; }
            public string name { get; set; }
            public int mark_id { get; set; }
            public int cost { get; set; }

        }
        List<Mark> model = new List<Mark>();
        List<Service> services = new List<Service>();
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(cn_string);

            conn.Open();
            SqlCommand cmdser = new SqlCommand("SELECT * FROM service", conn);
            SqlDataReader dr = cmdser.ExecuteReader();
            while (dr.Read())
            {
                comboBox1.Items.Add(dr["name"]);
                services.Add(new Service()
                {
                    service_id = ((int)dr["service_id"]),
                    name = dr["name"] as string,
                    mark_id = ((int)dr["mark_id"]),
                    cost = ((int)dr["cost"])

                });
            }
            conn.Close();

            conn.Open();
            SqlCommand cmdmark = new SqlCommand("SELECT mark.mark_id, service.mark_id, model_id FROM service JOIN mark ON service.mark_id=mark.mark_id", conn);
            SqlDataReader dr1 = cmdmark.ExecuteReader();

            while (dr1.Read())
            {
                comboBox2.Items.Add(dr1["model_id"]);
                model.Add(new Mark()
                {
                    model_id = dr1["model_id"] as string,
                    mark_id = ((int)dr1["mark_id"])
                });
            }
            conn.Close();
        }
        private string[] getModelById(int id)
        {
            return model.Where(line => line.mark_id == id).Select(l => l.model_id).ToArray();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();

            int id = services[comboBox1.SelectedIndex].service_id;
            label6.Text = services[comboBox1.SelectedIndex].cost.ToString();
            foreach (string model_id in getModelById(id))
            {
                this.comboBox2.Items.Add(model_id);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int quantity = int.Parse(textBox1.Text);
                decimal PRICE = int.Parse(label6.Text);
                decimal summa = (quantity * PRICE);

                textBox2.Text = summa.ToString();
            }
            catch
            {
                if ((textBox1.Text == ""))
                {
                    MessageBox.Show(
                    "Ошибка исходных данных.\n" + "Необходимо ввести данные в полe 'Количество'.",
                     "Подсчет",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Error);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            decimal quantity = int.Parse(textBox1.Text);
            int service_id = services[comboBox1.SelectedIndex].service_id;
            decimal usl_price = services[comboBox1.SelectedIndex].cost;

            decimal totalSum = int.Parse(textBox2.Text);
            textBox2.Text = totalSum.ToString();

            DateTime myDateTime = DateTime.Now;
            string sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");


            string sqlExpression = "INSERT INTO sell (quantity, service_id, date, cost) VALUES (@quantity, @service_id, @date, @cost); SET @sell_id=SCOPE_IDENTITY()";
           
            using (SqlConnection connection = new SqlConnection(cn_string))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);

                SqlParameter dateParam = new SqlParameter("@date", myDateTime);
                command.Parameters.Add(dateParam);
                SqlParameter quantityParam = new SqlParameter("@quantity", quantity);
                command.Parameters.Add(quantityParam);
                SqlParameter usl_idParam = new SqlParameter("@service_id", service_id);
                command.Parameters.Add(usl_idParam);

                SqlParameter totalsumParam = new SqlParameter("@cost", totalSum);
                command.Parameters.Add(totalsumParam);

                SqlParameter sell_idParam = new SqlParameter
                {
                    ParameterName = "@sell_id",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output // выходной
                };

                command.Parameters.Add(sell_idParam);

                command.ExecuteNonQuery();

            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8)
            {
                e.Handled = true;
            }
        }
    }
}


