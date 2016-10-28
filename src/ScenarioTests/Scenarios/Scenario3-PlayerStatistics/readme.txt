This scenario is a combined 1:1 and 1:* scenario with individual and lifetime stats
The example being used is a player/match statistics model
  
sc3.Team
-------------
ID [bigint]
Name [nvarchar(255)]

sc3.Player
-------------
ID [bigint]
TeamID [bigint]
Name [nvarchar(255)] 

sc3.PlayerPhysicalAttributes
-------------
PlayerID [bigint] 
HeightCentimetres [int]
WeightKG [double]
IsMale [bit]


sc3.PlayerAchievements
-------------
ID [bigint]
PlayerID [bigint] 
AwardedDate [SmallDateTime]
Description [nvarchar(255)]
PrizeMoney [decimal]


sc3.Match
------------
ID [bigint]
HomeTeamID [bigint]
AwayTeamID [bigint]
KickOffTimeUTC [DateTime]


sc3.PlayerMatchStats
-------------
ID [bigint]
PlayerID [bigint]
StartMinute [int]
EndMinute [int]
Goals [int]
Assists [int]
Fouls [int]
Tackles [int]
YellowCard [bit]
RedCard [bit]
DistanceCoveredKilometres [decimal]
PerformanceRating [decimal]
ManOfTheMatch [bit]


sc3.PlayerCareerStats
-------------
ID [bigint]
PlayerID [bigint]
GamesPlayed [int]
TotalMinutesPlayed [int]
Goals [int]
Assists [int]
Fouls [int]
Tackles [int]
YellowCards [int]
RedCards [int]
ManOfTheMatchCount [int]

Calculations
- Games Played (count)
- Minutes played (avg & total)
- player goals / game
- player minutes / goal 
- distance per tackle







