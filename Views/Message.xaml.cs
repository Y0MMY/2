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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace launher
{
    
    public partial class Message 
    {
        public enum Result
        {
            Cancel,
            Install,
            Ok
        }


        public enum MessageButtons
        {
            Ok,
            CancelInstall
        }

        public Result MessageResult;

        public Message(string text, MessageButtons buttons)
        {
            InitializeComponent();
            //this.ShowInTaskbar = false;


            if (buttons == MessageButtons.Ok)
            {
                Button button = new Button
                {
                    Content = "Ok",
                    Tag = Result.Ok,
                    Width = 100,
                    Padding = new Thickness(1),
                    FontSize = 20,

                    Template = Application.Current.Resources["button"] as ControlTemplate,
                    Background = new SolidColorBrush(Color.FromRgb(47, 228, 255))
                };

                button.Click += Click;
                wp_buttons.Children.Add(button);
            }
            else if(buttons == MessageButtons.CancelInstall)
            {
                Button button1 = new Button
                {
                    Content = "Установить",
                    Tag = Result.Install,
                    Width = 150,
                    Padding = new Thickness(1),
                    FontSize = 20,
                    Margin = new Thickness(0, 0, 10, 0),
                    Template = Application.Current.Resources["button"] as ControlTemplate,
                    Background = new SolidColorBrush(Color.FromRgb(47, 228, 255))
                };

                Button button2 = new Button
                {
                    Content = "Отмена",
                    Tag = Result.Cancel,
                    Width = 100,
                    Padding = new Thickness(1),
                    FontSize = 20,

                    Template = Application.Current.Resources["button"] as ControlTemplate,
                    Background = new SolidColorBrush(Color.FromRgb(47, 228, 255))
                };

                button1.Click += Click;
                button2.Click += Click;

                wp_buttons.Children.Add(button1);
                wp_buttons.Children.Add(button2);
            }

            lbl_text.Content = text;

        }

        private void Click(object sender, EventArgs args)
        {
            MessageResult = (Result)(sender as Button).Tag;


            Close();
        }
    }
}
