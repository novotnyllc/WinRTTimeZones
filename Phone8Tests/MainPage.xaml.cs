#define DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Phone8Tests.Resources;
using TimeZones;
using TimeZones.Internal;

namespace Phone8Tests
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();

            ConvertTimeToMountanPre2007Test();
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}

        public void ConvertTimeToMountanPre2007Test()
        {
            var dt = new DateTime(2006, 3, 15, 12, 0, 0, DateTimeKind.Utc);

           //  -7 hours
           var local = TimeZoneService.ConvertTimeBySystemTimeZoneId(dt, "Mountain Standard Time");

            var tz = TimeZoneService.FindSystemTimeZoneById("Mountain Standard Time");
        //    Debug.WriteLine(local);

            Debug.WriteLine(tz);
            //local.Hour.Should().Be(5);
            //local.Offset.Should().Be(TimeSpan.FromHours(-7));
        }
    }
}