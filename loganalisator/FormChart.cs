using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace loganalisator
{
    public partial class FormChart : Form
    {
        public FormChart()
        {           
            InitializeComponent();
            buttonChart.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
            buttonChart.BringToFront();
            buttonChart.Text = "Дата/Просмотры";
        }

        private void FormChart_Load(object sender, EventArgs e)
        {
            Form1 frm = (Form1)this.Owner;

            if (frm.ListOFtraffic.Count() > 0)
            {
                chart2.Visible = false;
                //настройка графика Дата/трафик
                chart1.Series.Add("DateTraffic");
                chart1.Series[0].ChartType = SeriesChartType.Line;
                chart1.Series[0].Color = Color.Black;
                chart1.Series[0].BorderWidth = 5;

                chart1.ChartAreas.Add("DateTraffic");
                chart1.ChartAreas[0].AxisX.Interval = 1;
                chart1.ChartAreas[0].AxisY.Interval = 5;
                chart1.ChartAreas[0].AxisX.TitleAlignment = StringAlignment.Center;
                chart1.ChartAreas[0].AxisX.TitleFont = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Underline);
                chart1.ChartAreas[0].AxisX.Title = "Дата";
                chart1.ChartAreas[0].AxisY.TitleAlignment = StringAlignment.Center;
                chart1.ChartAreas[0].AxisY.TitleFont = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Underline);

                if (frm.ListOFtraffic.Max() > 1048576)
                {                    
                    chart1.ChartAreas[0].AxisY.Title = "Трафик(Мб)";

                    for (int i = 0; i < frm.ListOFtraffic.Count(); i++)
                    {
                        double tmp = frm.ListOFtraffic[i] / 1048576;
                        chart1.Series[0].Points.AddXY(frm.ListOFdate[i], tmp);
                    }
                }

                else if (frm.ListOFtraffic.Max() < 1048576)
                {
                    chart1.ChartAreas[0].AxisY.Title = "Трафик(Кб)";

                    for (int i = 0; i < frm.ListOFtraffic.Count(); i++)
                    {
                        double tmp = frm.ListOFtraffic[i] / 1024;
                        chart1.Series[0].Points.AddXY(frm.ListOFdate[i], tmp);
                    }
                }
                //настройка графика Дата/просмотры
                chart2.Series.Add("DateHits");
                chart2.Series[0].ChartType = SeriesChartType.Line;
                chart2.Series[0].Color = Color.Black;
                chart2.Series[0].BorderWidth = 5;

                chart2.ChartAreas.Add("DateHits");
                chart2.ChartAreas[0].AxisX.Interval = 1;
                chart2.ChartAreas[0].AxisX.TitleAlignment = StringAlignment.Center;
                chart2.ChartAreas[0].AxisX.TitleFont = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Underline);
                chart2.ChartAreas[0].AxisX.Title = "Дата";
                chart2.ChartAreas[0].AxisY.TitleAlignment = StringAlignment.Center;
                chart2.ChartAreas[0].AxisY.TitleFont = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Underline);
                
                chart2.ChartAreas[0].AxisY.Title = "Просмотры";

                for (int i = 0; i < frm.ListOFhits.Count(); i++)
                {                
                    chart2.Series[0].Points.AddXY(frm.ListOFdate[i], frm.ListOFhits[i]);
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonChart_Click(object sender, EventArgs e)
        {

            if (chart1.Visible == true)
            {
                chart1.Visible = false;
                chart2.Visible = true;
                buttonChart.Text = "Дата/Трафик";
            }
            else
            {
                chart2.Visible = false;
                chart1.Visible = true;
                buttonChart.Text = "Дата/Просмотры";
            }
        }
    }
}
