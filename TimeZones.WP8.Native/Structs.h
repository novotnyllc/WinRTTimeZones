#pragma once

#include "pch.h"
#include "collection.h"

using namespace Windows;
using namespace Platform;
using namespace Windows::Foundation::Collections;
using namespace Platform::Collections;

namespace TimeZones_WP8_Native
{
public ref struct SYSTEMTIME_WRAPPED sealed
	{
	public:
          property short Year;
          property short Month;
          property short DayOfWeek;
          property short Day;
          property short Hour;
          property short Minute;
          property short Second;
          property short Milliseconds;	
	};




	public ref struct DYNAMIC_TIME_ZONE_INFORMATION_WRAPPED sealed
	{
	public: 
		
            property int Bias;
            property String^ StandardName;
            property SYSTEMTIME_WRAPPED^ StandardDate;
            property int StandardBias;
            property String^ DaylightName;
            property SYSTEMTIME_WRAPPED^ DaylightDate;
           property  int DaylightBias;           
           property  String^ TimeZoneKeyName;
            property bool DynamicDaylightTimeDisabled;	
	};

	public ref struct TIME_ZONE_INFORMATION_WRAPPED sealed
	{
	public: 
		
            property int Bias;
            property String^ StandardName;
            property SYSTEMTIME_WRAPPED^ StandardDate;
            property int StandardBias;
            property String^ DaylightName;
            property SYSTEMTIME_WRAPPED^ DaylightDate;
            property int DaylightBias; 
	};


	public ref struct CountAndTimeZone sealed
{
public:

	property DYNAMIC_TIME_ZONE_INFORMATION_WRAPPED^ TZ;
	property int count;
};

}