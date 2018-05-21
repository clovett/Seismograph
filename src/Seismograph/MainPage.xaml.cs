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
        DispatcherTimer timer;
        public MainPage()
        {
            paused = true;
            InitializeComponent();
            this.SizeChanged += MainPage_SizeChanged;
        }

        void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
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
                CheckTimer();
                StopAccelerometer();
            }
        }

        void Start()
        {
            if (paused)
            {
                paused = false;
                SetupAccelerometer();
                StartTimer();
                CheckTimer();
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

            // now we have the colors we can load the series and start the timer.
            Start();

            base.OnNavigatedTo(e);
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(reportInterval);
            timer.Tick += OnTick;
            timer.Stop();
        }

        private void CheckTimer()
        {
            if (timer != null)
            {
                if (!paused)
                {
                    timer.Start();
                    ChartX.Start();
                    ChartY.Start();
                    ChartZ.Start();
                }
                else
                {
                    timer.Stop();
                    ChartX.Stop();
                    ChartY.Stop();
                    ChartZ.Stop();
                }
            }
        }

        void OnSettingsChanged(object sender, EventArgs e)
        {
            // colors may have changed, so pass new colors through to the graph controls.
            SetColors();

            if (timer != null)
            {
                timer.Interval = TimeSpan.FromMilliseconds(Settings.Instance.Interval);
            }
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

        // this contain the last accelerometer reading.
        double x;
        double y;
        double z;

        private void OnTick(object sender, object e)
        {
            CheckOrientation();
            ChartX.SetCurrentValue(x);
            ChartY.SetCurrentValue(y);
            ChartZ.SetCurrentValue(z);
        }

        // give orientation lock 5 seconds delay
        int startTime;
        bool orientationLocked;

        private void CheckOrientation()
        {
            if (startTime == 0)
            {
                startTime = Environment.TickCount;
            }
            else if (!orientationLocked && startTime + 5000 < Environment.TickCount)
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
            x = y = z = 0;
            SetColors();
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
                x = args.Reading.AccelerationX;
                y = args.Reading.AccelerationY;
                z = args.Reading.AccelerationZ + 1;  // reverse gravity to make graph look balanced.
            }
        }

        private void OnPauseClick(object sender, RoutedEventArgs e)
        {
            UnlockOrientation();
            paused = true;
            PauseButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            PlayButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            CheckTimer();
        }

        private void OnPlayClick(object sender, RoutedEventArgs e)
        {
            paused = false;
            CheckTimer();
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
            if (error != null)
            {
                await ShowError(error.Message, AppResources.ErrorSavingSnapshotCaption);
            }
        }

        private async Task ShowError(string message, string title)
        {
            MessageDialog msgbox = new MessageDialog(message, title);
            msgbox.Commands.Add(new UICommand(AppResources.OkCaption));
            await msgbox.ShowAsync();
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            Exception error = null;
            try
            {
                XDocument doc = GetDataAsXml();

                var savePicker = new Windows.Storage.Pickers.FileSavePicker();

                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("XML Files", new List<string>() { ".xml" });
                savePicker.SuggestedFileName = AppResources.DefaultDataFileName;

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    using (var fileStream = await file.OpenStreamForWriteAsync())
                    {
                        XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
                        using (var writer = XmlWriter.Create(fileStream, settings))
                        {
                            doc.WriteTo(writer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            if (error != null)
            {
                await ShowError(error.Message, AppResources.ErrorSavingSnapshotCaption);
            }
        }

        private XDocument GetDataAsXml()
        {
            XDocument doc = new XDocument(new XElement("data"));
            XElement root = doc.Root;

            List<double> xdata = ChartX.History;
            List<double> ydata = ChartY.History;
            List<double> zdata = ChartZ.History;

            int len = Math.Min(xdata.Count, Math.Min(ydata.Count, zdata.Count));
            for (int i = 0; i < len; i++)
            {
                root.Add(new XElement("item", new XAttribute("x", xdata[i]), new XAttribute("y", ydata[i]), new XAttribute("z", zdata[i])));
            }

            return doc;
        }

    }
}
