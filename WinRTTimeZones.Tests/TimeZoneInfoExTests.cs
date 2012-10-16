using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TimeZones;

namespace WinRTTimeZones.Tests
{
    [TestClass]
    public class TimeZoneInfoExTests
    {
        [TestMethod]
        public void GetEasternDateTest()
        {
            TimeZoneInfoEx.SystemTimeZoneIds.Should().Contain("Eastern Standard Time");
        }

        [TestMethod]
        public void ConvertTimeToMountanPre2007Test()
        {
            var dt = new DateTime(2006, 3, 15, 12, 0, 0, DateTimeKind.Utc);

            // -7 hours
            var local = TimeZoneInfoEx.ConvertTimeBySystemTimeZoneId(dt, "Mountain Standard Time");


            local.Hour.Should().Be(5);
            local.Offset.Should().Be(TimeSpan.FromHours(-7));
        }

        [TestMethod]
        public void ConvertTimeToCentralDaylightTest()
        {
            var dt = new DateTime(1990, 7, 1, 12, 0, 0, DateTimeKind.Utc);

            // -6 hours Std, -5 Daylight
            var tz = TimeZoneInfoEx.FindSystemTimeZoneById("Central Standard Time");
            var local = tz.ConvertTime(dt);


            local.Hour.Should().Be(7);
            local.Offset.Should().Be(TimeSpan.FromHours(-5)); // CDT
        }
        [TestMethod]
        public void ConvertTimeToMountain2007Test()
        {
            var dt = new DateTime(2007, 3, 15, 12, 0, 0, DateTimeKind.Utc);

            // -7 hours
            var local = TimeZoneInfoEx.ConvertTimeBySystemTimeZoneId(dt, "Mountain Standard Time");


            local.Hour.Should().Be(6);
            local.Offset.Should().Be(TimeSpan.FromHours(-6)); // Daylight time in 2007
        }

        [TestMethod]
        public void ConvertTimeAcrossDateBoundaryLessUtc()
        {
            var dt = new DateTime(2007, 3, 16, 1, 0, 0, DateTimeKind.Utc);

            // -7 hours
            var local = TimeZoneInfoEx.ConvertTimeBySystemTimeZoneId(dt, "Mountain Standard Time");


            local.Day.Should().Be(15);
            local.Hour.Should().Be(19);
            local.Offset.Should().Be(TimeSpan.FromHours(-6));
        }

        [TestMethod]
        public void ConvertTimeAcrossDateBoundaryGreaterUtc()
        {
            var dt = new DateTime(2007, 3, 15, 23, 0, 0, DateTimeKind.Utc);

            // +9 hours
            var local = TimeZoneInfoEx.ConvertTimeBySystemTimeZoneId(dt, "Tokyo Standard Time");


            local.Day.Should().Be(16);
            local.Hour.Should().Be(8);
            local.Offset.Should().Be(TimeSpan.FromHours(9));
        }
    }
}
