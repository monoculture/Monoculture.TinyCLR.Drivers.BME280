/*
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

using System;

using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Spi;

namespace Monoculture.TinyCLR.Drivers.BME280
{
    internal class BME280BusWrapper
    {
        public BME280BusWrapper(SpiDevice device)
        {
            SpiDevice = device;
        }

        public BME280BusWrapper(I2cDevice device)
        {
            I2CDevice = device;
        }

        public I2cDevice I2CDevice { get; }

        public SpiDevice SpiDevice { get; }

        public BME280BusType BusType => SpiDevice == null ? BME280BusType.I2C : BME280BusType.Spi;

        public void Write(byte[] writeBuffer)
        {
            WriteRead(writeBuffer, 0, writeBuffer.Length, null, 0, 0);
        }

        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            WriteRead(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);
        }

        public void WriteRead( 
            byte[] writeBuffer, 
            int writeOffset,
            int writeLength,
            byte[] readBuffer,
            int readOffset,
            int readLength)
        {
            if (BusType == BME280BusType.I2C)
            {
                I2CDevice.WriteRead(
                    writeBuffer,
                    writeOffset,
                    writeLength,
                    readBuffer,
                    readOffset,
                    readLength);
            }
            else
            {
                var _bufferSize = writeLength + readLength;

                byte[] _readBuffer = new byte[_bufferSize];

                byte[] _writeBuffer = new byte[_bufferSize];

                Array.Copy(writeBuffer, writeOffset, _writeBuffer, 0, writeLength);

                SpiDevice.TransferFullDuplex(_writeBuffer, _readBuffer);

                Array.Copy(_readBuffer, writeLength, readBuffer, readOffset, readLength);
            }
        }
    }

}
