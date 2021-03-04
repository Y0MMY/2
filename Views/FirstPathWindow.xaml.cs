using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace launher
{
    /// <summary>
    /// Interaction logic for FirstPathWindow.xaml
    /// </summary>
    public partial class FirstPathWindow : Window
    {
        public FirstPathWindow()
        {
            InitializeComponent();
            this.Activate();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new CommonOpenFileDialog();

                dialog.IsFolderPicker = true;

                dialog.ShowDialog();

                this.Activate();
                tbx_currentFolder.Content = dialog.FileName;
                OptionsWindow.UserFolder = dialog.FileName;

                File.WriteAllText("./path", OptionsWindow.UserFolder);
            }
            catch
            {
                return;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (OptionsWindow.UserFolder == null)
            {
                Message message = new Message("Выберите путь до игры!", Message.MessageButtons.Ok);
                message.ShowDialog();
            }
            else
            {
                DialogResult = true;
            }
        }
    }
}
