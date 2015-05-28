using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Bekker.Adafruit.PCA9685PwmDriver
{
    public class PwmDriver
    {
        public static string I2CControllerName = "I2C1";
        public static byte PwmI2CAddr = 0x40;
        public static int PwmFreq = 50;

        private const byte Pca9685Mode1 = 0x0;
        private const byte Pca9685Prescale = 0xFE;


        private I2cDevice _i2Cpwm;
        private static bool _isInited = false;

        public bool IsDevicedInited => _isInited;

        public PwmDriver(byte i2CAddress = 0x40, int pwmFreq = 50, string controllerName = "I2C1")
        {
            PwmI2CAddr = i2CAddress;
            I2CControllerName = controllerName;
            PwmFreq = pwmFreq;
            InitI2CPwm();
        }

        public void ReInit()
        {
            InitI2CPwm();
        }

        private async void InitI2CPwm()
        {
            var settings = new I2cConnectionSettings(PwmI2CAddr);
            settings.BusSpeed = I2cBusSpeed.FastMode;

            var aqs = I2cDevice.GetDeviceSelector(I2CControllerName);
            var dis = await DeviceInformation.FindAllAsync(aqs);
            _i2Cpwm = await I2cDevice.FromIdAsync(dis[0].Id, settings);
            SetPwmFreq(PwmFreq);
            _isInited = true;
        }

        public void Dispose()
        {
            _i2Cpwm.Dispose();
        }

        private void SetPwmFreq(float freq)
        {
            float prescaleval = 25000000;
            prescaleval /= 4096;
            prescaleval /= freq;
            prescaleval -= 1;
            byte prescale = (byte)System.Math.Floor(prescaleval + 0.5);

            byte oldmode = Read8(Pca9685Mode1);
            byte newmode = (byte)((oldmode & 0x7F) | 0x10); // sleep
            Write8(Pca9685Mode1, newmode); // go to sleep
            Write8(Pca9685Prescale, prescale); // set the prescaler
            Write8(Pca9685Mode1, oldmode);

            Write8(Pca9685Mode1, (byte)(oldmode | 0x80));
        }

        private byte Read8(byte addr)
        {
            var readBuffer = new byte[1];
            _i2Cpwm.WriteRead(new byte[] { addr }, readBuffer);
            return readBuffer[0];
        }

        private void Write8(byte addr, byte d)
        {
            _i2Cpwm.Write(new byte[] { addr, d });
        }
    }
}
