using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using MetroFramework.Controls;

namespace WinSim
{
    public partial class CreateIcons : MetroForm
    {
        private string ShortCutPath;
        private Program.Config config;
        public CreateIcons(Program.Config config)
        {
            this.config = config;
            InitializeComponent();
            metroLabel1.Text = "Select number of screens";
            metroLabel2.Text = "Machine 1";
            metroLabel3.Text = "Machine 2";
            metroLabel4.Text = "Machine 3";
            metroLabel5.Text = "Machine 4";
            metroButton1.Text = "Confirm";
            metroButton1.Highlight = true;
            metroLabel6.Text = "Click to browse for application to create shortcuts";
            metroButton2.Text = "Browse";
            metroButton2.Highlight = true;
            metroButton3.Text = "Create Shortcuts";
            metroButton3.Highlight = true;
            metroButton4.Text = "Close";
            metroButton4.Highlight = true;
            metroTextBox1.Text = "";
            InitComboBox();
        }

        private void InitComboBox()
        {
            metroComboBox1.SelectedIndex = config.no_of_screens - 2;
            switch (config.no_of_screens)
            {
                case 2:
                    metroComboBox2.SelectedIndex = config.colors[0];
                    metroComboBox3.SelectedIndex = config.colors[1];
                    metroComboBox4.SelectedIndex = -1;
                    metroComboBox5.SelectedIndex = -1;
                    metroComboBox4.Enabled = false;
                    metroComboBox5.Enabled = false;
                    break;
                case 3:
                    metroComboBox2.SelectedIndex = config.colors[0];
                    metroComboBox3.SelectedIndex = config.colors[1];
                    metroComboBox4.SelectedIndex = config.colors[2];
                    metroComboBox5.SelectedIndex = -1;
                    metroComboBox5.Enabled = false;
                    break;
                case 4:
                    metroComboBox2.SelectedIndex = config.colors[0];
                    metroComboBox3.SelectedIndex = config.colors[1];
                    metroComboBox4.SelectedIndex = config.colors[2];
                    metroComboBox5.SelectedIndex = config.colors[3];
                    break;
            }
        }

        private void CreateShortcut(string shortcutPathName, bool create, string shortcutTarget, int machine)
        {
            if (create)
            {
                try
                {
                    WshShell myShell = new WshShell();
                    WshShortcut myShortcut = (WshShortcut)myShell.CreateShortcut(shortcutPathName);
                    myShortcut.TargetPath = Application.StartupPath+"\\WinSim.exe"; //The exe file this shortcut executes when double clicked 
                    myShortcut.IconLocation = shortcutTarget+",0"; //Sets the icon of the shortcut to the exe`s icon 
                    myShortcut.WorkingDirectory = Application.StartupPath; //The working directory for the exe 
                    myShortcut.Arguments = "m"+machine+" \""+shortcutTarget+"\""; //The arguments used when executing the exe 
                    myShortcut.Save(); //Creates the shortcut 
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    if (System.IO.File.Exists(shortcutPathName))
                        System.IO.File.Delete(shortcutPathName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        
        private void saveConfig()
        {
            Properties.Settings.Default["screens"] = config.no_of_screens;
            string colors = "";
            for (int i = 0; i < config.colors.Length;i++) {
                colors += config.colors[i] + " ";
            }
            Properties.Settings.Default["colors"] = colors.Trim();
            Properties.Settings.Default.Save();
            MessageBox.Show("Succesfully updated the settings!!", "Success");
        }

        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (metroComboBox1.SelectedIndex == 1)
            {
                metroComboBox4.Enabled = true;
                metroComboBox5.SelectedIndex = -1;
                metroComboBox5.Enabled = false;
            }
            else if (metroComboBox1.SelectedIndex == 2)
            {
                metroComboBox4.Enabled = true;
                metroComboBox5.Enabled = true;
            }
            else
            {
                metroComboBox4.SelectedIndex = -1;
                metroComboBox4.Enabled = false;
                metroComboBox5.SelectedIndex = -1;
                metroComboBox5.Enabled = false;
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            MetroComboBox [] comboBoxes = { metroComboBox2, metroComboBox3, metroComboBox4, metroComboBox5 };
            config.no_of_screens = metroComboBox1.SelectedIndex + 2;
            int[] colors = new int[config.no_of_screens];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = comboBoxes[i].SelectedIndex;
            }
            bool distinct = colors.Distinct().Count() == colors.Length;
            if (distinct)
            {
                config.colors = colors;
                saveConfig();
            }
            else
            {
                MessageBox.Show("Please make sure you select different colors for different machines", "Error");
            }
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                ShortCutPath = openFileDialog1.FileName;
                metroTextBox1.Text = ShortCutPath;
                Icon icon = Icon.ExtractAssociatedIcon(ShortCutPath);
            }
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            if (ShortCutPath == null || ShortCutPath.Length == 0)
            {
                metroTextBox1.Text = "Please select an application to create shortcuts for";
                return;
            }
            string shortcut = ShortCutPath.Split('\\').Last();
            shortcut = shortcut.Substring(0, shortcut.Length - 4);
            for(int i = 0; i < config.no_of_screens; i++)
            {
                string DesktopPathName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), shortcut + "(m"+(i+1)+").lnk");
                CreateShortcut(DesktopPathName, true, ShortCutPath, i+1);
            }
            MessageBox.Show("Shortcuts created succesfully","Success!!");
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        
    }
}
