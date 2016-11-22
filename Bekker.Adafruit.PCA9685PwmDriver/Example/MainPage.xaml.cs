using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Bekker.Adafruit.PCA9685PwmDriver;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Example
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private PwmDriver _driver;
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += Test;
            
        }

        private async void Test(object sender, RoutedEventArgs e)
        {
            _driver = await PwmDriver.Init();
            while (true)
            {
                if (_driver.IsDevicedInited)
                {
                    _driver.DrivePercentage(0, 0);
                    System.Threading.Tasks.Task.Delay(2000).Wait();
                    _driver.DrivePercentage(0, 100);
                }
            }
        }
       

        private void Unload(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
