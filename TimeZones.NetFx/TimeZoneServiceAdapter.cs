using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeZones
{
    internal class TimeZoneServiceAdapter : ITimeZoneServiceAdapter
    {
        private List<string> _systemTimeZones;

        public IReadOnlyList<string> SystemTimeZoneIds
        {
            get
            {
                if (_systemTimeZones == null)
                {
                    _systemTimeZones = TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id).ToList();
                }
                return _systemTimeZones;
            }
        }

        public ITimeZoneEx FindSystemTimeZoneById(string id)
        {
            return new TimeZoneEx(TimeZoneInfo.FindSystemTimeZoneById(id));
        }

        public ITimeZoneEx Local
        {
            get
            {
                return new TimeZoneEx(TimeZoneInfo.Local);
            }
        }

        public IEnumerable<ITimeZoneEx> GetAllTimeZones()
        {
            return TimeZoneInfo.GetSystemTimeZones().Select(tz => new TimeZoneEx(tz));
        }

        [DebuggerDisplay("Id = {Id}, BaseUtcOffset = {BaseUtcOffset}")]
        private class TimeZoneEx : ITimeZoneEx, IEquatable<TimeZoneEx>, IEquatable<ITimeZoneEx>
        {
            private readonly TimeZoneInfo _info;

            public TimeZoneEx(TimeZoneInfo info)
            {
                if (info == null) throw new ArgumentNullException("info");
                _info = info;
            }

            public bool Equals(ITimeZoneEx other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id.Equals(other.Id);
            }

            public bool Equals(TimeZoneEx other)
            {
                return Equals((ITimeZoneEx)other);
            }

            public bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset)
            {
                return _info.IsDaylightSavingTime(dateTimeOffset);
            }

            public DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset)
            {
                return TimeZoneInfo.ConvertTime(dateTimeOffset, _info);
            }

            public TimeSpan BaseUtcOffset
            {
                get { return _info.BaseUtcOffset; }
            }

            public string StandardName
            {
                get { return _info.StandardName; }
            }

            public string DaylightName
            {
                get { return _info.DaylightName; }
            }

            public string Id
            {
                get { return _info.Id; }
            }

            public override bool Equals(object obj)
            {
                return Equals((ITimeZoneEx)obj);
            }

            public override int GetHashCode()
            {
                return (Id != null ? Id.GetHashCode() : 0);
            }

            public static bool operator ==(TimeZoneEx left, TimeZoneEx right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(TimeZoneEx left, TimeZoneEx right)
            {
                return !Equals(left, right);
            }
        }
    }
}