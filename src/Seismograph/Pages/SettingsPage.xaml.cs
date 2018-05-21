using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
        }

        void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            ColorXPicker.SelectedColor = Colors.ParseColor(Settings.Instance.XColor);
            ColorYPicker.SelectedColor = Colors.ParseColor(Settings.Instance.YColor);
            ColorZPicker.SelectedColor = Colors.ParseColor(Settings.Instance.ZColor);
            TextBoxInterval.Text = Settings.Instance.Interval.ToString();
            TextBoxLogSize.Text = Settings.Instance.LogSize.ToString();
        }

        public void ApplyChanges()
        {
            Settings.Instance.XColor = ColorXPicker.SelectedColor.Name;
            Settings.Instance.YColor = ColorYPicker.SelectedColor.Name;
            Settings.Instance.ZColor = ColorZPicker.SelectedColor.Name;
            uint i = 0;
            if (uint.TryParse(TextBoxInterval.Text, out i))
            {
                Settings.Instance.Interval = i;
            }
            if (uint.TryParse(TextBoxLogSize.Text, out i))
            {
                if (i > 5 * 60)
                {
                    i = 5 * 60; // maximum 5 minutes
                }
                Settings.Instance.LogSize = (int)i;
            }
        }
        
        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            this.CloseCurrentFlyout();
        }

        private async Task ShowError(string message, string title)
        {
            MessageDialog msgbox = new MessageDialog(message, title);
            msgbox.Commands.Add(new UICommand(AppResources.OkCaption));
            await msgbox.ShowAsync();
        }

    }
}
