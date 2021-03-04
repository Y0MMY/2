using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace launher.Templates
{
    public partial class ButtonAnimate: ResourceDictionary
    {
       

        private void Button_MouseEnter(object sender, EventArgs args)
        {
            Rectangle rec = (Rectangle)(sender as Grid).Children[0];
            Grid grid = (sender as Grid);
            DoubleAnimation scaleTransform = new DoubleAnimation()
            {
                To = 1.02,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            ScaleTransform scale = new ScaleTransform();
            grid.RenderTransform = scale;

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleTransform);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleTransform);


            DoubleAnimation animationShadow = new DoubleAnimation()
            {
                From = 0,
                To = 0.5,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            DropShadowEffect shadow = new DropShadowEffect()
            {
                Color = Colors.Black,
                
            };

            rec.Effect = shadow;
            shadow.BeginAnimation(DropShadowEffect.OpacityProperty, animationShadow);

        }

        private void AddShadow(object sender, EventArgs args)
        {
          


        }

        private void Button_MouseLeave(object sender, EventArgs args)
        {
            Rectangle rec = (Rectangle)(sender as Grid).Children[0];
            Grid grid = (sender as Grid);
            DoubleAnimation scaleTransform = new DoubleAnimation()
            {
                From = 1.02,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            ScaleTransform scale = new ScaleTransform();
            grid.RenderTransform = scale;

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleTransform);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleTransform);

            DoubleAnimation animationShadow = new DoubleAnimation()
            {
                From = 0.5,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            DropShadowEffect shadow = new DropShadowEffect()
            {
                Color = Colors.Black
            };

            rec.Effect = shadow;
            shadow.BeginAnimation(DropShadowEffect.OpacityProperty, animationShadow);
        }
    }
}
