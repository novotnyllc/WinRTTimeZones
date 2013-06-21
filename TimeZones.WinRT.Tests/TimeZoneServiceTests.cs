using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TimeZones.WinRT;

namespace WinRTTimeZones.Tests
{
    [TestClass]
    public class TimeZoneInfoExTests
    {
        [TestMethod]
        public void GetEasternDateTest()
        {
            TimeZoneService.SystemTimeZoneIds.Should().Contain("Eastern Standard Time");
        }

        [TestMethod]
        public void UtcTest()
        {
            var tz = TimeZoneService.Utc;

            Assert.AreEqual("UTC", tz.Id);
            Assert.AreEqual(new TimeSpan(0), tz.BaseUtcOffset);
        }

        [TestMethod]
        public void AllTimeZonesMatchesSystemIds()
        {
            TimeZoneService.AllTimeZones.Select(tz => tz.Id).ShouldAllBeEquivalentTo(TimeZoneService.SystemTimeZoneIds);
        }

        [TestMethod]
        public void LocalTimeZoneTest()
        {
            var tz = TimeZoneService.Local;

            Assert.AreEqual("Eastern Standard Time", tz.Id);
        }


        [TestMethod]
        public void ConvertTimeDaylightTimeToHawaiiTest()
        {
            var dt = new DateTime(1990, 7, 1, 12, 0, 0, DateTimeKind.Utc);


            // -10
            var tz = TimeZoneService.FindSystemTimeZoneById("Hawaiian Standard Time");
            
            var local = tz.ConvertTime(dt);

            local.Hour.Should().Be(2);
            local.Offset.Should().Be(TimeSpan.FromHours(-10)); // HST
        }

        [TestMethod]
        public void ConvertTimeDaylightTimeToArizonaTest()
        {
            var dt = new DateTime(1990, 7, 1, 12, 0, 0, DateTimeKind.Utc);

            // -7
            var tz = TimeZoneService.FindSystemTimeZoneById("US Mountain Standard Time");
            
            var local = tz.ConvertTime(dt);

            local.Hour.Should().Be(5);
            local.Offset.Should().Be(TimeSpan.FromHours(-7)); // MST
        }

        [TestMethod]
        public void ConvertTimeToMountanPre2007Test()
        {
            var dt = new DateTime(2006, 3, 15, 12, 0, 0, DateTimeKind.Utc);

            // -7 hours
            var local = TimeZoneService.ConvertTimeBySystemTimeZoneId(dt, "Mountain Standard Time");


            local.Hour.Should().Be(5);
            local.Offset.Should().Be(TimeSpan.FromHours(-7));
        }

        [TestMethod]
        public void ConvertTimeToMountain2012Test()
        {
            var dt = new DateTime(2012, 11, 1, 12, 0, 0, DateTimeKind.Utc);

            // -7 hours
            var local = TimeZoneService.ConvertTimeBySystemTimeZoneId(dt, "Mountain Standard Time");


            local.Hour.Should().Be(6);
            local.Offset.Should().Be(TimeSpan.FromHours(-6)); // Daylight time in 2012
        }

        [TestMethod]
        public void BaseUtcOffsetShouldBeCorrect()
        {
            var tz = TimeZoneService.FindSystemTimeZoneById("Central Standard Time");
            tz.BaseUtcOffset.Should().Be(TimeSpan.FromHours(-6));
        }

        [TestMethod]
        public void SpecifyTimeZoneStandardUnspecifiedTest()
        {
            var dt = new DateTime(1990, 7, 1, 12, 0, 0, DateTimeKind.Unspecified); // just a test 
            var tz = TimeZoneService.FindSystemTimeZoneById("Central Standard Time");

            var result = TimeZoneService.SpecifyTimeZone(dt, tz);

            result.Hour.Should().Be(12); // same
            result.Offset.Should().Be(TimeSpan.FromHours(-5));
        }

        [TestMethod]
        public void SpecifyTimeZoneStandardUtcTest()
        {
            var dt = new DateTime(1990, 7, 1, 12, 0, 0, DateTimeKind.Utc); // just a test 
            var tz = TimeZoneService.FindSystemTimeZoneById("Central Standard Time");

            var result = TimeZoneService.SpecifyTimeZone(dt, tz);

            result.Hour.Should().Be(12); // same
            result.Offset.Should().Be(TimeSpan.FromHours(-5));
        }


