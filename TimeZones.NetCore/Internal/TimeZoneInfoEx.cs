using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeZones.Internal
{
    /// <summary>
    ///     Class to perform time zone conversions
    /// </summary>
    internal sealed class TimeZoneInfoEx
    {
        private static readonly Lazy<List<string>> _timeZones;
        private static readonly Lazy<IDictionary<string, TimeZoneInfoEx>> _timeZoneData;
        private DYNAMIC_TIME_ZONE_INFORMATION _source;

        static TimeZoneInfoEx()
        {
            _timeZoneData = new Lazy<IDictionary<string, TimeZoneInfoEx>>(FillData);
            _timeZones = new Lazy<List<string>>(() => new List<string>(_timeZoneData.Value.Keys));
        }

        private TimeZoneInfoEx(DYNAMIC_TIME_ZONE_INFORMATION source)
        {
            _source = source;
            Name = source.TimeZoneKeyName;
            StandardName = source.StandardName;
            DaylightName = source.DaylightName;

            BaseUtcOffset = new TimeSpan(0, -(source.Bias + source.StandardBias), 0);
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
            var utcDateTime = new SYSTEMTIME(dateTimeOffset.UtcDateTime);

            TIME_ZONE_INFORMATION tzi;
            if (SafeNativeMethods.GetTimeZoneInformationForYear(utcDateTime.Year, ref _source, out tzi))
            {
                SYSTEMTIME destDateTime;
                if (SafeNativeMethods.SystemTimeToTzSpecificLocalTime(ref tzi, ref utcDateTime, out destDateTime))
                {
                    var dt = FromSystemTime(destDateTime);
                    var daylight = IsDaylightTime(dt, ref tzi);

                    var bias = tzi.Bias + tzi.StandardBias;

                    if (daylight)
                        bias += tzi.DaylightBias;

                    var ts = new TimeSpan(0, -bias, 0);

                    var dto = new DateTimeOffset(dt, ts);
                    return dto;
                }
            }

            var error = Marshal.GetLastWin32Error();
            Marshal.ThrowExceptionForHR(error);
            throw new TimeZoneInfoExException(error, "Win32 error occured");
        }
        
        // Based on code from http://www.codeguru.com/cpp/cpp/date_time/routines/article.php/c19485/A-Time-Zone-API-supplement.htm
        private static SYSTEMTIME FindTimeZoneDate(SYSTEMTIME encoded, short year)
        {
            var st = new SYSTEMTIME();

            if (encoded.Month != 0)
            {
                st.Month = encoded.Month;
                st.Day = 1;
                st.Year = year;
                st.Hour = encoded.Hour;

                // Get the day of th week for the first day of the month
                var dt = new DateTime(st.Year, st.Month, st.Day);
                var dayOfWeek = (int)dt.DayOfWeek;

                // get the week offset
                var weekOfMonth = encoded.Day;
                var day = 1;


                // first part of week?
                if (dayOfWeek <= encoded.DayOfWeek)
                {
                    // figure out the day of the month
                    day = 1 + ((weekOfMonth - 1)*7 + (encoded.DayOfWeek - dayOfWeek));
                }
                else
                {
                    // Figure out the day of hte month
                    day = 1 + (weekOfMonth*7 - (dayOfWeek - encoded.DayOfWeek));
                }

                // could be too long
                if (weekOfMonth == 5)
                {
                    // Fix
                    while (day > 31)
                        day -= 7;
                }

                // Fill in the rest
                st.Day = (short)day;
                st.DayOfWeek = encoded.DayOfWeek;

            }

            return st;
        }


        /// <summary>
        ///     Determines if the current datetime value is in daylight time or not
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        public bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset)
        {
            var utcDateTime = new SYSTEMTIME(dateTimeOffset.UtcDateTime);

            TIME_ZONE_INFORMATION tzi;
            if (SafeNativeMethods.GetTimeZoneInformationForYear(utcDateTime.Year, ref _source, out tzi))
            {
                SYSTEMTIME destDateTime;
                if (SafeNativeMethods.SystemTimeToTzSpecificLocalTime(ref tzi, ref utcDateTime, out destDateTime))
                {
                    var dt = FromSystemTime(destDateTime);
                    var daylight = IsDaylightTime(dt, ref tzi);

                    return daylight;
                }
            }

            var error = Marshal.GetLastWin32Error();
            Marshal.ThrowExceptionForHR(error);
            throw new TimeZoneInfoExException(error, "Win32 error occured");
        }

        private static DateTime FromSystemTime(SYSTEMTIME systemTime)
        {
            return new DateTime(systemTime.Year,
                                systemTime.Month,
                                systemTime.Day,
                                systemTime.Hour,
                                systemTime.Minute,
                                systemTime.Second,
                                systemTime.Milliseconds,
                                DateTimeKind.Unspecified);
        }

        // Based on code from http://www.codeguru.com/cpp/cpp/date_time/routines/article.php/c19485/A-Time-Zone-API-supplement.htm
        private static bool IsDaylightTime(DateTime date, ref TIME_ZONE_INFORMATION tzi)
        {
            if (tzi.StandardDate.Month == 0 || tzi.DaylightDate.Month == 0) // Not daylight time in the TZ
                return false;

            // Reuse the DateTime greater/less-than operators and rules
            var stStd = FindTimeZoneDate(tzi.StandardDate, (short)date.Year);
            var stDlt = FindTimeZoneDate(tzi.DaylightDate, (short)date.Year);

            var stdDate = FromSystemTime(stStd);
            var dltDate = FromSystemTime(stDlt);

            // Down under?
            if (stdDate < dltDate)
            {
                // DST is backwards
                if (date < stdDate || date >= dltDate)
                    return true;
            }
            else if (date < stdDate && date >= dltDate)
                return true;

            return false;
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
            return EnumerateSystemTimeZones().Select(tz => new TimeZoneInfoEx(tz)).ToDictionary(tz => tz.Name);
        }


        internal static IEnumerable<DYNAMIC_TIME_ZONE_INFORMATION> EnumerateSystemTimeZones()
        {
            var i = 0;
            while (true)
            {
                DYNAMIC_TIME_ZONE_INFORMATION tz;
                var result = SafeNativeMethods.EnumDynamicTimeZoneInformation(i++, out tz);

                if (result != 0)
                    yield break;

                yield return tz;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DYNAMIC_TIME_ZONE_INFORMATION
        {
            [MarshalAs(UnmanagedType.I4)]
            public Int32 Bias;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string StandardName;

            public SYSTEMTIME StandardDate;

            [MarshalAs(UnmanagedType.I4)]
            public Int32 StandardBias;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DaylightName;

            public SYSTEMTIME DaylightDate;

            [MarshalAs(UnmanagedType.I4)]
            public Int32 DaylightBias;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string TimeZoneKeyName;

            [MarshalAs(UnmanagedType.I1)]
            public bool DynamicDaylightTimeDisabled;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEMTIME
        {
            [MarshalAs(UnmanagedType.U2)]
            public short Year;

            [MarshalAs(UnmanagedType.U2)]
            public short Month;

            [MarshalAs(UnmanagedType.U2)]
            public short DayOfWeek;

            [MarshalAs(UnmanagedType.U2)]
            public short Day;

            [MarshalAs(UnmanagedType.U2)]
            public short Hour;

            [MarshalAs(UnmanagedType.U2)]
            public short Minute;

            [MarshalAs(UnmanagedType.U2)]
            public short Second;

            [MarshalAs(UnmanagedType.U2)]
            public short Milliseconds;

            public SYSTEMTIME(DateTime dt)
            {
                dt = dt.ToUniversalTime(); // SetSystemTime expects the SYSTEMTIME in UTC
                Year = (short)dt.Year;
                Month = (short)dt.Month;
                DayOfWeek = (short)dt.DayOfWeek;
                Day = (short)dt.Day;
                Hour = (short)dt.Hour;
                Minute = (short)dt.Minute;
                Second = (short)dt.Second;
                Milliseconds = (short)dt.Millisecond;
            }
        }

        private static class SafeNativeMethods
        {
            [DllImport("kernel32.dll")]
            internal static extern bool SystemTimeToTzSpecificLocalTime([In] ref TIME_ZONE_INFORMATION lpTimeZoneInformation,
                                                                        [In] ref SYSTEMTIME lpUniversalTime,
                                                                        out SYSTEMTIME lpLocalTime);

            [DllImport("Advapi32.dll")]
            internal static extern int EnumDynamicTimeZoneInformation([In] int dwIndex, out DYNAMIC_TIME_ZONE_INFORMATION lpTimeZoneInformation);


            [DllImport("kernel32.dll")]
            internal static extern bool GetTimeZoneInformationForYear([In] short wYear,
                                                                      [In] ref DYNAMIC_TIME_ZONE_INFORMATION pdtzi,
                                                                      out TIME_ZONE_INFORMATION ptzi);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct TIME_ZONE_INFORMATION
        {
            [MarshalAs(UnmanagedType.I4)]
            public Int32 Bias;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string StandardName;

            public SYSTEMTIME StandardDate;

            [MarshalAs(UnmanagedType.I4)]
            public Int32 StandardBias;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DaylightName;

            public SYSTEMTIME DaylightDate;

            [MarshalAs(UnmanagedType.I4)]
            public Int32 DaylightBias;
        }
    }
}