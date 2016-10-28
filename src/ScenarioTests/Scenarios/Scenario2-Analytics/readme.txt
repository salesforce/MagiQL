This scenario is a simple 1:*  data scenario
The example being used is a simplified website analytics model
  
sc2.Location
-------------
ID [bigint]
LocationHostID [bigint]
Path [nvarchar(255)]


sc2.LocationHost
-------------
ID [bigint]
Host [nvarchar(255)]


sc2.LocationHit
-------------
ID [bigint]
LocationID [bigint] 
TimeStampUTC [DateTime]
UserID [guid]
StatusCode [int]
ResponseDurationSeconds [decimal]
IsHtml [bit]
ResponseSizeBytes [int]



Calculations

- Html Bytes (if isHtml then responsebytes) 
 
- Day of week? - good for group by
