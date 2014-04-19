// TimeZones.WPA81.Native.cpp
#include "pch.h"
#include "TimeZones.WPA81.Native.h"

using namespace Windows;
using namespace Platform;
using namespace Windows::Foundation::Collections;
using namespace Platform::Collections;


namespace TimeZones
{
namespace WPA81
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

String^ TimeZoneInfoEx::GetLocalTimeStandardName()
{
	DYNAMIC_TIME_ZONE_INFORMATION tz;
	auto result = GetDynamicTimeZoneInformation(&tz);
	if(result == 0 || result == 1 || result == 2)
	{
		auto str = ref new String(tz.StandardName);
		return str;
	}

	auto error = GetLastError();
	throw ref new Exception(error, "Win32 Error occurred");
}

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

bool TimeZoneInfoEx::IsDaylightSavingTime(DateTime utcDateTime)
{
	// Convert the input time (as UTC) to a system time
	auto st = DateTimeToSystemTime(&utcDateTime);

	TIME_ZONE_INFORMATION tzi;
	if(GetTimeZoneInformationForYear(st.wYear, &_source, &tzi))
	{
		SYSTEMTIME destDateTime;
		if(SystemTimeToTzSpecificLocalTime(&tzi, &st, &destDateTime))
		{
			// Determine if the specified time in is daylight in the local time zone
			auto daylight = IsDaylightTime(&destDateTime, &tzi);
			return daylight;
		}
	}

	auto error = GetLastError();
	throw ref new Exception(error, "Win32 Error occurred");
}

DateTime TimeZoneInfoEx::ConvertTime(DateTime utcDateTime, TimeSpan* offset)
{
	// Convert the input time (as UTC) to a system time
	auto st = DateTimeToSystemTime(&utcDateTime);
	TIME_ZONE_INFORMATION tzi;

	if(GetTimeZoneInformationForYear(st.wYear, &_source, &tzi))
	{
		SYSTEMTIME destDateTime;
		if(SystemTimeToTzSpecificLocalTime(&tzi, &st, &destDateTime))
		{
			// Determine offset to return as WinRT's date time does not contain offset info			
			auto daylight = IsDaylightTime(&destDateTime, &tzi);

			auto bias = tzi.Bias + tzi.StandardBias;
			
			if(daylight)
				bias += tzi.DaylightBias;

			offset->Duration = SecondsToTicks(-bias);

			// Finally, convert the input time (which is UTC) to the specified TZ's time
			auto dt = SystemTimeToDateTime(&destDateTime);
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


}
}
}