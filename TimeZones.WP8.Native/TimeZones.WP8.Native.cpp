// TimeZones.WP8.Native.cpp
#include "pch.h"
#include "TimeZones.WP8.Native.h"

using namespace Windows;
using namespace Platform;
using namespace Windows::Foundation::Collections;
using namespace Platform::Collections;


namespace TimeZones
{
namespace WP8
{
namespace Native
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


	// Adjust the encoded value to this year's values
	SYSTEMTIME sttzd, sttzs;
	FindTimezoneDate(&tzi->DaylightDate, date->wYear, &sttzd);
	FindTimezoneDate(&tzi->StandardDate, date->wYear, &sttzs);
	
	
	auto destDate = SystemTimeToDateTime(date).UniversalTime;
	// convert these to date times so we can do simple comparisons

	auto stdDate = SystemTimeToDateTime(&sttzs).UniversalTime;
	auto dltDate = SystemTimeToDateTime(&sttzd).UniversalTime;


	// Down under?
    if (stdDate < dltDate)
    {
        // DST is backwards
        if (destDate < stdDate || destDate >= dltDate)
            return true;
    }
    else if (destDate < stdDate && destDate >= dltDate)
        return true;


	return false;
}

//
//TimeZoneInfoEx^ TimeZoneInfoEx::FindSystemTimeZoneById(String^ id)
//{
//	return _timeZoneData->Lookup(id);
//}

}
}
}