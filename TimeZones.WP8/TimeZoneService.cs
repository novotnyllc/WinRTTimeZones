using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZones.Internal;

namespace TimeZones
{
    class TimeZoneService : ITimeZoneService
    {
        public IReadOnlyList<string> SystemTimeZoneIds
        {
            get
            {
                return TimeZoneInfoEx.SystemTimeZoneIds;
            }
        }
        public ITimeZoneEx FindSystemTimeZoneById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("id");

            return new TimeZoneEx(TimeZoneInfoEx.FindSystemTimeZoneById(id));
        }


        private class TimeZoneEx : ITimeZoneEx
        {
            private readonly TimeZoneInfoEx _info;

            public TimeZoneEx(TimeZoneInfoEx info)
            {
                if (info == null) throw new ArgumentNullException("info");
                _info = info;
            }

            public bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset)
            {
                return _info.IsDaylightSavingTime(dateTimeOffset);
            }

            public DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset)
            {
                return _info.ConvertTime(dateTimeOffset);
            }

            public TimeSpan BaseUtcOffset
            {
                get { return _info.BaseUtcOffset; }
            }

            public string StandardName { get { return _info.StandardName; } }

            public string DaylightName
            {
                get { return _info.DaylightName; }
            }

            public string Id { get { return _info.Name; } }
        }
    }
}
