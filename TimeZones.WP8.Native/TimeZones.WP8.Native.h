#pragma once

#include "pch.h"
#include "collection.h"

using namespace Windows;
using namespace Platform;
using namespace Windows::Foundation::Collections;
using namespace Windows::Foundation;
using namespace Platform::Collections;

namespace TimeZones
{
namespace WP8
{
namespace Native
{

    public ref class TimeZoneInfoEx sealed
    {
	private:				
		DYNAMIC_TIME_ZONE_INFORMATION _source;
		
	public:
		static IMap<String^, TimeZoneInfoEx^>^ CreateMap();
		DateTime ConvertTime(DateTime utcDateTime, TimeSpan* offset);
		bool IsDaylightSavingTime(DateTime utcDateTime);		    
		property String^ Name;
		property String^ StandardName;
		property String^ DaylightName;
		property TimeSpan BaseUtcOffset;

	private:
		TimeZoneInfoEx(DYNAMIC_TIME_ZONE_INFORMATION);
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

		// Based on code from http://www.codeguru.com/cpp/cpp/date_time/routines/article.php/c19485/A-Time-Zone-API-supplement.htm

		static int inline dayofweek(int y, int m, int d) /* 0 = Sunday / int y, m, d; / 1 <= m <= 12, y > 1752 or so */ 
		{ 
			static int t[] = {0, 3, 2, 5, 0, 3, 5, 1, 4, 6, 2, 4}; 
			y -= m < 3; 
			if( m > 1 ) --m;
			return (y + y/4 - y/100 + y/400 + t[m%12] + d) % 7; 
		}

		static void inline FindTimezoneDate(const SYSTEMTIME *pEncoded, UINT wYear, SYSTEMTIME *pOut)
		{
			 // check for invalid data
			if( pEncoded == NULL || pOut == NULL ) return;

			SYSTEMTIME	st;

			ZeroMemory(&st, sizeof(SYSTEMTIME));
			 // NULL month?  If so then there is no decode
			if( pEncoded->wMonth != 0 ) 
			{
				st.wMonth = pEncoded->wMonth;
				st.wDay = 1;
				st.wYear = wYear;
				st.wHour = pEncoded->wHour;
				 // Get the Day of Week for the first day of the month
				int wDayOfWeek = dayofweek(st.wYear, st.wMonth, st.wDay);
				 // Get the week offset
				int wWeekOfMonth = pEncoded->wDay;
				int wDay = 1;

				 // First part of the week?
				if( wDayOfWeek <= pEncoded->wDayOfWeek )
				{
					 // Figure out the day of the month
					wDay = 1+((wWeekOfMonth-1)*7+(pEncoded->wDayOfWeek-wDayOfWeek));
				}
				else
				{
					 // Figure out the day of the month
					wDay = 1+(wWeekOfMonth*7-(wDayOfWeek-pEncoded->wDayOfWeek));
				}

				 // Oops, too long
				if( wWeekOfMonth == 5 )
				{
					 // Fix
					while( wDay > 31 ) wDay -= 7;
				}

				 // Fill in misc
				st.wDay = wDay;
				st.wDayOfWeek = pEncoded->wDayOfWeek;
			}
			 // Copy it to the user
			*pOut = st;
		}
				
    };
}
}
}