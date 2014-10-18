using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCodeResultChecker
{
    public partial class frmSolution : Form
    {
        string slnPath; // путь решения
        string slnName; // имя решения
        string taskPath; // путь задачи
        const string cpp_name = "main.cpp";

        public frmSolution(string taskPath, string slnPath, string slnName)
        {
            InitializeComponent();
            this.slnPath = slnPath;
            this.slnName = slnName;
            this.taskPath = taskPath;
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
			// имя результирующего приложения
            string exe_name = "app.exe";
			// путь к main.cpp
            string main_cpp_path = slnPath + @"\" + cpp_name;

            if (!File.Exists(main_cpp_path)) {
                MessageBox.Show("Отсутствуют файл main.cpp");
                return;
            }

            IEnumerable<FileInfo> files = getSourceCodeFiles();
            string run_args = "";
			// составляем список файлов для сборки
            foreach(var file in files){
                run_args = run_args + " " + file.Name;
            }
            run_args = run_args + " -o " + exe_name; // параметр вывода ошибок
			
			// запускаем сборку
            var gp = new Process()
            {
                StartInfo = new ProcessStartInfo(@"g++.exe")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = slnPath,
                    Arguments = run_args
                }
            };
            if (!gp.Start())
            {
                MessageBox.Show("Ошибка запуска проверки.");
                return;
            }
			// читаем ошибки
            var stderr = gp.StandardError;
            string line;
            List<string> gp_out = new List<string>();
            while ((line = stderr.ReadLine()) != null)
            {
                gp_out.Add(line);
                Console.WriteLine(line);
            }
			// выход
            gp.WaitForExit();
			
			//если ошибок нет
            if (gp_out.Count == 0)
            {
                // запускаем результирующее приложение app.exe
				Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = slnPath + @"\" + exe_name;
                p.Start();
                p.WaitForExit();
				// читаем выход
                string output = p.StandardOutput.ReadToEnd();
				// выводим результат
                MessageBox.Show(output, "Код выполнился со следующим результатом:");
                p.WaitForExit();
            }
            else
            {
                MessageBox.Show(string.Join(Environment.NewLine, gp_out), "Ошибка исполнения");
            }
        }
		
		// фильтр по расширению файлов
        IEnumerable<FileInfo> getSourceCodeFiles() {
            string[] extensions = new string[2] { ".cpp", ".h" };
            DirectoryInfo df = new DirectoryInfo(slnPath);
            IEnumerable<FileInfo> files = df.EnumerateFiles();
                
            return files.Where(f => extensions.Contains(f.Extension));
        }
		
		// обновляем дерево
        private void recreateFileTree() {
            tvFiles.Nodes.Clear();
            if (Directory.Exists(slnPath))
            {
                IEnumerable<FileInfo> files = getSourceCodeFiles();
                foreach (var file in files)
                {
                    tvFiles.Nodes.Add(file.Name);
                }
            }
        }

        private void frmSolution_Load(object sender, EventArgs e)
        {
            this.Text = "Решение: "+slnName;
            recreateFileTree();
        }
		
		// выбор файла
        private void tvFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string path = slnPath + @"\" + tvFiles.SelectedNode.FullPath;
            if (File.Exists(path))
            {
                scintilla.Text = File.ReadAllText(path);
            }
        }
		
		// сохранение
        private void btnSave_Click(object sender, EventArgs e)
        {
            string path = slnPath + @"\" + tvFiles.SelectedNode.FullPath;
            if (File.Exists(path))
            {
                File.WriteAllText(path, scintilla.Text);
            }
        }
    }
}
