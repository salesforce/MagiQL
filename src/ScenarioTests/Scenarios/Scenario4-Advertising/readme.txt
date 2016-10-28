Account
---------
Id            [int]
Name          [nvarchar]
CurrencyCode  [nvarchar] 

Campaign
----------
Id            [bigint]
AccountId     [int]
Name          [nvarchar]
Budget        [decimal]
IsEnabled     [bit]

AdUnitTest
-----------
Id            [bigint]
CampaignId    [bigint]
Name          [nvarchar]
Budget        [deicmal]
IsEnabled     [bit]

AdUnit
----------
Id            [bigint]
AdUnitTestId  [bigint]
Name          [nvarchar]
Type          [int]
IsEnabled     [bit]

AdCollection/AdUnit
Lifetime/Daily/Hourly
-----------
AdUnitId       [bigint]
AdCollectionId [bigint]
CampaignId     [bigint]
AccountId      [int]
DateUTC        [DateTime]
Impressions    [int]
UniqueImpressions [int]
Clicks         [int]
UniqueClicks   [int]
Spend          [decimal]
FeedbackScore  [decimal]

Calculations
- ClickRate
- CostPerClick
- CostPer 1000 Impressions
- Min budget (AdCollection vs campaign)