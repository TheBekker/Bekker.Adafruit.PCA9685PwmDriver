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

            _driver = new PwmDriver();
            while(true)
            {
                if(_driver.IsDevicedInited)
                {
                    _driver.MovePercentage(0, 0);
                    System.Threading.Tasks.Task.Delay(2000).Wait();
                    _driver.MovePercentage(0, 95);
                    System.Threading.Tasks.Task.Delay(2000).Wait();
                }
            }
        }

        private void Unload(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
