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
                SpiDevice.TransferSequential(
                    writeBuffer, 
                    writeOffset, 
                    writeLength, 
                    readBuffer, 
                    readOffset, 
                    readLength);
            }
        }
    }
}
