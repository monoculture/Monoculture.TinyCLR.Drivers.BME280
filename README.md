
[![Build Status](https://dev.azure.com/deep-water-systems/Monoculture.TinyCLR.Drivers.BME280/_apis/build/status/Monoculture.TinyClr.Devices.BME280-CI?branchName=master)](https://dev.azure.com/deep-water-systems/Monoculture.TinyCLR.Drivers.BME280/_build/latest?definitionId=13?branchName=master)

# Monoculture.TinyCLR.Drivers.BME280

TinyCLR library for the BME280 Humidity, Barometric Pressure + Temp sensor.

# Recommended Modes
   Based on Bosch BME280I2C environmental sensor data sheet.

Weather Monitoring :
   forced mode, 1 sample/minute
   pressure ×1, temperature ×1, humidity ×1, filter off
   Current Consumption =  0.16 μA
   RMS Noise = 3.3 Pa/30 cm, 0.07 %RH
   Data Output Rate 1/60 Hz

Humidity Sensing :
   forced mode, 1 sample/second
   pressure ×0, temperature ×1, humidity ×1, filter off
   Current Consumption = 2.9 μA
   RMS Noise = 0.07 %RH
   Data Output Rate =  1 Hz

Indoor Navigation :
   normal mode, standby time = 0.5ms
   pressure ×16, temperature ×2, humidity ×1, filter = x16
   Current Consumption = 633 μA
   RMS Noise = 0.2 Pa/1.7 cm
   Data Output Rate = 25Hz
   Filter Bandwidth = 0.53 Hz
   Response Time (75%) = 0.9 s

Gaming :
   normal mode, standby time = 0.5ms
   pressure ×4, temperature ×1, humidity ×0, filter = x16
   Current Consumption = 581 μA
   RMS Noise = 0.3 Pa/2.5 cm
   Data Output Rate = 83 Hz
   Filter Bandwidth = 1.75 Hz
   Response Time (75%) = 0.3 s
