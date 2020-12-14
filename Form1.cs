using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Globalization;

namespace file_in_pc
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
        }
        /// <summary>
        /// Директория для сканирования
        /// </summary>
        string path = "";
        /// <summary>
        /// Регулярное выражение по критерям которого будет происходить поиск
        /// </summary>
        string rex = "";
        /// <summary>
        /// Файл с параметрами для сохранения
        /// </summary>
        string setting_file = "set.dat";
        /// <summary>
        /// Список непроверенных директорий в случае прерывания проверки
        /// </summary>
        List<string> not_over = new List<string>();
        /// <summary>
        /// Главный поток, который производит поиск, запускает по нажатию на button1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            find_file find_File = new find_file();           
            find_File.find(path, rex, this, ref not_over);
            backgroundWorker2.WorkerSupportsCancellation = true;
            backgroundWorker2.CancelAsync();
            button1.BeginInvoke((Action)(() => button1.Enabled = true));
            button2.BeginInvoke((Action)(() => button2.Enabled = false));
        }

        /// <summary>
        /// Функция прорисовки на treeview
        /// </summary>
        /// <param name="files"> Найденные файлы в выбранной папке </param>
        /// <returns> Возвращает флаг прерывания потока: true - поток вызван к завершению; false - иначе. </returns> 
        public bool View_it_tree(string[] files)
        {
            if (files.Length == 0)
            {
                if (backgroundWorker1.CancellationPending || backgroundWorker3.CancellationPending) { return true; } else { return false; }
            }
            treeView1.Invoke((Action)(() => treeView1.BeginUpdate()));
            long ntime = DateTime.Now.Ticks;
            foreach (var fi in files)
            {
                if (backgroundWorker1.CancellationPending || backgroundWorker3.CancellationPending)
                {
                    treeView1.Invoke((Action)(() => treeView1.EndUpdate()));
                    return true; // вызов прекращения потока, вызывается при прорисовки каждого файла
                } 
                List<string> s = fi.Split('\\').ToList();
                TreeNode[] all = null;
                TreeNode nowN = null;
                try
                {
                    if (treeView1.Nodes[0].Text.Equals(s[0])) { nowN = treeView1.Nodes[0]; } else { treeView1.Invoke((Action)(() => nowN = treeView1.Nodes.Add(s[0], s[0]))); }
                }
                catch
                {
                    treeView1.Invoke((Action)(() => nowN = treeView1.Nodes.Add(s[0], s[0])));
                    nowN = treeView1.Nodes[0];
                }
                for (int j = 1; j < s.Count; j++)
                {   
                    string k = s[j];
                    treeView1.Invoke((Action)(() => all = nowN.Nodes.Find(k, false)));
                    if (all != null)
                        if (all.Length > 0)
                        {
                            nowN = all[0];
                        } else
                        {
                            treeView1.Invoke((Action)(() => nowN = nowN.Nodes.Add(k, k))); 
                            
                        }  
                }
                if ((DateTime.Now.Ticks - ntime)/TimeSpan.TicksPerMillisecond >= 17) //происовка чтобы не мерцало
                {
                    ntime = DateTime.Now.Ticks;
                    treeView1.Invoke((Action)(() => treeView1.EndUpdate()));
                }            
            }            
            treeView1.Invoke((Action)(() => treeView1.EndUpdate()));
            label3.Invoke((Action)(() => label3.Text = (Convert.ToInt32(label3.Text) + files.Length).ToString()));
            return false;
        }
        /// <summary>
        /// Отображения количество всего файлов
        /// </summary>
        /// <param name="count"> Количество файлов </param>
        /// <returns> Прерывание от закрытия потока </returns>
        public bool view_all_file(int count)
        {
            if (backgroundWorker1.CancellationPending || backgroundWorker3.CancellationPending) { return true; } 
                if (count == 0)
                {
                    return false;
                } else
            {
                label4.Invoke((Action)(() => label4.Text = (Convert.ToInt32(label4.Text) + count).ToString()));
            }
            return false;
        }
        /// <summary>
        /// Отображение текущей директории
        /// </summary>
        /// <param name="dir"> Директория для отображения </param>
        public void View_dir(string dir)
        {
            label6.Invoke((Action)(() => label6.Text = dir));
        }
        /// <summary>
        /// Кнопка "Начать"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            not_over = new List<string>();
            if (textBox1.Text != rex)
            {
                rex = textBox1.Text;
                string[] f = File.ReadAllLines(setting_file);
                for (int i = 0; i < f.Length; i++)
                {
                    if (f[i].Contains("rex = ")) { f[i] = "rex = " + rex; }
                }
                File.WriteAllLines(setting_file, f);
            }
            treeView1.Nodes.Clear();
            label3.Text = "0";
            label4.Text = "0";
            label6.Text = "//";
            label2.Text = DateTime.MinValue.ToString("HH:mm:ss:ffff");
            button1.Enabled = false;
            button2.Enabled = true;
            button4.Enabled = false;
            
            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.RunWorkerAsync(); 
        }

        private void Form1_Load(object sender, EventArgs e)
        {           
            try
            {
                string[] f = File.ReadAllLines(setting_file);
                foreach (var line in f)
                {
                    if (line.Contains("path = "))
                    {
                        path = line.Substring(7);
                    }
                    if (line.Contains("rex = "))
                    {
                        rex = line.Substring(6);                    
                    }
                }
            }
            catch
            {
                FileStream file = File.Create(setting_file);
                byte[] ini = new UTF8Encoding(true).GetBytes("path = \"\"\nrex = \"\"");
                file.Write(ini, 0, ini.Length);
                path = "";
                rex = "";
                file.Close();
            }
            label7.Text = path;
            textBox1.Text = rex;
        }
        /// <summary>
        /// Кнопка "Закончить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.WorkerSupportsCancellation = true;
                backgroundWorker1.CancelAsync();
            }
            if (backgroundWorker2.IsBusy)
            {
                backgroundWorker2.WorkerSupportsCancellation = true;
                backgroundWorker2.CancelAsync();
            }
            if (backgroundWorker3.IsBusy)
            {
                backgroundWorker3.WorkerSupportsCancellation = true;
                backgroundWorker3.CancelAsync();
            }
            button1.Enabled = true;
            button2.Enabled = false;
            button4.Enabled = true;
        }
        /// <summary>
        /// Поток отсчитывающий время выполнения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            TimeSpan data;
            try
            {
                data = DateTime.Now.Subtract(DateTime.ParseExact(label2.Text, "HH:mm:ss:ffff", provider));
            }
            catch
            {
                data = new TimeSpan(DateTime.Now.Ticks);
            }         
            label2.Invoke((Action)(() => label2.Text = data.ToString()));
            do
            {
                if (backgroundWorker2.CancellationPending)
                    return;
                label2.BeginInvoke((Action)(() => label2.Text = DateTime.Now.Subtract(data).ToString("HH:mm:ss:ffff")));
                Thread.Sleep(10);
            } while (true);
        }
        /// <summary>
        /// Поток для продолжения поиска при прерывании
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < not_over.Count; i++)
            {
                if (backgroundWorker3.CancellationPending)
                    return;
                string sub = not_over[0];
                not_over.RemoveAt(0);
                find_file find_File = new find_file();               
                find_File.find(sub, rex, this, ref not_over);
            }
            backgroundWorker2.WorkerSupportsCancellation = true;
            backgroundWorker2.CancelAsync();
            button1.BeginInvoke((Action)(() => button1.Enabled = true));
            button2.BeginInvoke((Action)(() => button2.Enabled = false));

        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Выбор папки поиска
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                path = folderBrowserDialog1.SelectedPath;
                label7.Text = path;
                string[] f = File.ReadAllLines(setting_file);
                for (int i = 0; i < f.Length; i++)
                {
                    if (f[i].Contains("path = ")) { f[i] = "path = " + path; }
                }
                File.WriteAllLines(setting_file, f);
            }
        }
        /// <summary>
        /// Кнопка "Продолжить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            button2.Enabled = true;
            button1.Enabled = false;
            backgroundWorker3.RunWorkerAsync();
            backgroundWorker2.RunWorkerAsync();
        }
    }
}
