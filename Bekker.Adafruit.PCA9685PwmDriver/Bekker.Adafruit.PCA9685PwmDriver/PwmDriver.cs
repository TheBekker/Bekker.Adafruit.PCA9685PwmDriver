using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Bekker.Adafruit.PCA9685PwmDriver
{
    public class PwmDriver
    {
        private const byte RegMode1 = 0x0;
        private const byte RegPrescale = 0xFE;
        private const double ClockFrequency = 25000000;
        private const byte Servo0OnL = 0x6;
        private const byte Servo0OnH = 0x7;
        private const byte Servo0OffL = 0x8;
        private const byte Servo0OffH = 0x9;
        private const int I2CResetAddress = 0x0;
        private const ushort PulseResolution = 4096;

        public static string I2CControllerName = "I2C1";
        public static byte PwmI2CAddr = 0x40;
        public static int PwmFreq = 60;
        private static readonly byte[] ResetCommand = { 0x06 };
        private static readonly uint MinPulse = 150;
        private static readonly uint MaxPulse = 600;
        private static bool _isInited;
        private I2cDevice _primaryDevice;
        private I2cDevice _resetDevice;

        public PwmDriver(byte i2CAddress = 0x40, int pwmFreq = 60, string controllerName = "I2C1")
        {
            PwmI2CAddr = i2CAddress;
            I2CControllerName = controllerName;
            PwmFreq = pwmFreq;
            UISafeWait(EnsureInitializedAsync);
        }

        public bool IsDevicedInited => _isInited;

        public void DrivePercentage(byte servo, double percentage)
        {
            if (percentage > 1.0)
            {
                percentage = 1.0;
            }
            if (percentage < 0)
            {
                percentage = 0;
            }
            var intPercentage = (int)(percentage*100);
            var pulse = Map(intPercentage, 0, 100, MinPulse, MaxPulse);
            if (intPercentage == 0)
            {
                pulse = 0;
            }
            Pulse(servo, 0, (int)pulse);
        }

        public void Pulse(byte num, int on, int off)
        {
            if (!_isInited)
            {
                return;
            }
            Write8(RegMode1, 0x0);
            Write8((byte)(Servo0OnL + 4 * num), (byte)on);
            Write8((byte)(Servo0OnH + 4 * num), (byte)(on >> 8));
            Write8((byte)(Servo0OffL + 4 * num), (byte)off);
            Write8((byte)(Servo0OffH + 4 * num), (byte)(off >> 8));
        }

        private async Task EnsureInitializedAsync()
        {
            // If already initialized, done
            if (_isInited)
            {
                return;
            }

            // Validate
            if (string.IsNullOrWhiteSpace(I2CControllerName))
            {
                throw new Exception("Controller name not set");
            }

            // Get a query for I2C
            var aqs = I2cDevice.GetDeviceSelector(I2CControllerName);

            // Find the first I2C device
            var di = (await DeviceInformation.FindAllAsync(aqs)).FirstOrDefault();

            // Make sure we found an I2C device
            if (di == null)
            {
                throw new Exception("Device Info null: " + I2CControllerName);
            }

            // Connection settings for primary device
            var primarySettings = new I2cConnectionSettings(PwmI2CAddr)
            {
                BusSpeed = I2cBusSpeed.FastMode,
                SharingMode = I2cSharingMode.Exclusive
            };

            // Get the primary device
            _primaryDevice = await I2cDevice.FromIdAsync(di.Id, primarySettings);
            if (_primaryDevice == null)
            {
                throw new Exception("PCA9685 primary device not found");
            }


            // Connection settings for reset device
            var resetSettings = new I2cConnectionSettings(PwmI2CAddr);
            resetSettings.SlaveAddress = I2CResetAddress;

            // Get the reset device
            _resetDevice = await I2cDevice.FromIdAsync(di.Id, resetSettings);
            if (_resetDevice == null)
            {
                throw new Exception("PCA9685 reset device not found");
            }

            // Initialize the controller
            await InitializeControllerAsync();


            // Done initializing
            _isInited = true;
        }

        private async Task InitializeControllerAsync()
        {
            if (_primaryDevice == null) return;

            ResetController();
            var prescaleval = ClockFrequency;
            prescaleval /= PulseResolution;
            prescaleval /= PwmFreq;
            prescaleval -= 1;
            var prescale = (byte)Math.Floor(prescaleval + 0.5);
            Write8(RegPrescale, prescale);

            await RestartControllerAsync(0xA1);
        }

        private void ResetController()
        {
            _resetDevice.Write(ResetCommand);
        }

        private async Task RestartControllerAsync(byte mode1)
        {
            Write8(RegMode1, mode1);

            // Wait for more than 500us to stabilize.  	
            await Task.Delay(1);
        }

        private byte Read8(byte addr)
        {
            var readBuffer = new byte[1];
            _primaryDevice.WriteRead(new[] { addr }, readBuffer);
            return readBuffer[0];
        }

        private void Write8(byte addr, byte d)
        {
            _primaryDevice.Write(new[] { addr, d });
        }

        private long Map(long x, long in_min, long in_max, long out_min, long out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        public static void UISafeWait(Func<Task> taskFunction)
        {
            Task.Run(async () => { await taskFunction().ConfigureAwait(false); }).Wait();
        }
    }
}