        [TestMethod]
        public void SpecifyTimeZoneStandardTest()
        {
            var dt = new DateTime(1990, 7, 1, 12, 0, 0, DateTimeKind.Local); // just a test 
            var tz = TimeZoneService.FindSystemTimeZoneById("Central Standard Time");

            var result = TimeZoneService.SpecifyTimeZone(dt, tz);

            result.Hour.Should().Be(12); // same
            result.Offset.Should().Be(TimeSpan.FromHours(-5));
        }

        [TestMethod]
        public void SpecifyTimeZoneDaylightTest()
        {
            var dt = new DateTime(1990, 2, 1, 12, 0, 0, DateTimeKind.Local); // just a test 
            var tz = TimeZoneService.FindSystemTimeZoneById("Central Standard Time");

            var result = TimeZoneService.SpecifyTimeZone(dt, tz);

            result.Hour.Should().Be(12); // same
            result.Offset.Should().Be(TimeSpan.FromHours(-6));
        }



        [TestMethod]
        public void ConvertTimeToCentralDaylightTest()
        {
            var dt = new DateTime(1990, 7, 1, 12, 0, 0, DateTimeKind.Utc);

            // -6 hours Std, -5 Daylight
            var tz = TimeZoneService.FindSystemTimeZoneById("Central Standard Time");
            var local = tz.ConvertTime(dt);


            local.Hour.Should().Be(7);
            local.Offset.Should().Be(TimeSpan.FromHours(-5)); // CDT
        }
        [TestMethod]
        public void ConvertTimeToMountain2007Test()
        {
            var dt = new DateTime(2007, 3, 15, 12, 0, 0, DateTimeKind.Utc);

            // -7 hours
            var local = TimeZoneService.ConvertTimeBySystemTimeZoneId(dt, "Mountain Standard Time");


            local.Hour.Should().Be(6);
            local.Offset.Should().Be(TimeSpan.FromHours(-6)); // Daylight time in 2007
        }

        [TestMethod]
        public void ConvertTimeAcrossDateBoundaryLessUtc()
        {
            var dt = new DateTime(2007, 3, 16, 1, 0, 0, DateTimeKind.Utc);

            // -7 hours
            var local = TimeZoneService.ConvertTimeBySystemTimeZoneId(dt, "Mountain Standard Time");


            local.Day.Should().Be(15);
            local.Hour.Should().Be(19);
            local.Offset.Should().Be(TimeSpan.FromHours(-6));
        }

        [TestMethod]
        public void ConvertTimeAcrossYearBoundaryLessUtc()
        {
            var dt = new DateTime(2007, 1, 1, 1, 0, 0, DateTimeKind.Utc);

            // -7 hours
            var local = TimeZoneService.ConvertTimeBySystemTimeZoneId(dt, "Mountain Standard Time");

            local.Year.Should().Be(2006);
            local.Month.Should().Be(12);
            local.Day.Should().Be(31);
            local.Hour.Should().Be(18);
            local.Offset.Should().Be(TimeSpan.FromHours(-7));
        }

        [TestMethod]
        public void ConvertTimeAcrossDateBoundaryGreaterUtc()
        {
            var dt = new DateTime(2007, 3, 15, 23, 0, 0, DateTimeKind.Utc);

            // +9 hours
            var local = TimeZoneService.ConvertTimeBySystemTimeZoneId(dt, "Tokyo Standard Time");


            local.Day.Should().Be(16);
            local.Hour.Should().Be(8);
            local.Offset.Should().Be(TimeSpan.FromHours(9));
        }

        [TestMethod]
        public void ConvertTimeAcrossYearBoundaryGreaterUtc()
        {
            var dt = new DateTime(2006, 12, 31, 23, 0, 0, DateTimeKind.Utc);

            // +9 hours
            var local = TimeZoneService.ConvertTimeBySystemTimeZoneId(dt, "Tokyo Standard Time");

            local.Year.Should().Be(2007);
            local.Day.Should().Be(1);
            local.Month.Should().Be(1);
            local.Hour.Should().Be(8);
            local.Offset.Should().Be(TimeSpan.FromHours(9));
        }
    }
}
