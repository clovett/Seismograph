using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Seismograph
{
    public class Entry
    {
        public Entry()
        {
            Type = 0;
        }
        public int Type;
        public long Timestamp;

        internal virtual void WriteTo(StreamWriter writer)
        {

        }
    }
    public class Acceleration : Entry
    {
        public Acceleration()
        {
            Type = 1;
            X = Y = Z = 0;
        }
        public double X;
        public double Y;
        public double Z;

        internal override void WriteTo(StreamWriter writer)
        {
            writer.WriteLine("{0},{1},{2},{3}", Timestamp, X, Y, Z);
        }
    }

    public class GPS : Entry
    {
        public GPS()
        {
            Type = 2;
        }
        public double Lat;
        public double Lon;
        public double Alt;
    }

    public delegate bool ValuePredicate(Entry e);
    public delegate double ValueGetter(Entry e);

    public class Model
    {
        public Model()
        {
            History = new List<Entry>();
        }

        public List<Entry> History; 

        internal void Clear()
        {
            History = new List<Entry>();
        }

        public long MostRecentTime
        {
            get
            {
                if (History.Count > 0)
                {
                    return History[History.Count - 1].Timestamp;
                }
                return 0;
            }
        }

        internal void AddAccel(long t, double x, double y, double z)
        {
            History.Add(new Acceleration() { Timestamp = t, X = x, Y = y, Z = z });

            int span = Settings.Instance.LogSize * 1000; // in seconds
            if (History.Count > 1)
            {
                long lastTime = this.MostRecentTime;
                if (lastTime - History[0].Timestamp > span)
                {
                    // time to prune some values so we don't run out of memory!
                    int n = History.Count;
                    int i = 0;
                    for (i = 0; i < n; i++)
                    {
                        if (lastTime - History[i].Timestamp <= span)
                        {
                            break;
                        }
                    }
                    History.RemoveRange(0, i);
                }
            }
        }

        internal void WriteTo(StreamWriter writer)
        {
            writer.WriteLine("Time,X,Y,Z");
            foreach (var item in this.History)
            {
                item.WriteTo(writer);
            }
        }

        /// <summary>
        /// Get number of values from end of the log
        /// </summary>
        /// <param name="size">How many to return</param>
        /// <param name="msPerValue">How many milliseconds between values</param>
        /// <param name="filter">Filter on Entry types</param>
        /// <param name="getter">Delegate that projects a specific field of each entry</param>
        /// <returns></returns>
        public List<double> Tail(int size, int msPerValue, ValuePredicate filter, ValueGetter getter)
        {
            List<double> result = new List<double>();

            long time = 0;

            for (int i = this.History.Count - 1; i >= 0; i--)
            {
                Entry e = this.History[i];
                if ((time == 0 || time - e.Timestamp > msPerValue) && filter(e))
                {
                    result.Add(getter(e));
                    time = e.Timestamp;

                    if (result.Count >= size)
                    {
                        break;
                    }
                }

            }
            result.Reverse();
            return result;
        } 

    }
}
