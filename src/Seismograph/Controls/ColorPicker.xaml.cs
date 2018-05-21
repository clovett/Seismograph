using Seismograph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Seismograph
{
    public sealed partial class ColorPicker : UserControl
    {
        bool pressed;

        public ColorPicker()
        {
            InitializeComponent();
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            pressed = true;
            UpdateState();
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            pressed = false;
            UpdateState();
            ShowPickerPage();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            pressed = false;
            base.OnLostFocus(e);
            UpdateState();
        }

        void UpdateState()
        {
            // Need Transparent instead of null in order to be hittable.
            Border.Background = pressed ? (Brush)this.Resources["ButtonPressedBackgroundThemeBrush"] : new SolidColorBrush(Windows.UI.Colors.Transparent);
        }

        public NamedColor SelectedColor
        {
            get { return (NamedColor)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(NamedColor), typeof(ColorPicker), new PropertyMetadata(null, new PropertyChangedCallback(OnColorPropertyChanged)));

        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker cp = (ColorPicker)d;
            cp.DataContext = e.NewValue;
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ColorPicker), new PropertyMetadata(null));


        private void ShowPickerPage()
        {
            ColorPickerPage page = new ColorPickerPage();
            page.SelectedColor = this.SelectedColor;
            page.SelectionChanged += OnColorChanged;
            page.Flyout("ShowPickerPage", new Action(() =>
            {

            }));
        }

        void OnColorChanged(object sender, EventArgs e)
        {
            ColorPickerPage page = (ColorPickerPage)sender;
            this.SelectedColor = page.SelectedColor;
        }

    }
}
