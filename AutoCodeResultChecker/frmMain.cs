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
    public partial class frmMain : Form
    {
        // путь к папке
		string fullPath;
        
        public frmMain()
        {
            fullPath = "";
            InitializeComponent();
            btnSolutions.Enabled = false;
        }

        private void оПриложенииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout about = new frmAbout();
            about.Show();
        }
		
		// загрузка формы
        private void frmMain_Load(object sender, EventArgs e)
        {
            // получаем папку с классами задачами
			string path = AppDomain.CurrentDomain.BaseDirectory+"Tasks";
            if (Directory.Exists(path))
            {
                string[] tasks_l1 = Directory.GetDirectories(path);
				// и записываем их всех в дерево
                foreach(var f_task in tasks_l1){
                    string[] f_tasks_l2 = Directory.GetDirectories(f_task);
                    TreeNode node = new TreeNode(f_task.Remove(0,path.Length+1));
                    foreach (var task in f_tasks_l2)
                    {
                        node.Nodes.Add(task.Remove(0, f_task.Length + 1));
                    }
                    treeTasks.Nodes.Add(node);
                }
            }
        }
		
		// выбор элемента дерева
        private void treeTasks_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // получаем выбранную задачу
			fullPath = AppDomain.CurrentDomain.BaseDirectory + @"Tasks\" + treeTasks.SelectedNode.FullPath;
            string path = fullPath + @"\description.rtf";
			// если файл описания существуют, активируем кнопку перехода к решению
            if (File.Exists(path))
            {
                rtbDescription.LoadFile(path);
                btnSolutions.Enabled = true;
            }
            else btnSolutions.Enabled = false;
        }
		
		// переход к решению
        private void btnSolutions_Click(object sender, EventArgs e)
        {
            // получаем папку решений
			string path = fullPath + @"\Solutions";
            if (Directory.Exists(path))
            {
                frmAdd add = new frmAdd();
                string[] solutions = Directory.GetDirectories(path);
				// загржуаем список существующих решений в форму выбора
                foreach (var sln in solutions)
                {
                    add.cbSolutions.Items.Add(sln.Remove(0, path.Length + 1));
                }
                add.ShowDialog();
				
				// получаем выбранное название
                string solution = add.cbSolutions.Text;
                string sln_path = path+@"\"+solution;
				// если не существует такое решение, то создаем папку с таким названием и копируем туда шаблон
                if (!Directory.Exists(sln_path))
                {
                    Directory.CreateDirectory(sln_path);
                    string tpl_path = fullPath + @"\tpl";
                    foreach (var file in Directory.GetFiles(tpl_path))
                        File.Copy(file, Path.Combine(sln_path, Path.GetFileName(file)));
                }
                frmSolution frmSln = new frmSolution(fullPath,sln_path, solution);
                frmSln.ShowDialog();
            }
            
        }

    }
}
