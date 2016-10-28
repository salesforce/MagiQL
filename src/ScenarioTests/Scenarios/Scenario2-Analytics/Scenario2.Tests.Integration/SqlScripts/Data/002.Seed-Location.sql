TRUNCATE TABLE sc2.location
  
INSERT INTO [sc2].[Location]
           ([LocationHostID]
           ,[Path])
     VALUES
--www.testsite.com
(1,'/'),		   
(1,'/contact'),		   
(1,'/about'),		   
(1,'/login'),		   
(1,'/icon.jpg'),		   

--account.testsite.com
(2,'/'),
(2,'/contact'),	   
(2,'/help'),	   
(2,'/login'),		   
(2,'/logout'),	

--www.my.test
(3,'/'),
(3,'/contact'),
(3,'/image1.jpg'),
(3,'/image2.jpg'),
(3,'/image3.jpg'),

--www.example.com
(4,'/'),
(4,'/about')

