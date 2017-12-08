﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Server.View
{
    public partial class ucSetting : UserControl
    {
        private bool checkHasFileIni = false;

        public ucSetting()
        {
            if (!this.DesignMode)
            {
                InitializeComponent();
            }
        }


        #region -- Event --

        private void ucSetting_Load(object sender, EventArgs e)
        {
            txtIP.Text = Controller.Ip.getIpLocal();
            resizeFill();

            cbNumCourse.DataSource = Controller.CourseController.getList();
            cbNumCourse.DisplayMember = "id";

            load();
        }

        private void lblCSDL_Click(object sender, EventArgs e)
        {
            if (!checkHasFileIni)
            {
                if (checkBoxCSDL.Checked) checkBoxCSDL.Checked = false;
                else checkBoxCSDL.Checked = true;
                checkBoxCSDL_OnChange(null, null);
            }
        }

        private void lblFile_Click(object sender, EventArgs e)
        {
            if (!checkHasFileIni)
            {
                if (checkBoxFile.Checked) checkBoxFile.Checked = false;
                else checkBoxFile.Checked = true;
                checkBoxFile_OnChange(null, null);
            }
        }

        //textbox number only
        private void txtNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!checkHasFileIni)
            {
                if (!Char.IsDigit(e.KeyChar) && !Char.IsControl(e.KeyChar)) { e.Handled = true; }
            }
            else e.Handled = true;
        }

        private void txtIP_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void lbl_MouseEnter(object sender, EventArgs e)
        {
            (sender as Label).ForeColor = Color.FromArgb(25, 33, 94);
        }

        private void lblCSDL_MouseLeave(object sender, EventArgs e)
        {
            lblCSDL.ForeColor = Color.FromArgb(146, 18, 36);
        }

        private void lblFile_MouseLeave(object sender, EventArgs e)
        {
            lblFile.ForeColor = Color.FromArgb(104, 7, 71);
        }

        private void cbNumCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            var count = Controller.QuestionController.countInCourse((cbNumCourse.SelectedItem as Model.Course).Id);
            lblQuesDB.Text = "Có " + count + " câu hỏi";
            lblQuesDB.Tag = count;

            Controller.ListObj.addWithDB((cbNumCourse.SelectedItem as Model.Course).Id);
        }

        private void checkBoxCSDL_OnChange(object sender, EventArgs e)
        {
            if (!checkHasFileIni)
            {
                if (checkBoxCSDL.Checked)
                {
                    TitleTransition.ShowSync(panelHeaderQuesDB);
                    ShowTransition.ShowSync(panelContentQuesDB);
                    panelHeaderQuesDB.Visible = true;
                    panelContentQuesDB.Visible = true;
                }
                else
                {
                    TitleTransition.Hide(panelHeaderQuesDB);
                    ShowTransition.Hide(panelContentQuesDB);
                }
            }
        }

        private void checkBoxFile_OnChange(object sender, EventArgs e)
        {
            if (!checkHasFileIni)
            {
                if (checkBoxFile.Checked)
                {
                    TitleTransition.ShowSync(panelHeaderQuesFile);
                    ShowTransition.ShowSync(panelContentQuesFile);
                    panelHeaderQuesFile.Visible = true;
                    panelContentQuesFile.Visible = true;
                }
                else
                {
                    TitleTransition.Hide(panelHeaderQuesFile);
                    ShowTransition.Hide(panelContentQuesFile);
                }
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            if (!checkHasFileIni)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();

                openFileDialog.InitialDirectory = Path.GetPathRoot(Environment.SystemDirectory);
                openFileDialog.Filter = "xls files (*.xls)|*.xls|xlsx files (*.xlsx)|*.xlsx";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtPathFile.Text = openFileDialog.FileName;
                }
            }
        }

        private void txtPathFile_OnValueChanged(object sender, EventArgs e)
        {
            Controller.ReadFileExcel.read(txtPathFile.Text);
            int countQuesInFile = Controller.ListObj.LsQuesFile.Count;
            lblQuesFile.Text = "Có " + countQuesInFile + " câu hỏi";
            lblQuesFile.Tag = countQuesInFile;
        }

        private void btnCreateOrOK_Click(object sender, EventArgs e)
        {
            if (validate())
            {
                string path = create_folder_contain_setting();
                create_file_setting(path);
                checkHasFileIni = true;
                MessageBox.Show("Khởi tạo cấu hình thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Question);
                this.btnDestroy.Visible = true;
                this.btnCreateOrOK.Visible = false;
            }
        }

        private void btnDestroy_Click(object sender, EventArgs e)
        {
            create_folder_contain_setting();
            this.btnDestroy.Visible = false;
            this.btnCreateOrOK.Visible = true;
            checkHasFileIni = false;
            cbNumCourse.Enabled = true;
        }

        #endregion


        #region -- Method --

        private void load()
        {
            string path = Path.GetDirectoryName(Application.ExecutablePath);
            path = Path.Combine(path, Controller.Constant.nameFolderSaveFile);
            path = Path.Combine(path, Controller.Constant.nameFileSetting);
            if (File.Exists(path))
            {
                checkHasFileIni = true;
                cbNumCourse.Enabled = false;
                btnCreateOrOK.Visible = false;
                Controller.IO_INI ini = new Controller.IO_INI(path);
                txtIP.Text = ini.IniReadValue(Controller.Constant.sectionInfo, Controller.Constant.keyIP);
                txtPort.Text = ini.IniReadValue(Controller.Constant.sectionInfo, Controller.Constant.keyPort);
                txtTime.Text = ini.IniReadValue(Controller.Constant.sectionSetting, Controller.Constant.keyTime);
                string course = ini.IniReadValue(Controller.Constant.sectionDB, Controller.Constant.keyIdCourse);
                if (course != string.Empty)
                {
                    checkBoxCSDL.Checked = true;
                    panelHeaderQuesDB.Visible = true;
                    panelContentQuesDB.Visible = true;

                    string[] SplitCourse = course.Split('\\');
                    string idcourse = SplitCourse[0];
                    cbNumCourse.SelectedText = idcourse;
                    txtNumQuesDB.Text = ini.IniReadValue(Controller.Constant.sectionDB, Controller.Constant.keyGetQuesDB);
                }

                string file = ini.IniReadValue(Controller.Constant.sectionFile, Controller.Constant.keyPathFile);
                if (file != string.Empty)
                {
                    checkBoxFile.Checked = true;
                    panelHeaderQuesFile.Visible = true;
                    panelContentQuesFile.Visible = true;

                    txtPathFile.Text = ini.IniReadValue(Controller.Constant.sectionFile, Controller.Constant.keyPathFile);
                    txtNumQuesFile.Text = ini.IniReadValue(Controller.Constant.sectionFile, Controller.Constant.keyGetQuesFile);
                }

                btnDestroy.Visible = true;
            }
        }

        private bool checkTextBoxEmpty(Bunifu.Framework.UI.BunifuMaterialTextbox txt)
        {
            return txt.Text.Length > 0;
        }

        private bool validate()
        {
            if (!checkBoxCSDL.Checked && !checkBoxFile.Checked)
            {
                MessageBox.Show("Bạn phải cấu hình lấy câu hỏi", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            List<Bunifu.Framework.UI.BunifuMaterialTextbox> ls = new List<Bunifu.Framework.UI.BunifuMaterialTextbox>();
            ls.Add(txtIP);
            ls.Add(txtPort);
            ls.Add(txtTime);
            foreach (var i in ls)
                if (!checkTextBoxEmpty(i))
                {
                    MessageBox.Show("Bạn phải điền đủ thông tin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

            if (checkBoxCSDL.Checked)
            {
                if (!checkTextBoxEmpty(txtNumQuesDB))
                {
                    MessageBox.Show("Bạn phải điền đủ thông tin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                int numQuesDB = Convert.ToInt32(txtNumQuesDB.Text);
                if (numQuesDB > Convert.ToInt32(lblQuesDB.Tag))
                {
                    MessageBox.Show("Câu hỏi lấy ra từ CSDL phải bé hơn " + Convert.ToInt32(lblQuesDB.Tag).ToString(), "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            if (checkBoxFile.Checked)
            {
                if (!checkTextBoxEmpty(txtNumQuesFile))
                {
                    MessageBox.Show("Bạn phải điền đủ thông tin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                int numQuesFile = Convert.ToInt32(txtNumQuesFile.Text);
                if (Convert.ToInt32(lblQuesFile.Tag) == 0)
                {
                    MessageBox.Show("Không có câu hỏi!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                if (numQuesFile > Convert.ToInt32(lblQuesFile.Tag))
                {
                    MessageBox.Show("Câu hỏi lấy ra từ File phải bé hơn " + Convert.ToInt32(lblQuesFile.Tag).ToString(), "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }

            return true;
        }

        private void Empty(DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        private string create_folder_contain_setting()
        {
            string path = Path.GetDirectoryName(Application.ExecutablePath);
            path = Path.Combine(path, Controller.Constant.nameFolderSaveFile);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(Controller.Constant.nameFolderSaveFile);
            }

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            Empty(dirInfo);


            path = Path.Combine(path, Controller.Constant.nameFileSetting);

            return path;
        }

        private void create_file_setting(string path)
        {
            Controller.IO_INI ini = new Controller.IO_INI(path);
            ini.IniWriteValue(Controller.Constant.sectionInfo, Controller.Constant.keyIP, txtIP.Text);
            ini.IniWriteValue(Controller.Constant.sectionInfo, Controller.Constant.keyPort, txtPort.Text);
            ini.IniWriteValue(Controller.Constant.sectionSetting, Controller.Constant.keyTime, txtTime.Text);
            if (checkBoxCSDL.Checked)
            {
                ini.IniWriteValue(Controller.Constant.sectionDB, Controller.Constant.keyIdCourse, (cbNumCourse.SelectedItem as Model.Course).Id + "//" + (cbNumCourse.SelectedItem as Model.Course).Name);
                ini.IniWriteValue(Controller.Constant.sectionDB, Controller.Constant.keyGetQuesDB, txtNumQuesDB.Text);
            }

            if (checkBoxFile.Checked)
            {
                ini.IniWriteValue(Controller.Constant.sectionFile, Controller.Constant.keyPathFile, txtPathFile.Text);
                ini.IniWriteValue(Controller.Constant.sectionFile, Controller.Constant.keyGetQuesFile, txtNumQuesFile.Text);
            }
        }

        public void resizeFill()
        {
            this.Width = this.Parent.Width;
            this.Height = this.Parent.Height;
        }

        #endregion
    }
}
