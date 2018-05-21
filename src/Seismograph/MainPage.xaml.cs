using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.ApplicationModel.Store.LicenseManagement;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Seismograph
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool loaded;
        Accelerometer sensor;
        bool paused;
        Model model = new Model();
        DateTimeOffset? startTime;

        public MainPage()
        {
            paused = true;
            InitializeComponent();

            ChartX.ValueFilter = new ValuePredicate((e) => { return e is Acceleration; });
            ChartX.ValueGetter = new ValueGetter((e) => { return ((Acceleration)e).X; });
            ChartX.Model = model;

            ChartY.ValueFilter = new ValuePredicate((e) => { return e is Acceleration; });
            ChartY.ValueGetter = new ValueGetter((e) => { return ((Acceleration)e).Y; });
            ChartY.Model = model;

            ChartZ.ValueFilter = new ValuePredicate((e) => { return e is Acceleration; });
            ChartZ.ValueGetter = new ValueGetter((e) => { return ((Acceleration)e).Z; });
            ChartZ.Model = model;
        }

        private void ShowDebugMessage(string msg)
        {
#if DEBUG
            //ShowErrorMessage(msg);
#endif
        }

        public void OnVisibilityChanged(bool visible)
        {
            if (loaded)
            {
                if (visible)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }

        void Stop()
        {
            if (!paused)
            {
                paused = true;
                StartStopCharts();
                StopAccelerometer();
            }
        }

        void Start()
        {
            if (paused)
            {
                paused = false;
                SetupAccelerometer();
                StartStopCharts();
                SetColors();
            }
        }

        int reportInterval = 30;

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Settings.Instance = await Settings.LoadSettings();
            Settings.Instance.Changed += OnSettingsChanged;
            await Colors.Instance.Load();

            loaded = true;

            Start();

            base.OnNavigatedTo(e);
        }

        private void StartStopCharts()
        {
            if (!paused)
            {
                ChartX.Start();
                ChartY.Start();
                ChartZ.Start();
            }
            else
            {
                ChartX.Stop();
                ChartY.Stop();
                ChartZ.Stop();
            }
        }

        void OnSettingsChanged(object sender, EventArgs e)
        {
            // colors may have changed, so pass new colors through to the graph controls.
            SetColors();
            if (sensor != null)
            {
                sensor.ReportInterval = Settings.Instance.Interval;
            }
        }

        private void SetColors()
        {
            ChartX.Stroke = new SolidColorBrush(Colors.ParseColor(Settings.Instance.XColor).Color);
            ChartY.Stroke = new SolidColorBrush(Colors.ParseColor(Settings.Instance.YColor).Color);
            ChartZ.Stroke = new SolidColorBrush(Colors.ParseColor(Settings.Instance.ZColor).Color);
        }

        // give orientation lock 5 seconds delay
        bool orientationLocked;

        private void CheckOrientation()
        {
            if (!startTime.HasValue)
            {
                // do nothing
            }
            else if (!orientationLocked && DateTimeOffset.Now - startTime.Value > TimeSpan.FromSeconds(5))
            {
                LockOrientation();
            }
        }

        private void LockOrientation()
        {
            orientationLocked = true;
            DisplayInformation.AutoRotationPreferences = DisplayInformation.GetForCurrentView().CurrentOrientation;
        }

        private void UnlockOrientation()
        {
            orientationLocked = false;
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape | DisplayOrientations.LandscapeFlipped | DisplayOrientations.Portrait | DisplayOrientations.PortraitFlipped;
        }

        private void Clear()
        {
            ChartX.Clear();
            ChartY.Clear();
            ChartZ.Clear();
            model.Clear();
            SetColors();
            startTime = null;
        }

        private void SetupAccelerometer()
        {
            try
            {
                sensor = Accelerometer.GetDefault();
            }
            catch
            {

            }
            if (sensor == null)
            {
                ShowErrorMessage(AppResources.AccelerometerNotFound);
            }
            else
            {
                sensor.ReportInterval = Settings.Instance.Interval;
                reportInterval = (int)sensor.MinimumReportInterval;
                sensor.ReadingChanged += OnAccelerometerChanged;
            }
        }

        private void StopAccelerometer()
        {
            if (sensor != null)
            {
                sensor.ReadingChanged -= OnAccelerometerChanged;
                sensor = null;
            }
        }

        private void ShowErrorMessage(string errorMessage)
        {
            MessagePanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            MessageText.Text = errorMessage;
        }

        void OnAccelerometerChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            if (!paused)
            {
                if (!startTime.HasValue)
                {
                    startTime = args.Reading.Timestamp;
                }
                long t = (long)((args.Reading.Timestamp - startTime.Value).TotalMilliseconds);
                double x = args.Reading.AccelerationX;
                double y = args.Reading.AccelerationY;
                double z = args.Reading.AccelerationZ + 1;  // reverse gravity to make graph look balanced.
                model.AddAccel(t, x, y, z);
            }
        }

        private void OnPauseClick(object sender, RoutedEventArgs e)
        {
            UnlockOrientation();
            paused = true;
            PauseButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            PlayButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            StartStopCharts();
        }

        private void OnPlayClick(object sender, RoutedEventArgs e)
        {
            paused = false;
            StartStopCharts();
            PauseButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            PlayButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            SettingsPage page = new SettingsPage();
            page.Flyout("ShowPickerPage", new Action(() =>
            {
                // update the new settings
                Settings.Instance.SaveSettings();
                page.ApplyChanges();
            }));
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void OnSnapshotClick(object sender, RoutedEventArgs e)
        {
            string fileName = AppResources.DefaultSnapshotFileName + ".jpg";
            SaveSnapshot(fileName);
        }

        async void SaveSnapshot(string fileName)
        {
            bool saved = this.paused;
            this.paused = true;
            this.StartStopCharts();
            Exception error = null;
            try
            {
#if DEBUG
                FrameworkElement element = RootLayout;
#else
                FrameworkElement element = ContentPanel;
#endif
                RenderTargetBitmap bitmap = new RenderTargetBitmap();
                await bitmap.RenderAsync(element);

                IBuffer pixelBuffer = await bitmap.GetPixelsAsync();
                byte[] pixels = pixelBuffer.ToArray();

                var savePicker = new Windows.Storage.Pickers.FileSavePicker();

                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("PNG Images", new List<string>() { ".png" });
                savePicker.SuggestedFileName = AppResources.DefaultSnapshotFileName;

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    using (var fileStream = await file.OpenStreamForWriteAsync())
                    {
                        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream.AsRandomAccessStream());
                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight,
                            96, 96, pixels);
                        await encoder.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            this.paused = saved;
            if (error != null)
            {
                await ShowError(error.Message, AppResources.ErrorSavingSnapshotCaption);
            }
            this.StartStopCharts();
        }

        private async Task ShowError(string message, string title)
        {
            MessageDialog msgbox = new MessageDialog(message, title);
            msgbox.Commands.Add(new UICommand(AppResources.OkCaption));
            await msgbox.ShowAsync();
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            bool saved = this.paused;
            this.paused = true;
            this.StartStopCharts();
            Exception error = null;
            try
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();

                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("CSV Files", new List<string>() { ".csv" });
                savePicker.SuggestedFileName = AppResources.DefaultDataFileName;

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    using (var fileStream = await file.OpenStreamForWriteAsync())
                    {
                        using (var writer = new StreamWriter(fileStream))
                        {
                            model.WriteTo(writer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            this.paused = saved;
            if (error != null)
            {
                await ShowError(error.Message, AppResources.ErrorSavingSnapshotCaption);
            }
            this.StartStopCharts();
        }

    }
}
