/*
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.I2c;

namespace Monoculture.TinyCLR.Drivers.BME280
{
    public class BME280Driver
    {
        private int _tFine;
        private int _rawHumidity;
        private int _rawPressure;
        private int _rawTemperature;
        private readonly I2cDevice _device;
        private BME280CFData _calibration;

        public BME280Driver(I2cDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }

        public static I2cConnectionSettings GetI2CConnectionSettings(BME280Address address)
        {
            var settings = new I2cConnectionSettings((int) address)
            {
                BusSpeed = I2cBusSpeed.FastMode,
                AddressFormat = I2cAddressFormat.SevenBit
            };

            return settings;
        }

        public bool IsInitialized { get; private set; }

        public BME280Filter Filter { get; private set; } = BME280Filter.Off;

        public BME280SensorMode SensorMode { get; private set; } = BME280SensorMode.Normal;

        public BME280OverSample OsrTemperature { get; private set; } = BME280OverSample.X1;

        public BME280OverSample OsrPressure { get; private set; } = BME280OverSample.X1;

        public BME280OverSample OsrHumidity { get; private set; } = BME280OverSample.X1;

        public BME280StandbyTime StandbyDuration { get; private set; } = BME280StandbyTime.Ms05;

        public void Initialize()
        {
            ChipId();

            Reset();

            LoadCalibration();

            WriteSettings();

            IsInitialized = true;
        }

        private void Reset()
        {
            _device.Write(new byte[] { 0xD0, 0xE0 });

            Thread.Sleep(2);
        }

        private void ChipId()
        {
            var buffer = new byte[1];

            _device.WriteRead(new byte[] { 0xD0 }, buffer);

            if (buffer[0] != 0x60)
                throw new ApplicationException("Unrecognized chip");
        }

        private void LoadCalibration()
        {
            var crcBuffer = new byte[1];

            _device.WriteRead(new byte[] { 0xE8 }, crcBuffer);

            var calibrationBuffer = new byte[33];

            _device.WriteRead(new byte[] { 0x88 }, 0, 1, calibrationBuffer, 0, 26);

            _device.WriteRead(new byte[] { 0xE1 }, 0, 1, calibrationBuffer, 26, 7);
                
             _calibration = new BME280CFData
            {
                T1 = BitConverter.ToUInt16(calibrationBuffer, 0),
                T2 = BitConverter.ToInt16(calibrationBuffer, 2),
                T3 = BitConverter.ToInt16(calibrationBuffer, 4),
                P1 = BitConverter.ToUInt16(calibrationBuffer, 6),
                P2 = BitConverter.ToInt16(calibrationBuffer, 8),
                P3 = BitConverter.ToInt16(calibrationBuffer, 10),
                P4 = BitConverter.ToInt16(calibrationBuffer, 12),
                P5 = BitConverter.ToInt16(calibrationBuffer, 14),
                P6 = BitConverter.ToInt16(calibrationBuffer, 16),
                P7 = BitConverter.ToInt16(calibrationBuffer, 18),
                P8 = BitConverter.ToInt16(calibrationBuffer, 20),
                P9 = BitConverter.ToInt16(calibrationBuffer, 22),
                H1 = calibrationBuffer[25],
                H2 = BitConverter.ToInt16(calibrationBuffer, 26),
                H3 = calibrationBuffer[28],
                H4 = (short)((calibrationBuffer[29] << 4) | (calibrationBuffer[30] & 0xF)),
                H5 = (short)((calibrationBuffer[31] << 4) | (calibrationBuffer[30] >> 4)),
                H6 = (sbyte)calibrationBuffer[32]
            };

            if (crcBuffer[0] != CalculateCrc(calibrationBuffer))
                throw new ApplicationException("CRC error loading configuration.");
        }

        private static byte CalculateCrc(byte[] buffer)
        {
            uint crcReg = 0xFF;

            const byte polynomial = 0x1D;

            for (var index = 0; index < buffer.Length; index++)
            {
                for (byte bitNo = 0; bitNo < 8; bitNo++)
                {
                    byte din;

                    if (((crcReg & 0x80) > 0) ^ ((buffer[index] & 0x80) > 0))
                        din = 1;
                    else
                        din = 0;

                    crcReg = (ushort)((crcReg & 0x7F) << 1);

                    buffer[index] = (byte)((buffer[index] & 0x7F) << 1);

                    crcReg = (ushort)(crcReg ^ (polynomial * din));
                }
            }

            return (byte)(crcReg ^ 0xFF);
        }

        public void ChangeSettings(
            BME280SensorMode sensorMode = BME280SensorMode.Normal,
            BME280OverSample osrTemperature = BME280OverSample.X16,
            BME280OverSample osrPressure = BME280OverSample.X16,
            BME280OverSample osrHumidity = BME280OverSample.X16,
            BME280Filter filter = BME280Filter.Off,
            BME280StandbyTime standbyDuration = BME280StandbyTime.Ms05)
        {
            SensorMode = sensorMode;
            OsrPressure = osrPressure;
            OsrHumidity = osrHumidity;
            OsrTemperature = osrTemperature;
            StandbyDuration = standbyDuration;

            WriteSettings();
        }

        private void WriteSettings()
        {
            var humiReg = (byte) OsrHumidity;

            var measReg = (byte)(((byte)OsrTemperature << 5) |
                                 ((byte)OsrPressure << 3) |
                                 (byte)SensorMode);

            var confReg = (byte)((byte)StandbyDuration << 5 | (byte)Filter << 3 | 1);

            _device.Write(new byte[] { 0xF2, humiReg });
            _device.Write(new byte[] { 0xF5, confReg });
            _device.Write(new byte[] { 0xF4, measReg });
        }

        private void TakeForcedReading()
        {
            var measReg = (byte)(((byte)OsrTemperature << 5) |
                                 ((byte)OsrPressure << 3) |
                                 (byte)SensorMode);

            _device.Write(new byte[] { 0xF4, measReg });

            Thread.Sleep(2);
        }

        public void Update()
        {
            if(SensorMode == BME280SensorMode.Forced)
                TakeForcedReading();

            var buffer = new byte[8];

            _device.WriteRead(new byte[] { 0xF7 }, buffer);

            _rawHumidity = buffer[7] | buffer[6] << 8;

            _rawPressure = buffer[0] << 12 | buffer[1] << 4 | buffer[2] >> 4;

            _rawTemperature = buffer[3] << 12 | buffer[4] << 4 | buffer[5] >> 4;

            var var1 = _rawTemperature / 16384.0 - _calibration.T1 / 1024.0;

            var1 = var1 * _calibration.T2;

            var var2 = _rawTemperature / 131072.0 - _calibration.T1 / 8192.0;

            var2 = var2 * var2 * _calibration.T3;

            _tFine = (int)(var1 + var2);
        }

        public double Temperature
        {
            get
            {
                const double temperatureMin = -40;
                const double temperatureMax = 85;

                var x = _tFine / 5120.0;

                if (x < temperatureMin)
                    x = temperatureMin;
                else if (x > temperatureMax)
                    x = temperatureMax;

                return x;
            }
        }

        public float Pressure
        {
            get
            {
                float pressure;

                const float pressureMin = 30000.0f;
                const float pressureMax = 110000.0f;

                var var1 = _tFine / 2.0f - 64_000.0f;

                var var2 = var1 * var1 * _calibration.P6 / 32_768.0f;

                var2 = var2 + var1 * _calibration.P5 * 2.0f;

                var2 = var2 / 4.0f + _calibration.P4 * 65_536.0f;

                var var3 = _calibration.P3 * var1 * var1 / 524_288.0f;

                var1 = (var3 + _calibration.P2 * var1) / 524_288.0f;

                var1 = (1.0f + var1 / 32_768.0f) * _calibration.P1;

                if (var1 > 0)
                {
                    pressure = 1_048_576.0f - _rawPressure;

                    pressure = (pressure - var2 / 4_096.0f) * 6_250.0f / var1;

                    var1 = _calibration.P9 * pressure * pressure / 2_147_483_648.0f;

                    var2 = pressure * _calibration.P8 / 32_768.0f;

                    pressure = pressure + (var1 + var2 + _calibration.P7) / 16.0f;

                    if (pressure < pressureMin)
                        pressure = pressureMin;
                    else if (pressure > pressureMax)
                        pressure = pressureMax;
                }
                else
                {
                    pressure = pressureMin;
                }

                return pressure;
            }
        }

        public float Humidity
        {
            get
            {
                const float humidityMin = 0.0f;
                const float humidityMax = 100.0f;

                var var1 = _tFine - 76800.0f;

                var var2 = _calibration.H4 * 64.0f + _calibration.H5 / 16384.0f * var1;

                var var3 = _rawHumidity - var2;

                var var4 = _calibration.H2 / 65536.0f;

                var var5 = 1.0f + _calibration.H3 / 67108864.0f * var1;

                var var6 = 1.0f + _calibration.H6 / 67108864.0f * var1 * var5;

                var6 = var3 * var4 * (var5 * var6);

                var humidity = var6 * (1.0f - _calibration.H1 * var6 / 524288.0f);

                if (humidity > humidityMax)
                    humidity = humidityMax;
                else if (humidity < humidityMin)
                    humidity = humidityMin;

                return humidity;
            }
        }
    }
}
