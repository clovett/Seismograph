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
    public sealed partial class SettingsPage : Seismograph.Common.LayoutAwarePage
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
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
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
