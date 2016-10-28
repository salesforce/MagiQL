This scenario is a simple flat 1:* data scenario
The example being used is a simplified website analytics model
There are no stats in this example.
  
sc1.Room
-------------
ID [bigint]
HouseID [bigint]
Name [nvarchar(255)]
VolumeCubicMetres [decimal]


sc1.RoomSensor
-------------
RoomID [bigint]
LastUpdated [Datetime]
TemperatureCelcius [int]
HumidityPercent [decimal]
NoiseDecibels [decimal]
IsLight [bit]


Calculations
- House Average Temperature - considering room volume
- House Average Humitity - considering room volume

 





