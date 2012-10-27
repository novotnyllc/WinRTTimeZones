using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeZones
{
    class TimeZoneService : ITimeZoneService
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

        public DateTimeOffset ConvertTimeBySystemTimeZoneId(DateTimeOffset dateTimeOffset, string destinationTimeZoneId)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTimeOffset, destinationTimeZoneId);
        }

        private class TimeZoneEx : ITimeZoneEx
        {
            private readonly TimeZoneInfo _info;

            public TimeZoneEx(TimeZoneInfo info)
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
                return TimeZoneInfo.ConvertTime(dateTimeOffset, _info);
            }

            public TimeSpan BaseUtcOffset {
                get { return _info.BaseUtcOffset; }
            }
            public string StandardName { get { return _info.StandardName; } }
            public bool SupportsDaylightSavingTime {
                get { return _info.SupportsDaylightSavingTime; }
            }

            public string DaylightName
            {
                get { return _info.DaylightName; }
            }

            public string Id { get { return _info.Id; } }
        }
    }

    
}
