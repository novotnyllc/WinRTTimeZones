#pragma once

#include "pch.h"
#include "collection.h"

using namespace Windows;
using namespace Platform;
using namespace Windows::Foundation::Collections;
using namespace Windows::Foundation;
using namespace Platform::Collections;

namespace TimeZones_WP8_Native
{
    public ref class TimeZoneInfoEx sealed
    {
	private:
		
		TimeZoneInfoEx(DYNAMIC_TIME_ZONE_INFORMATION);
		DYNAMIC_TIME_ZONE_INFORMATION _source;
		//static IMap<String^, TimeZoneInfoEx^>^ _timeZoneData;
		
	public:
		static IMap<String^, TimeZoneInfoEx^>^ CreateMap();
		//static TimeZoneInfoEx^ FindSystemTimeZoneById(String^);
		DateTime ConvertTime(DateTime dateTime, TimeSpan* offset);
		bool IsDaylightSavingTime(DateTime dateTime);
		

    public:
	//	static TimeZoneInfoEx^ FindSystemTimeZoneById(String^);
		//TimeZoneInfoEx();

	property String^ Name;
	property String^ StandardName;
	property String^ DaylightName;
	property TimeSpan BaseUtcOffset;

	private:
	
		static bool IsDaylightTime(const SYSTEMTIME*, const TIME_ZONE_INFORMATION*);


		static long long inline SecondsToTicks(long long seconds)
		{
			return seconds * 60 * 10000000; // ticks per sec;
		}
	
		static SYSTEMTIME inline DateTimeToSystemTime(const DateTime* dateTime)
		{
			ULARGE_INTEGER ulint;
			ulint.QuadPart = dateTime->UniversalTime;
			FILETIME ft = {ulint.LowPart, ulint.HighPart};

			SYSTEMTIME st;
			FileTimeToSystemTime(&ft, &st);
			return st;
		}

		static DateTime inline SystemTimeToDateTime(const SYSTEMTIME* st)
		{
			FILETIME ft;
			SystemTimeToFileTime(st, &ft);

			ULARGE_INTEGER ulint = {ft.dwLowDateTime, ft.dwHighDateTime};

			DateTime dt;
			dt.UniversalTime = ulint.QuadPart;

			return dt;
		}

		static DateTime inline SystemTimeToDateTime(SYSTEMTIME st, short yearOverride)
		{
			FILETIME ft;
			st.wYear = yearOverride;

			SystemTimeToFileTime(&st, &ft);

			ULARGE_INTEGER ulint = {ft.dwLowDateTime, ft.dwHighDateTime};

			DateTime dt;
			dt.UniversalTime = ulint.QuadPart;

			return dt;
		}
				
    };
}