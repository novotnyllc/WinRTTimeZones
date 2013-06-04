using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TZI = TimeZones.WP8.Native.TimeZoneInfoEx;

namespace TimeZones.Internal
{
    /// <summary>
    ///     Class to perform time zone conversions
    /// </summary>
    internal sealed class TimeZoneInfoEx
    {
        private static readonly Lazy<List<string>> _timeZones;
        private static readonly Lazy<IDictionary<string, TimeZoneInfoEx>> _timeZoneData;
        private readonly TZI _source;

        static TimeZoneInfoEx()
        {
            _timeZoneData = new Lazy<IDictionary<string, TimeZoneInfoEx>>(FillData);
            _timeZones = new Lazy<List<string>>(() => new List<string>(_timeZoneData.Value.Keys));
        }

        private TimeZoneInfoEx(TZI source)
        {
            _source = source;
            Name = source.Name;
            StandardName = source.StandardName;
            DaylightName = source.DaylightName;
            BaseUtcOffset = source.BaseUtcOffset;
        }

        /// <summary>
        ///     All available time zones
        /// </summary>
        public static IReadOnlyList<string> SystemTimeZoneIds
        {
            get { return _timeZones.Value; }
        }

        /// <summary>
        ///     Invariant name such as "Eastern Standard Time"
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Localized name for the standard time
        /// </summary>
        public string StandardName { get; private set; }

        /// <summary>
        ///     Localized name for the daylight time
        /// </summary>
        public string DaylightName { get; private set; }

        public TimeSpan BaseUtcOffset { get; private set; }
        

        /// <summary>
        ///     Gets a TimeZoneInfo by id.
        /// </summary>
        /// <param name="id">Invariant Time Zone name. See TimeZones property for full list.</param>
        /// <returns></returns>
        public static TimeZoneInfoEx FindSystemTimeZoneById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("id");

            TimeZoneInfoEx tz;
            if (_timeZoneData.Value.TryGetValue(id, out tz))
            {
                return tz;
            }

            throw new TimeZoneInfoExException(-1, "Time Zone is not in the list of TimeZones");
        }

        public static TimeZoneInfoEx GetLocalTimeZone()
        {
            var id = TZI.GetLocalTimeId();

            return FindSystemTimeZoneById(id);
        }

        private bool Equals(TimeZoneInfoEx other)
        {
            return string.Equals(Name, other.Name);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        /// <param name="obj"> The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is TimeZoneInfoEx && Equals((TimeZoneInfoEx)obj);
        }

        /// <summary>
        ///     Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///     A hash code for the current <see cref="T:System.Object" /> .
        /// </returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <summary>
        ///     Gets a DateTimeOffset for this time zone
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        public DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset)
        {
            TimeSpan offset;
            var dto = _source.ConvertTime(dateTimeOffset.ToUniversalTime(), out offset);

            // The UtcDateTime here isn't really UTC -- it's the converted local time
            var dt = DateTime.SpecifyKind(dto.UtcDateTime, DateTimeKind.Unspecified);
            var converted = new DateTimeOffset(dt, offset);

            return converted;
        }



        /// <summary>
        ///     Converts a DateTimeOffset to one in the specified system time zone
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <param name="destinationTimeZoneId"></param>
        /// <returns></returns>
        public static DateTimeOffset ConvertTimeBySystemTimeZoneId(DateTimeOffset dateTimeOffset, string destinationTimeZoneId)
        {
            var tz = FindSystemTimeZoneById(destinationTimeZoneId);
            return tz.ConvertTime(dateTimeOffset);
        }

        private static IDictionary<string, TimeZoneInfoEx> FillData()
        {
            return TZI.CreateMap().Values.Select(tz => new TimeZoneInfoEx(tz)).ToDictionary(tz => tz.Name);
        }


        public bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset)
        {
            return _source.IsDaylightSavingTime(dateTimeOffset.ToUniversalTime());
        }
    }
}