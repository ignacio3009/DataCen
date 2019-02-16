using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataCen
{
    public partial class Form1 : Form
    {
        bool firstTime = true;
        bool detenido = true;
        Timer timer = new Timer();
        

        public Form1()
        {
            InitializeComponent();

        }

        public void mainRutine(object sender, EventArgs e)
        {
            getFiles();
        }

        public static void createDataFolder()
        {

            string path = getDataFolder();
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
            return;
        }
        public static string getDataFolder()
        {
            string file_path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            string path = file_path + @"\data";
            return path;
        }
        public static void getCmgCen()
        {
            string url_costo_marginal = "https://www.coordinador.cl/wp-json/costo-marginal/v1/data";
            var client = new WebClient();
            createDataFolder();
            client.DownloadFile(url_costo_marginal, getDataFolder() + @"\CostoMarginal_CEN.csv");
        }
        public static void getDemandaCen()
        {
            string url_demanda = "https://sic.coordinadorelectrico.cl/index.php/wp-content/uploads/graficos-online/curvapato.csv";
            var client = new WebClient();
            createDataFolder();
            client.DownloadFile(url_demanda, getDataFolder() + @"\Demanda_CEN.csv");
        }
        public static void formatFiles()
        {
            string data_folder = getDataFolder();
            string dda_file_cen = data_folder + @"\Demanda_CEN.csv";
            string cmg_file_cen = data_folder + @"\CostoMarginal_CEN.csv";
            string dda_file_final = data_folder + @"\Demanda.csv";
            string cmg_file_final = data_folder + @"\CostoMarginal.csv";

            formatDemanda();
            formatCmg();


        }
        public static string[] formatCmg()
        {
            string data_folder = getDataFolder();
            string cmg_file_cen = data_folder + @"\CostoMarginal_CEN.csv";
            string cmg_file_final = data_folder + @"\CostoMarginal.csv";
            string text = File.ReadAllText(cmg_file_cen);
            text = text.Replace("\"", "");

            string[] separaters = { "}," };
            string[] data = text.Split(separaters, StringSplitOptions.None);

            for (int i = 0; i < data.Length; i++)
            {
                string t = data[i];
                t = t.Replace("fecha:", "");
                t = t.Replace("barra:", "");
                t = t.Replace("cmg:", "");
                t = t.Replace("[", "");
                t = t.Replace("]", "");
                t = t.Replace("{", "");
                t = t.Replace("}", "");

                data[i] = t;

            }
            //File.WriteAllLines(cmg_file_final, data);
            return data;
        }
        public static string dateFormat(string str)
        {
            string year = str.Substring(0, 4);
            string month = str.Substring(4, 2);
            string day = str.Substring(6, 2);
            return year + "-" + month + "-" + day;

        }
        public static string hourFormat(string str)
        {
            string hour = str.Substring(0, 2);
            string minutes = str.Substring(2, 2);
            string seconds = str.Substring(4, 2);
            return hour + ":" + minutes + ":" + seconds;
        }
        public static string[] formatDemanda()
        {
            string data_folder = getDataFolder();
            string dda_file_cen = data_folder + @"\Demanda_CEN.csv";
            string dda_file_final = data_folder + @"\Demanda.csv";

            string[] ArrayDemanda = new string[5000];
            string[] lines = File.ReadAllLines(dda_file_cen);
            int idx = 0;
            string demanda_key_to_eliminate = "diferencia";
            foreach (string line in lines)
            {
                string[] row = line.Split(',');
                if (!line.Contains(demanda_key_to_eliminate) && row.Length > 1)
                {
                    string s = dateFormat(row[0]) + " " + hourFormat(row[1]) + "," + row[2] + "," + row[3] + ",";
                    double dif = double.Parse(row[2]) - double.Parse(row[3]);
                    s = s + dif.ToString("0.######");
                    ArrayDemanda[idx] = s;
                    idx++;
                }
            }

            //File.WriteAllLines(dda_file_final, ArrayDemanda);
            return ArrayDemanda;
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (detenido)
            {
                label5.Text = "Conectado";
                getFiles();
                int minutes = Convert.ToInt32(Math.Round(numericUpDown1.Value, 0));
                timer.Interval = (60 * 1000 * minutes);
                timer.Tick += new EventHandler(mainRutine);
                timer.Start();
                detenido = false;
            }
            else
            {
                timer.Stop();
                label5.Text = "Pausado";
            }
        }

        private void getFiles()
        {
            string data_folder = getDataFolder();
            string dda_file_final = data_folder + @"\Demanda.csv";
            string cmg_file_final = data_folder + @"\CostoMarginal.csv";
            string dda_file_cen = data_folder + @"\Demanda_CEN.csv";
            string cmg_file_cen = data_folder + @"\CostoMarginal_CEN.csv";
            bool is_locked_demanda_cen;
            bool is_locked_cmg_cen;
            bool is_locked_demanda_final;
            bool is_locked_cmg_final;
            if (firstTime)
            {
                createDataFolder();
                firstTime = false;
                is_locked_demanda_cen = false;
                is_locked_cmg_cen = false;
                is_locked_demanda_final = false;
                is_locked_cmg_final = false;
            }
            else
            {
                FileInfo fiDemandaCen = new FileInfo(dda_file_cen);
                FileInfo fiCmgCen = new FileInfo(cmg_file_cen);
                is_locked_demanda_cen = IsFileLocked(fiDemandaCen);
                is_locked_cmg_cen = IsFileLocked(fiCmgCen);
                FileInfo fiDemanda = new FileInfo(dda_file_final);
                FileInfo fiCmg = new FileInfo(cmg_file_final);
                is_locked_demanda_final = IsFileLocked(fiDemanda);
                is_locked_cmg_final = IsFileLocked(fiCmg);
            }
            

            

            if (!is_locked_demanda_cen && !is_locked_demanda_final)
            {
                getDemandaCen();
                string[] fileDemanda = formatDemanda();
                File.WriteAllLines(dda_file_final, fileDemanda);
                label3.Text = "Archivo: " + dda_file_final.ToString() +" actualizado a las " + DateTime.Now.ToString() + "\n";
            }

            if (!is_locked_cmg_cen && !is_locked_cmg_final)
            {
                getCmgCen();
                string[] fileCmg = formatCmg();
                File.WriteAllLines(cmg_file_final, fileCmg);
                label3.Text += "Archivo: " + cmg_file_final+ " actualizado a las " + DateTime.Now.ToString() + "\n";
            }
            return;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (!detenido)
            {
                detenido = true;
                label3.Text = "Pausado";
                label5.Text = "Pausado";
                timer.Stop();
            }
            
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
           
                //if the form is minimized  
                //hide it from the task bar  
                //and show the system tray icon (represented by the NotifyIcon control)  
                if (this.WindowState == FormWindowState.Minimized)
                {
                    Hide();
                notifyIcon1.BalloonTipText = "Estado : " + (detenido ? "Pausado" : "Conectado");
                    notifyIcon1.Visible = true;
                    notifyIcon1.ShowBalloonTip(1000);
            }
        }
    }
}
