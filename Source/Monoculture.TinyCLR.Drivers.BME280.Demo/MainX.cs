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

using System.Threading;
using System.Diagnostics;

using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.I2c;

namespace Monoculture.TinyCLR.Drivers.BME280.Demo
{
    public class MainX
    {
        private bool _isStopped;

        private BME280Driver _device;

        private static void Main()
        {
           new MainX().Run();
        }

        public void Run()
        {
            var settings = BME280Driver.GetI2CConnectionSettings(BME280Address.Primary);

            var controller = I2cController.FromName(G120E.I2cBus.I2c0);

            var device = controller.GetDevice(settings);

            _device = new BME280Driver(device);

            _device.Initialize();

            _device.ChangeSettings(
                BME280SensorMode.Forced,
                BME280OverSample.X1,
                BME280OverSample.X1,
                BME280OverSample.X1,
                BME280Filter.Off);

            Loop();
        }

        private void Loop()
        {
            while (!_isStopped)
            {
                _device.Update();

                Debug.WriteLine("Pressure: " + _device.Pressure);
                Debug.WriteLine("Humidity: " + _device.Humidity);
                Debug.WriteLine("Temperature:" + _device.Temperature);

                Thread.Sleep(1000);
            }
        }

        public void Stop()
        {
            _isStopped = true;
        } 
    }
}
