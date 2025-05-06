using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageMagick;
using AdsJumboWinForm;



namespace ConvertImage
{
   
    public partial class frm_main : Form
    {

        public frm_main()
        {
            InitializeComponent();
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frm_info frm = new frm_info();
            frm.Show();
        }

        [STAThread]
        private void btn_fileOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "Webp Images (*..webp)|*.webp",
                Title = "Select Wevb Images"
            };
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                string[] selectFiles = ofd.FileNames;
                List<Image> imgList = new List<Image>();

                foreach (string filePath in selectFiles)
                {
                    try
                    {
                        //MessageBox.Show(filePath);
                        list_Add(Path.GetFileName(filePath),filePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load image : {filePath}. Error : {ex.Message}");
                    }
                }
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            //드래그된 데이터가 파일일 경우에만 허용
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (fileTypeCheck(file))
                    {
                        string fileName = Path.GetFileName(file);
                        string filePath = file;
                        list_Add(fileName, filePath);
                    }
                }
            }
        }
        private bool fileTypeCheck(string path)
        {
            if (Path.GetExtension(path).ToLower() == ".webp")
            {
                return true;
            }
            else { return false; }
        }
        private void list_Add(string fileName, string filePath)
        {
            ListViewItem item = new ListViewItem(fileName);
            item.SubItems.Add(filePath);
            item.SubItems.Add("");
            listView1.Items.Add(item);
        }

        private void btn_itemClear_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
        }

        private void frm_main_Load(object sender, EventArgs e)
        {

            comboType.Text = "PNG";

            //저장 파일 경로 읽기
            if (File.Exists("path.txt"))
            {
                string savedPath = File.ReadAllText("path.txt");
                txt_save.Text = savedPath;
            }
        }

        private void btn_filePath_Click(object sender, EventArgs e)
        {
            //저장 파일 경로 저장
            using (FolderBrowserDialog fd = new FolderBrowserDialog())
            {
                if(fd.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = fd.SelectedPath;
                    File.WriteAllText("path.txt", selectedPath);
                    txt_save.Text = selectedPath;
                }
            }
        }

        private void btn_convert_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txt_save.Text))
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.SubItems[2].Text))
                    {
                        ConvertImage(item.SubItems[1].Text);
                    }
                }
            }
            else
            {
                MessageBox.Show($"The save path does not exist : {txt_save.Text}","TWIC",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }
        private void ConvertImage(string path)
        {
            //변환 타입 지정
            string splitName = Path.GetFileNameWithoutExtension(path);
            string fileName = $"{txt_save.Text}\\{splitName}.{comboType.Text.ToLower()}";

            try
            {
                using (var Image = new MagickImage(path))
                {
                    Image.Write(fileName);
                    listUpdate(path);
                }
            }
            catch (MagickException ex) //변환 실패시 에러 표시
            {
                MessageBox.Show($"Conversion failed : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void listUpdate(string path)
        {
            foreach(ListViewItem item in listView1.Items)
            {
                if (item.SubItems[1].Text.Equals(path,  StringComparison.OrdinalIgnoreCase))
                {
                    //item.SubItems[2].ForeColor = Color.DarkGreen;
                    item.SubItems[2].Text = "Success !";
                }
            }
        }

        private void frm_main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
