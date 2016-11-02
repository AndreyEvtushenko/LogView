using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//пространство имён для работы с файлами
using System.IO;                       
//пространство имён для работы с регулярными выражениями
using System.Text.RegularExpressions;
//протранство имён для реализации экспорта в Excel
using Microsoft.Office.Interop.Excel;  

namespace loganalisator
{
    public partial class Form1 : Form
    {
        public Form1()
        {           
            InitializeComponent();
            ToolStripMenuItemGraph.Enabled = false;
            printToolStripMenuItem.Enabled = false;
            excelToolStripMenuItem.Enabled = false;
        }
        //контейнеры для хранения обработанных данных из файла
        public List<string> ListOFdate = new List<string>();
        public List<UInt32> ListOFhits = new List<UInt32>();
        public List<double> ListOFtraffic = new List<double>();
        List<UInt32> ListOFhosts = new List<UInt32>();
        SortedDictionary<string, double> PageTraffic = new SortedDictionary<string, double>();
        SortedDictionary<string, UInt32> PageVisits = new SortedDictionary<string, UInt32>();
    
        private void MenuItemOpen_Click(object sender, EventArgs e)
        {            
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;             
                string str = "";                                       
                FileInfo fileinfo = new FileInfo(filename);             
                double filesize = fileinfo.Length;
                //открываем файл для чтения и ассоциируем с ним поток
                FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read); 

