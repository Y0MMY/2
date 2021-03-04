using System;
using System.Collections.Generic;
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
   


    public partial class ModeWindow : Window
    {

        public enum ModeAnswer
        {
            Cancel,
            Install
        }

        public ModeAnswer Result;

        public ModeWindow(string description)
        {
            InitializeComponent();
            tb_description.Text = description;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Result = ModeAnswer.Cancel;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Result = ModeAnswer.Install;
            Close();
        }
    }
}
