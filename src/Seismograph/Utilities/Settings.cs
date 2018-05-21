using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;

namespace Seismograph
{
    public class Settings
    {
        private uint interval;
        private uint minInterval;
        private string xColor;
        private string yColor;
        private string zColor;
        private string snapshotColor;
        private int logSize = 60 * 5; // 5 minutes

        public const string RemoveAdsProductId = "RemoveAdsId";

        const string SettingsFile = "settings.xml";

        public Settings()
        {
            var sensor = Accelerometer.GetDefault();
            if (sensor != null)
            {
                minInterval = sensor.MinimumReportInterval;
                Interval = minInterval;
            }
            else
            {
                Interval = 100;
            }
            XColor = "#FF9E0F00";
            YColor = "Green";
            ZColor = "#FF0A3B9D";
            SnapshotColor = "DarkGray";
        }

        private static Settings instance;

        public static Settings Instance
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        public event EventHandler Changed;

        void OnChanged()
        {
            if (Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        public async  static Task<Settings> LoadSettings()
        {
            CacheFolder folder = new CacheFolder("SeismographCache");
            IsolatedStorage<Settings> store = new IsolatedStorage<Settings>(folder);
            Settings settings = await store.LoadFromFileAsync(SettingsFile);
            if (settings == null)
            {
                settings = new Settings();
            }
            return settings;
        }

        public async void SaveSettings()
        {
            try
            {

                CacheFolder folder = new CacheFolder("SeismographCache");
                IsolatedStorage<Settings> store = new IsolatedStorage<Settings>(folder);
                await store.SaveToFileAsync(SettingsFile, this);
            }
            catch
            {
            }
        }


        public uint Interval
        {

            get { return interval; }

            set
            {
                if (value < minInterval)
                {
                    value = minInterval;
                }
                if (interval != value)
                {
                    interval = value;
                    OnChanged();
                }
            }
        }
        public string XColor
        {

            get { return xColor; }

            set
            {
                if (xColor != value)
                {
                    xColor = value;
                    OnChanged();
                }
            }
        }

        public string YColor
        {

            get { return yColor; }

            set
            {
                if (yColor != value)
                {
                    yColor = value;
                    OnChanged();
                }
            }
        }

        public string ZColor
        {

            get { return zColor; }

            set
            {
                if (zColor != value)
                {
                    zColor = value;
                    OnChanged();
                }
            }
        }

        public string SnapshotColor
        {
            get { return snapshotColor; }

            set
            {
                if (snapshotColor != value)
                {
                    snapshotColor = value;
                    OnChanged();
                }
            }
        }

        public int LogSize
        {
            get { return this.logSize; }
            set
            {
                if (this.logSize != value)
                {
                    this.logSize = value;
                    OnChanged();
                }
            }
        }
        

    }


}