                if (stream != null)                                     //если файл открыт
                {
                    toolStripStatusName.Text = "Путь: " + filename;
                    toolStripStatusSize.Text = "Рамер: " + String.Format("{0:0.00}", filesize /= 1048576) + "Мб";

                    dataGridView1.Rows.Clear();
                    dataGridView2.Rows.Clear();
                    ListOFdate.Clear();
                    ListOFhosts.Clear();
                    ListOFhits.Clear();
                    ListOFtraffic.Clear();
                    PageTraffic.Clear();
                    PageVisits.Clear();
                    //создаём объект StreamReader и ассоциируем его с открытым потоком
                    StreamReader reader = new StreamReader(stream); 
                    str = reader.ReadLine();                        
                    //шаблон регулярного выражения для проверки формата лог-файла
                    Regex CLF = new Regex(@"^\d+\.\d+\.\d+\.\d+ [\w+|-] [\w+|-] \[.+\] "".+"" \d+ \d+"); 

                    if (CLF.IsMatch(str) == true)
                    {
                        Match matchDate = Regex.Match(str, @"\d{2}/[A-z]{3}/\d{4}");    
                        Match matchIP = Regex.Match(str, @"\d+\.\d+\.\d+\.\d+");        
                        Match matchPage = Regex.Match(str, @" /([A-z/-]+(\.php|\.html|/)|)");
                        Match matchCodeTraffic = Regex.Match(str, @" \d{3} (\d+|-)");       
                        string[] arrayCodeTraffic = matchCodeTraffic.Value.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        string DateTmp = matchDate.Value;
                        double sumTraffic;
                        UInt32 uniqueip = 1;
                        UInt32 hits = 1;
                        string pageadresstmp = matchPage.Value;
                        double Traffictmp;
                        string pagevisitstmp = matchPage.Value;
                        UInt32 visitstmp = 1;
                        List<string> ListOFip = new List<string>();

                        if (arrayCodeTraffic[1] != "-")
                            sumTraffic = Convert.ToDouble(arrayCodeTraffic[1]);
                        else
                            sumTraffic = 0;

                        ListOFdate.Add(DateTmp);
                        ListOFip.Add(matchIP.Value);
                        PageTraffic.Add(matchPage.Value, sumTraffic);
                        PageVisits.Add(matchPage.Value, visitstmp);
                        //читаем до конца
                        while (!reader.EndOfStream)
                        {
                            str = reader.ReadLine();

                            matchDate = Regex.Match(str, @"\d{2}/[A-z]{3}/\d{4}");   
                            matchPage = Regex.Match(str, @" /([A-z/-]+(\.php|\.html|/)|)");                     
                            matchCodeTraffic = Regex.Match(str, @" \d{3} (\d+|-)");
                            matchIP = Regex.Match(str, @"\d+\.\d+\.\d+\.\d+");       
                            arrayCodeTraffic = matchCodeTraffic.Value.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            if (arrayCodeTraffic[1] != "-")
                                Traffictmp = Convert.ToDouble(arrayCodeTraffic[1]);
                            else
                                Traffictmp = 0;

                            if (matchDate.Value == DateTmp)                            
                            {
                                int index = ListOFip.BinarySearch(matchIP.Value);   
                                if (index < 0)
                                {
                                    uniqueip++;
                                    ListOFip.Insert(~index, matchIP.Value);
                                }

                                if (matchPage.Value != pageadresstmp)
                                {
                                    hits++;
                                    pageadresstmp = matchPage.Value;
                                }

                                if (arrayCodeTraffic[1] != "-")
                                    sumTraffic += Convert.ToDouble(arrayCodeTraffic[1]);
                            }
                            else
                            {
                                ListOFhosts.Add(uniqueip);
                                ListOFhits.Add(hits);
                                ListOFtraffic.Add(sumTraffic);

                                dataGridView1.Rows.Add(
                                    ListOFdate.Last(),
                                    ListOFhosts.Last(),
                                    ListOFhits.Last(),
                                    String.Format("{0:0.00}", sumTraffic /= 1048576));

                                ListOFdate.Add(matchDate.Value);

                                uniqueip = 1;
                                DateTmp = matchDate.Value;
                                hits = 1;
                                pageadresstmp = matchPage.Value;
                                ListOFip.Clear();
                                ListOFip.Add(matchIP.Value);

                                if (arrayCodeTraffic[1] != "-")
                                    sumTraffic = Convert.ToUInt32(arrayCodeTraffic[1]);
                            }

                            if (!PageTraffic.ContainsKey(matchPage.Value))
                                PageTraffic.Add(matchPage.Value, Traffictmp);
                            else
                                PageTraffic[matchPage.Value] += Traffictmp;

                            if (!PageVisits.ContainsKey(matchPage.Value))
                                PageVisits.Add(matchPage.Value, visitstmp = 1);
                            else if (matchPage.Value != pagevisitstmp)
                                PageVisits[matchPage.Value]++;
                            pagevisitstmp = matchPage.Value;

                        }
                        ListOFtraffic.Add(sumTraffic);
                        ListOFhits.Add(hits);
                        dataGridView1.Rows.Add(DateTmp, uniqueip, hits, String.Format("{0:0.00}", sumTraffic /= 1048576));

                        foreach (KeyValuePair<string, double> kvp in PageTraffic)
                        {
                            Traffictmp = kvp.Value;
                            dataGridView2.Rows.Add(kvp.Key, String.Format("{0:0.00}", Traffictmp /= 1048576), PageVisits[kvp.Key]);
                        }

                        ToolStripMenuItemGraph.Enabled = true;
                        printToolStripMenuItem.Enabled = true;
                        excelToolStripMenuItem.Enabled = true; 
                        dataGridView1.Focus();
                    }
                    else 
                    {
                        FormChart frm = new FormChart();
                        frm.Text = "Предупреждение";
                        frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                        frm.Width = 500;
                        frm.Height = 130;
                        frm.MaximizeBox = false;
                        frm.MinimizeBox = false;
                        frm.StartPosition = FormStartPosition.CenterScreen;
                        frm.chart1.Visible = false;
                        frm.chart2.Visible = false;
                        frm.labelHelp.Visible = true;
                        frm.buttonOK.Visible = true;
                        frm.buttonChart.Visible = false;
                        frm.buttonOK.Location = new System.Drawing.Point(200, 50);
                        frm.buttonOK.DialogResult = DialogResult.OK;
                        frm.labelHelp.Text = "Записи лог файла не соответствуют формату Common Log Format";
                        frm.Show(this);

                        ToolStripMenuItemGraph.Enabled = false;
                        printToolStripMenuItem.Enabled = false;
                        excelToolStripMenuItem.Enabled = false;
                    }
                    stream.Close();
                }
            }
        }

        private void pageStatisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Visible == true)
            {
                dataGridView1.Visible = false;
                dataGridView2.Visible = true;
                pageStatisticsToolStripMenuItem.Text = "Общая статистика";
                dataGridView2.Focus();
            }
            else
            {
                dataGridView1.Visible = true;
                dataGridView2.Visible = false;
                pageStatisticsToolStripMenuItem.Text = "Статистика по страницам";
                dataGridView2.Focus();
            }
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Bitmap bmp = new Bitmap(dataGridView1.Size.Width + 10, dataGridView1.Size.Height + 10);
            dataGridView1.DrawToBitmap(bmp, dataGridView1.Bounds);
            e.Graphics.DrawImage(bmp, 0, 0);
        }

        private void printDocument2_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Bitmap bmp = new Bitmap(dataGridView2.Size.Width + 10, dataGridView2.Size.Height + 10);
            dataGridView2.DrawToBitmap(bmp, dataGridView2.Bounds);
            e.Graphics.DrawImage(bmp, 0, 0);
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = printDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                if(dataGridView1.Visible == true)
                    printDocument1.Print();
                else
                    printDocument2.Print();
            }       
        }

        private void excelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
            ExcelApp.Application.Workbooks.Add(Type.Missing);

            if (dataGridView1.Visible == true)
            {
                ExcelApp.Columns.ColumnWidth = 10;

                ExcelApp.Cells[1, 1] = "Дата";
                ExcelApp.Cells[1, 2] = "Хосты";
                ExcelApp.Cells[1, 3] = "Просмотры";
                ExcelApp.Cells[1, 4] = "Трафик(Мб)";

                for (int i = 0; i < dataGridView1.ColumnCount; i++)
                    for (int j = 0; j < dataGridView1.RowCount; j++)
                        ExcelApp.Cells[j + 2, i + 1] = dataGridView1[i, j].Value;
            }
            else
            {
                ExcelApp.Columns.ColumnWidth = 10;
                ExcelApp.Columns[1].ColumnWidth = 40;

                ExcelApp.Cells[1, 1] = "Страница";
                ExcelApp.Cells[1, 2] = "Посещения";
                ExcelApp.Cells[1, 3] = "Трафик(Мб)";

                for (int i = 0; i < dataGridView2.ColumnCount; i++)
                    for (int j = 0; j < dataGridView2.RowCount; j++)
                        ExcelApp.Cells[j + 2, i + 1] = dataGridView2[i, j].Value;
            }
            ExcelApp.Visible = true;
        }

        private void ToolStripMenuItemHelp_Click(object sender, EventArgs e)
        {
            FormChart frm = new FormChart();
            frm.Text = "Справка";
            frm.FormBorderStyle = FormBorderStyle.FixedDialog;            
            frm.Width = 520;
            frm.Height = 250;
            frm.MaximizeBox = false;
            frm.MinimizeBox = false;
            frm.StartPosition = FormStartPosition.CenterScreen;
            frm.chart1.Visible = false;
            frm.chart2.Visible = false;
            frm.labelHelp.Visible = true;
            frm.buttonOK.Location = new System.Drawing.Point(220, 160);
            frm.buttonOK.Visible = true;
            frm.buttonChart.Visible = false;
            frm.buttonOK.DialogResult = DialogResult.OK;
            frm.labelHelp.Text = "Программа анализирует лог файлы сервера Apache. Лог файл должен\n" +
                                "быть в записан в формате Common Log Format.\n"+
                                "\"Открыть\" - открывает диалоговое окно выбора лог файла\n" +                                 
                                "\"Статистика по страницам/общая статистика\" - Переключение между\n" +
                                "таблицами\n" + 
                                "\"График\" - рисует график\n" + 
                                "\"Печать\" - печатает текущий фрагмент таблицы в формате bitmap\n" + 
                                "\"Excel\" - экспортирует активную таблицу в Excel";
            frm.Show(this);
        }

        private void ToolStripMenuItemGraph_Click(object sender, EventArgs e)
        {
            FormChart frm = new FormChart();
            frm.Owner = this;
            frm.Text = "График";
            frm.FormBorderStyle = FormBorderStyle.Sizable;
            frm.MaximizeBox = true;
            frm.MinimizeBox = true;
            frm.buttonOK.Visible = false;
            frm.buttonChart.Visible = true;
            frm.Show(this);
        }     
    }
}
