using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace launher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class InitWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer progressBar = new DispatcherTimer();
        DispatcherTimer iconTimer = new DispatcherTimer();

        int adittion = 10;
        private string operation;

      
       
        private int counter = 1;
        private int counterIcon = 0;

        public InitWindow()
        {
            InitializeComponent();

            progressBar.Tick += UpdateProgressBar;
            progressBar.Interval = TimeSpan.FromMilliseconds(1);
            progressBar.Start();

            iconTimer.Tick += ChangeIcon;
            iconTimer.Interval = TimeSpan.FromSeconds(2.5);
            iconTimer.Start();       
        }

        public void SetOperation(string operation)
        {
            this.operation = operation;
            NextLabel();
        }

        private void UpdateProgressBar(object sender, EventArgs args)
        {
            rotate.Angle += 5;
        }

        private void ChangeIcon(object sender, EventArgs args)
        {
            DoubleAnimation anim = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5)
            };

            anim.Completed += CompleteChangeIcon;

            UIElement element = gridIcon.Children[counterIcon++];

            element.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        private void CompleteChangeIcon(object sender, EventArgs args)
        {
            DoubleAnimation anim = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };

            if (counterIcon == gridIcon.Children.Count)
                counterIcon = 0;

            UIElement element = gridIcon.Children[counterIcon];
            element.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        private void NextLabel()
        {
            DoubleAnimation anim = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400)
            };

            anim.Completed += CompleteNextLabel;


            label.BeginAnimation(Label.OpacityProperty, anim);

                 
        }

        private void CompleteNextLabel(object sender, EventArgs args)
        {
            label.Content = operation;

            DoubleAnimation anim = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(400)
            };

            label.BeginAnimation(Label.OpacityProperty, anim);
        }
    }
}
