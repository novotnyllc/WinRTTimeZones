#pragma once

#include "pch.h"
#include "collection.h"
#include "Structs.h"

using namespace Windows;
using namespace Platform;
using namespace Windows::Foundation::Collections;
using namespace Platform::Collections;

namespace TimeZones_WP8_Native
{

	public value struct Foo
	{
		int bar;
	};

    public ref class TimeZoneInfoMethods sealed
    {
	
	private:
		static void FillSystemTimeWrapped(SYSTEMTIME_WRAPPED^ wrapped, SYSTEMTIME& source)
		{
			//wrapped = ref new SYSTEMTIME_WRAPPED();

			wrapped->Day = source.wDay;
			wrapped->DayOfWeek = source.wDayOfWeek;
			wrapped->Hour = source.wHour;
			wrapped->Milliseconds = source.wMilliseconds;
			wrapped->Minute = source.wMinute;
			wrapped->Month = source.wMonth;
			wrapped->Second = source.wSecond;
			wrapped->Year = source.wYear;
		}

		static void FillDynamicInfoWrapped(DYNAMIC_TIME_ZONE_INFORMATION_WRAPPED^ wrapped, DYNAMIC_TIME_ZONE_INFORMATION* source)
		{
			wrapped->Bias = source->Bias;
			wrapped->DaylightBias = source->DaylightBias;
			wrapped->DaylightDate = ref new SYSTEMTIME_WRAPPED();
			FillSystemTimeWrapped(wrapped->DaylightDate, source->DaylightDate);
			wrapped->DaylightName = ref new String(source->DaylightName);
			wrapped->DynamicDaylightTimeDisabled = source->DynamicDaylightTimeDisabled != 0;
			wrapped->StandardBias = source->StandardBias;
			wrapped->StandardDate = ref new SYSTEMTIME_WRAPPED();
			FillSystemTimeWrapped(wrapped->StandardDate, source->StandardDate);
			wrapped->StandardName = ref new String(source->StandardName);
			wrapped->TimeZoneKeyName = ref new String(source->TimeZoneKeyName);
		}

		static void FillTimeZoneInfoWrapped(TIME_ZONE_INFORMATION_WRAPPED^ wrapped, TIME_ZONE_INFORMATION* source)
		{
			wrapped->Bias = source->Bias;
			wrapped->DaylightBias = source->DaylightBias;
			wrapped->DaylightDate = ref new SYSTEMTIME_WRAPPED();
			FillSystemTimeWrapped(wrapped->DaylightDate, source->DaylightDate);
			wrapped->DaylightName = ref new String(source->DaylightName);
			wrapped->StandardBias = source->StandardBias;
			wrapped->StandardDate = ref new SYSTEMTIME_WRAPPED();
			FillSystemTimeWrapped(wrapped->StandardDate, source->StandardDate);
			wrapped->StandardName = ref new String(source->StandardName);
		}

		static TIME_ZONE_INFORMATION FromTimeZoneInfoWrapped(TIME_ZONE_INFORMATION_WRAPPED^& wrapped)
		{		

			TIME_ZONE_INFORMATION info;
			ZeroMemory(&info, sizeof(info));

			info.Bias = wrapped->Bias;
			info.DaylightBias = wrapped->DaylightBias;
			info.DaylightDate = FromSystemTimeWrapped(wrapped->DaylightDate);
			CopyStringData(info.DaylightName, wrapped->DaylightName, 32);
			info.StandardBias = wrapped->StandardBias;
			info.StandardDate = FromSystemTimeWrapped(wrapped->StandardDate);
			CopyStringData(info.StandardName, wrapped->StandardName, 32);

			return info;
		}

		static DYNAMIC_TIME_ZONE_INFORMATION FromDynamicTimeZoneInfoWrapped(DYNAMIC_TIME_ZONE_INFORMATION_WRAPPED^& wrapped)
		{		

			DYNAMIC_TIME_ZONE_INFORMATION info;// = new DYNAMIC_TIME_ZONE_INFORMATION();
			ZeroMemory(&info, sizeof(info));

			info.Bias = wrapped->Bias;
			info.DaylightBias = wrapped->DaylightBias;
			info.DaylightDate = FromSystemTimeWrapped(wrapped->DaylightDate);
			CopyStringData(info.DaylightName, wrapped->DaylightName, 32);
			info.StandardBias = wrapped->StandardBias;
			info.StandardDate = FromSystemTimeWrapped(wrapped->StandardDate);
			CopyStringData(info.StandardName, wrapped->StandardName, 32);
			CopyStringData(info.TimeZoneKeyName, wrapped->TimeZoneKeyName, 128);
			info.DynamicDaylightTimeDisabled = wrapped->DynamicDaylightTimeDisabled;

			return info;
		}

		static void CopyStringData(WCHAR* data, String^ str, int maxCount)
		{
			auto start = str->Data();
			auto wstr = std::wstring(start);

			
			for(unsigned int i = 0; i < wstr.length(); i++)
			{
				data[i] = wstr.at(i);
			}
		}

		static SYSTEMTIME FromSystemTimeWrapped(SYSTEMTIME_WRAPPED^ wrapped)
		{
			SYSTEMTIME st;
			ZeroMemory(&st, sizeof(st));
			st.wDay = wrapped->Day;
			st.wDayOfWeek = wrapped->DayOfWeek;
			st.wHour = wrapped->Hour;
			st.wMilliseconds = wrapped->Milliseconds;			
			st.wMonth = wrapped->Month;
			st.wMinute = wrapped->Minute;
			st.wSecond = wrapped->Second;
			st.wYear = wrapped->Year;

			return st;
		}

	public:
		static CountAndTimeZone^ EnumDynamicTimeZoneInformationWrapped(int count)
		{			
			DYNAMIC_TIME_ZONE_INFORMATION tzi;
			ZeroMemory(&tzi, sizeof(tzi));
			auto result = EnumDynamicTimeZoneInformation(count, &tzi);

			auto dtzi = ref new DYNAMIC_TIME_ZONE_INFORMATION_WRAPPED();

			FillDynamicInfoWrapped(dtzi, &tzi);

			auto res = ref new CountAndTimeZone();
			res->count = result;
			res->TZ = dtzi;
			return res;			
		}

		static SYSTEMTIME_WRAPPED^ SystemTimeToTzSpecificLocalTimeWrapped(_In_ TIME_ZONE_INFORMATION_WRAPPED^ tzi, _In_ SYSTEMTIME_WRAPPED^ universalTime)
		{
			SYSTEMTIME local;
			ZeroMemory(&local, sizeof(local));

			auto utzi = FromTimeZoneInfoWrapped(tzi);
			auto ust = FromSystemTimeWrapped(universalTime);

			auto result = SystemTimeToTzSpecificLocalTime(&utzi, &ust, &local);

			//return result < 0;
			SYSTEMTIME_WRAPPED^ res = ref new SYSTEMTIME_WRAPPED();
		//	FillSystemTimeWrapped(res, &local);

			return res;
		}

	
		static TIME_ZONE_INFORMATION_WRAPPED^ GetTimeZoneInformationForYearWrapped(_In_ short wYear, _In_ DYNAMIC_TIME_ZONE_INFORMATION_WRAPPED^ pdtzi)
		{
			TIME_ZONE_INFORMATION info;
			ZeroMemory(&info, sizeof(info));

			auto updtzi = FromDynamicTimeZoneInfoWrapped(pdtzi);
			auto result = GetTimeZoneInformationForYear(wYear, &updtzi, &info);


			TIME_ZONE_INFORMATION_WRAPPED^ wrapped = ref new TIME_ZONE_INFORMATION_WRAPPED();
			//ZeroMemory(&wrapped, sizeof(wrapped));

			FillTimeZoneInfoWrapped(wrapped, &info);
			return  wrapped;
		}
				
    };
}