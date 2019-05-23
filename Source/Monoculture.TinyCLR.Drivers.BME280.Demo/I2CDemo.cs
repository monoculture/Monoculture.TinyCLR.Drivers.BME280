﻿/*
 * Author: Monoculture 2019
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

using System.Threading;
using System.Diagnostics;

using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.I2c;

namespace Monoculture.TinyCLR.Drivers.BME280.Demo
{
    public class I2CDemo
    {
        public static void Main()
        {
            var settings = BME280Driver.GetI2CConnectionSettings(BME280Address.Primary);

            var controller = I2cController.FromName(G120E.I2cBus.I2c0);

            var device = controller.GetDevice(settings);

            var driver = new BME280Driver(device);
                   
            driver.Initialize();

            driver.ChangeSettings(
                BME280SensorMode.Forced,
                BME280OverSample.X1,
                BME280OverSample.X1,
                BME280OverSample.X1,
                BME280Filter.Off);

            while (true)
            {
                driver.Read();

                Debug.WriteLine("Pressure: " + driver.Pressure);
                Debug.WriteLine("Humidity: " + driver.Humidity);
                Debug.WriteLine("Temperature:" + driver.Temperature);

                Thread.Sleep(1000);
            }
        }
    }
}
