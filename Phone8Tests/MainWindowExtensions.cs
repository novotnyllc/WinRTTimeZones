using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Client;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace Phone8Tests
{
    public static class MainWindowExtensions
    {
        /// <summary>
        /// Call this method from the Loaded event in MainPage
        /// </summary>
        /// <param name="testProvider">Optional test provider implementation. If omitted the default MsTest provider will be used</param>
        public static void StartTestRunner(this PhoneApplicationPage mainPage, IUnitTestProvider testProvider = null)
        {
            SystemTray.IsVisible = false;

            if (testProvider != null)
            {
                UnitTestSystem.RegisterUnitTestProvider(testProvider);
            }

          //  var testPage = (IMobileTestPage)UnitTestSystem.CreateTestPage(settings);;
            var testPage = UnitTestSystem.CreateTestPage();
         //   mainPage.BackKeyPress += (x, xe) => xe.Cancel = testPage.NavigateBack();
            ((PhoneApplicationFrame)Application.Current.RootVisual).Content = testPage;
        }
    }
}