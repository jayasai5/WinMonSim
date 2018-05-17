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

namespace WinSim
{
    public partial class CreateIcons : Form
    {
        private string ShortCutPath;
        public CreateIcons()
        {
            InitializeComponent();
            label1.Text = "Click to browse for application to create shortcuts";
            label2.Text = "";
            button1.Text = "Browse";
            button2.Text = "Confirm";
            button3.Text = "Close";
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) {
                ShortCutPath = openFileDialog1.FileName;
                label2.Text = ShortCutPath;
                Icon icon = Icon.ExtractAssociatedIcon(ShortCutPath);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (ShortCutPath == null||ShortCutPath.Length==0) {
                label2.Text = "Please select an application to create shortcuts for";
                return;
            }
            string shortcut = ShortCutPath.Split('\\').Last();
            shortcut = shortcut.Substring(0, shortcut.Length - 4);
            string DesktopPathName1 = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), shortcut + "(m1).lnk");
            string DesktopPathName2 = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), shortcut + "(m2).lnk");
            CreateShortcut(DesktopPathName1, true, ShortCutPath,1);
            CreateShortcut(DesktopPathName2, true, ShortCutPath,2);
            label2.Text = "Shortcuts created succesfully";
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

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
