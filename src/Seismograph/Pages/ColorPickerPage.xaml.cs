using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Seismograph
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ColorPickerPage : Page
    {
        LinearGradientBrush rainbow;
        bool pressed;

        public ColorPickerPage()
        {
            this.InitializeComponent();
            this.IsHitTestVisible = true;
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(NamedColor), typeof(ColorPicker), new PropertyMetadata(null, new PropertyChangedCallback(OnSelectedColorChanged)));

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPickerPage)d).OnSelectedColorChanged();
        }

        private void OnSelectedColorChanged()
        {
            if (SelectedColor != null)
            {
                SelectionSwatch.Fill = new SolidColorBrush(SelectedColor.Color);
                // move the border to the matching Y location...
            }
            if (SelectionChanged != null)
            {
                SelectionChanged(this, EventArgs.Empty);
            }
        }


        public NamedColor SelectedColor
        {
            get { return (NamedColor)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public event EventHandler SelectionChanged;

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);

            Point pos = e.GetCurrentPoint(BackgroundSwatch).Position;

            rainbow = (LinearGradientBrush)this.BackgroundSwatch.Background;

            Color c = FindColorAt(pos.Y);

            LinearGradientBrush colorBrightnessSpectrum = new LinearGradientBrush() { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };

            colorBrightnessSpectrum.GradientStops.Add(new GradientStop() { Color = Windows.UI.Colors.Black, Offset = 0 });
            colorBrightnessSpectrum.GradientStops.Add(new GradientStop() { Color = c, Offset = .5 });
            colorBrightnessSpectrum.GradientStops.Add(new GradientStop() { Color = Windows.UI.Colors.White, Offset = 1 });

            this.BackgroundSwatch.Background = colorBrightnessSpectrum;
            this.pressed = true;
        }

        private Color FindColorAt(double y)
        {
            double height = this.RenderSize.Height;
            double offset = (y / height); // get number between 0 and 1.

            // and find that offset in the gradient stops.
            LinearGradientBrush brush = (LinearGradientBrush)this.BackgroundSwatch.Background;

            GradientStop previous = null;
            GradientStop next = null;

            int count = brush.GradientStops.Count;
            foreach (GradientStop stop in brush.GradientStops)
            {
                next = stop;
                if (stop.Offset > offset)
                {
                    break;
                }
                previous = stop;
            }
            if (next == null)
            {
                // no stops?
                return Windows.UI.Colors.Transparent;
            }
            if (previous == null)
            {
                // no offset 0?
                return Windows.UI.Colors.Transparent;
            }

            // ok, now the color is somewhere between these two stops.
            Color from = previous.Color;
            Color to = next.Color;

            double range = next.Offset - previous.Offset;
            double inset = (offset - previous.Offset); // into the range
            double percent = inset / range; // amount of to color, (1-percent) is amount of from-color

            return Interpolate(from, to, percent);
        }

        private Color Interpolate(Color from, Color to, double percent)
        {
            double toPercent = percent;
            double fromPercent = 1 - percent;

            double alpha = (from.A * fromPercent) + (to.A * toPercent);
            double red = (from.R * fromPercent) + (to.R * toPercent);
            double green = (from.G * fromPercent) + (to.G * toPercent);
            double blue = (from.B * fromPercent) + (to.B * toPercent);
            return Color.FromArgb((byte)alpha, (byte)red, (byte)green, (byte)blue);
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);
            Point pos = e.GetCurrentPoint(BackgroundSwatch).Position;
            if (this.pressed)
            {
                SelectionBorder.Visibility = Visibility.Visible;
                double borderHeight = SelectionBorder.ActualHeight;
                Color c = FindColorAt(pos.Y);
                SelectedColor = new NamedColor(c.ToString(), c);
                SelectionBorder.Margin = new Thickness(0, pos.Y - borderHeight / 2, 0, 0);
            }
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            this.pressed = false;
            base.OnPointerReleased(e);
            this.CloseCurrentFlyout();
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            this.CloseCurrentFlyout();
        }
    }
}
