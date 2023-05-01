using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Timers;

namespace P90Ez.Twitch
{
    /// <summary>
    /// A ConcurrentBag that automaticly removes expires objects.
    /// </summary>
    internal class ExpirableList<T> //ToDo: inherit from generic collection.
    {
        private volatile ConcurrentBag<Tuple<DateTime, T>> collection = new ConcurrentBag<Tuple<DateTime, T>>();

        private Timer timer;

        /// <summary>
        /// The interval in which will be checked if items have expired.
        /// </summary>
        public double Interval
        {
            get { return timer.Interval; }
            set { timer.Interval = value; }
        }

        private TimeSpan expiration;

        /// <summary>
        /// The timespan after which items expire.
        /// </summary>
        public TimeSpan Expiration
        {
            get { return expiration; }
            set { expiration = value; }
        }

        /// <summary>
        /// Defines a ConcurrentBag that automaticly removes expired objects.
        /// </summary>
        /// <param name="Interval">The interval (in seconds) at which items will be checked for expiration.</param>
        /// <param name="Expiration">The TimeSpan an object stays valid inside the collection.</param>
        public ExpirableList(int Interval, TimeSpan Expiration)
        {
            timer = new Timer();
            timer.Interval = Interval * 1000;
            timer.Elapsed += Tick;
            timer.Start();

            expiration = Expiration;
        }

        /// <summary>
        /// Will be invoked by the <see cref="timer"/>. Checks if items are expired.
        /// </summary>
        private void Tick(object sender, EventArgs e)
        {
            while(collection.TryPeek(out var item) && DateTime.Now.Subtract(item.Item1) > expiration) //when there is an item in the collection & the item is expired -> remove
            {
                collection.TryTake(out var res); //remove item
            }
        }

        /// <summary>
        /// Adds an object to the collection.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            collection.Add(new Tuple<DateTime, T>(DateTime.Now, item));
        }

        /// <summary>
        /// Checks if the collection contains the provided item.
        /// </summary>
        /// <returns>True if item was found, otherwise false.</returns>
        public bool Contains(T item)
        {
            try
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    if (collection.ElementAt(i).Item2.Equals(item))
                        return true;
                }
            }
            catch { } //items could be removed while iterating -> index would then be out of range
            return false;
        }
    }
}
