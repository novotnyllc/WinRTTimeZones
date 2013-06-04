WinRTTimeZones
==============

Simple Time Zone conversion for WinRT, Windows Store, .NET and Windows Phone 8 apps.

PCL
-
WinRTTimeZones is implemented as a Portable Class Library that can be used at the above frameworks. 

Installation
-
The easiest way to get started is to use the NuGet package. Make sure you have NuGet 2.1 installed first.

Install-Package [WinRTTimeZones](https://github.com/onovotny/WinRTTimeZones)

Usage
-
There's a single entry point for use:

	using TimeZones;	
	public void ConvertTime()
	{
		// Get the time zone we want

		var tz = TimeZoneService.FindSystemTimeZoneById("Central Standard Time");
		var dt = new DateTime(1990, 7, 1, 12, 0, 0, DateTimeKind.Utc);
		
		// This time will be central time
		var local = tz.ConvertTime(dt);
	}
	
The main APIs are in the _TimeZoneService_ class:
	
	/// <summary>
	/// All available time zones
	/// </summary>
	public static IReadOnlyList<string> SystemTimeZoneIds
    
	/// <summary>
    /// Gets a TimeZoneEx by id.
    /// </summary>
    /// <param name="id">Invariant Time Zone name. See SystemTimeZoneIds property for full list.</param>
    /// <returns></returns>
	public static ITimeZoneEx FindSystemTimeZoneById(string id)

    /// <summary>
    /// Converts a DateTimeOffset to one in the specified system time zone
    /// </summary>
	public static DateTimeOffset ConvertTimeBySystemTimeZoneId(DateTimeOffset dateTimeOffset, string destinationTimeZoneId)
    
	/// <summary>
    /// Sets the specified timezone on the date without converting the time
    /// </summary>
	public static DateTimeOffset SpecifyTimeZone(DateTimeOffset dateTimeOffset, ITimeZoneEx timeZone)
	
	/// <summary>
	/// UTC Time Zone
    /// </summary>
    public static ITimeZoneEx Utc

	/// <summary>
    /// Local Time Zone
    /// </summary>
    public static ITimeZoneEx Local

For repeated use, you can get a reference to a specific _ITimeZoneEx_ instance:

	/// <summary>
	/// Represents a time zone
	/// </summary>
    public interface ITimeZoneEx
    {
        /// <summary>
        /// Determines if the current datetime value is in daylight time or not
        /// </summary>
        bool IsDaylightSavingTime(DateTimeOffset dateTimeOffset);

        /// <summary>
        ///  Gets a DateTimeOffset for this time zone
        /// </summary>
        DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset);

        /// <summary>
        /// Normal offset from UTC
        /// </summary>
        TimeSpan BaseUtcOffset { get; }
        
        /// <summary>
        /// Localized name for standard time
        /// </summary>
        string StandardName
        {
            get;
        }
        
        /// <summary>
        /// Localized name for daylight time
        /// </summary>
        string DaylightName { get; }

        /// <summary>
        /// System Id for this time zone
        /// </summary>
        string Id { get; }
    }

Tests
--
The project contains full unit tests.

Support
-
For questions or support, please contact [@onovotny](https://twitter.com/onovotny) on Twitter.
