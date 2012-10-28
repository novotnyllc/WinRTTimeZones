// TimeZones.WP8.Native.cpp
#include "pch.h"
#include "TimeZones.WP8.Native.h"

using namespace Windows;
using namespace Platform;
using namespace Windows::Foundation::Collections;
using namespace Platform::Collections;


namespace TimeZones_WP8_Native
{

TimeZoneInfoEx::TimeZoneInfoEx(DYNAMIC_TIME_ZONE_INFORMATION tz) : _source(tz)
{
	Name = ref new String(tz.TimeZoneKeyName);	
	StandardName = ref new String(tz.StandardName);
	DaylightName = ref new String(tz.DaylightName);

	TimeSpan ts;
	ts.Duration = SecondsToTicks(-(tz.Bias + tz.StandardBias));
	BaseUtcOffset = ts;
}

// static init
//IMap<String^, TimeZoneInfoEx^>^ TimeZoneInfoEx::_timeZoneData = TimeZoneInfoEx::CreateMap();

IMap<String^, TimeZoneInfoEx^>^ TimeZoneInfoEx::CreateMap()
{
			auto map = ref new Map<String^, TimeZoneInfoEx^>();

			auto i(0);
			while(true)
			{
				DYNAMIC_TIME_ZONE_INFORMATION tz;
				auto result = EnumDynamicTimeZoneInformation(i++, &tz);
				if(result != 0)
					break;

				map->Insert(ref new String(tz.TimeZoneKeyName) , ref new TimeZoneInfoEx(tz));
			}

			return map;
}

bool TimeZoneInfoEx::IsDaylightSavingTime(DateTime dateTime)
{
	auto utcDateTime = DateTimeToSystemTime(&dateTime);
	TIME_ZONE_INFORMATION tzi;

	if(GetTimeZoneInformationForYear(utcDateTime.wYear, &_source, &tzi))
	{
		SYSTEMTIME destDateTime;
		if(SystemTimeToTzSpecificLocalTime(&tzi, &utcDateTime, &destDateTime))
		{

			auto dt = SystemTimeToDateTime(&destDateTime);
			auto daylight = IsDaylightTime(&destDateTime, &tzi);

			return daylight;
		}
	}

	auto error = GetLastError();
	throw ref new Exception(error, "Win32 Error occurred");
}

DateTime TimeZoneInfoEx::ConvertTime(DateTime dateTime, TimeSpan* offset)
{

	auto utcDateTime = DateTimeToSystemTime(&dateTime);
	TIME_ZONE_INFORMATION tzi;

	if(GetTimeZoneInformationForYear(utcDateTime.wYear, &_source, &tzi))
	{
		SYSTEMTIME destDateTime;
		if(SystemTimeToTzSpecificLocalTime(&tzi, &utcDateTime, &destDateTime))
		{

			auto dt = SystemTimeToDateTime(&destDateTime);
			auto daylight = IsDaylightTime(&destDateTime, &tzi);


			auto bias = tzi.Bias + tzi.StandardBias;
			
			if(daylight)
				bias += tzi.DaylightBias;

			offset->Duration = SecondsToTicks(-bias);
			return dt;
		}
	}

	

	auto error = GetLastError();
	throw ref new Exception(error, "Win32 Error occurred");
}


bool TimeZoneInfoEx::IsDaylightTime(const SYSTEMTIME* date, const TIME_ZONE_INFORMATION* tzi)
{
	if(tzi->StandardDate.wMonth == 0 || tzi->DaylightDate.wMonth == 0) // no daylight time in the TZ
	{
		return false;
	}

	auto destDate = SystemTimeToDateTime(date);
	// convert these to date times so we can do simple comparisons
	auto stdDate = SystemTimeToDateTime(tzi->StandardDate, date->wYear);
	auto dltDate = SystemTimeToDateTime(tzi->DaylightDate, date->wYear);

	if(destDate.UniversalTime >= dltDate.UniversalTime && destDate.UniversalTime < stdDate.UniversalTime)
		return true;

	return false;
}

//
//TimeZoneInfoEx^ TimeZoneInfoEx::FindSystemTimeZoneById(String^ id)
//{
//	return _timeZoneData->Lookup(id);
//}

}