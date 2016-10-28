TRUNCATE TABLE sc2.locationhit
 
INSERT INTO [sc2].[LocationHit]
           ([LocationID] ,[TimeStampUTC] ,[UserID] ,[StatusCode], [ResponseDurationSeconds], [IsHtml], [ResponseSizeBytes])
     VALUES 
	 	  
--www.testsite.com
--(1,'/'),		   
(1 , '2000-01-01 12:00' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 200, 0.234, 1, 20000),
(1 , '2000-01-01 12:05' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 200, 0.243, 1, 20000),
(1 , '2000-01-02 14:55' ,'7909065c-e0d8-45c0-99a8-cc4014524512' , 200, 0.290, 1, 20032),
--(2,'/contact'),		   
(2 , '2000-01-01 12:11' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 200, 0.703, 1, 19300),
(2 , '2000-01-01 12:12' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 200, 0.703, 1, 19300),
(2 , '2000-01-02 12:12' ,'7909065c-e0d8-45c0-99a8-cc4014524512' , 200, 0.703, 1, 19300),
--(3,'/about'),		   
(3 , '2000-01-02 12:15' ,'7909065c-e0d8-45c0-99a8-cc4014524512' , 200, 1.203, 1, 79600),
--(4,'/login'),		   
(4 , '2000-01-03 19:38' ,'2a6e7111-f4ce-4262-98b8-b7fa6877adf0' , 200, 1.840, 1, 80600),
--(5,'/icon.jpg'),		   
(5 , '2000-01-01 12:00' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 200, 0.109, 0, 5763),
(5 , '2000-01-01 12:05' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 200, 0.103, 0, 5763),
(5 , '2000-01-01 12:11' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 404, 0.109, 0, 5763),
(5 , '2000-01-01 12:12' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 200, 0.112, 0, 5763),
(5 , '2000-01-02 12:12' ,'7909065c-e0d8-45c0-99a8-cc4014524512' , 200, 0.102, 0, 5763),
(5 , '2000-01-02 12:15' ,'7909065c-e0d8-45c0-99a8-cc4014524512' , 200, 0.119, 0, 5763),
(5 , '2000-01-02 14:55' ,'7909065c-e0d8-45c0-99a8-cc4014524512' , 200, 0.107, 0, 5763),
(5 , '2000-01-03 19:38' ,'2a6e7111-f4ce-4262-98b8-b7fa6877adf0' , 404, 0.109, 0, 5763),

--account.testsite.com
--(6,'/'),
(6 , '2000-01-01 18:22' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 200, 0.287, 1, 29800),
(6 , '2000-01-01 19:57' ,'2a6e7111-f4ce-4262-98b8-b7fa6877adf0' , 200, 1.986, 1, 29800),
(6 , '2000-01-03 11:07' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 200, 0.765, 1, 29899),
(6 , '2000-01-04 08:32' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 200, 0.563, 1, 29899),
--(7,'/contact'),	   
(7 , '2000-01-01 18:35' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 404, 0.187, 1, 2000),
(7 , '2000-01-01 18:36' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 404, 0.187, 1, 2000),
(7 , '2000-01-01 22:12' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 404, 0.187, 1, 2000), 
--(8,'/help'),	   
(8 , '2000-01-02 19:39' ,'2a6e7111-f4ce-4262-98b8-b7fa6877adf0' , 200, 0.779, 1, 15009),
(8 , '2000-01-03 22:09' ,'7909065c-e0d8-45c0-99a8-cc4014524512' , 200, 1.006, 1, 15009),
--(9,'/login'),		   
(9 , '2000-01-03 10:11' ,'2a6e7111-f4ce-4262-98b8-b7fa6877adf0' , 200, 0.654, 1, 12987),
(9 , '2000-01-04 16:44' ,'7909065c-e0d8-45c0-99a8-cc4014524512' , 200, 0.876, 1, 12986),
--(10,'/logout'),	
-- none


--www.my.test
--(11,'/'),
(11 , '2000-01-01 05:12' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 200, 1.009, 1, 65200),
(11 , '2000-01-02 22:47' ,'4d726029-dba2-426e-849e-cac40f6af685' , 200, 1.768, 1, 66876),
(11 , '2000-01-02 22:59' ,'4d726029-dba2-426e-849e-cac40f6af685' , 200, 1.465, 1, 62623),
(11 , '2000-01-04 17:01' ,'f36ad5f5-e7ce-4acd-a6c2-c5471171bc3c' , 200, 1.223, 1, 76009),
--(12,'/contact'),
(12 , '2000-01-01 05:12' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 404, 1.009, 1, 65200), 
--(13,'/image1.jpg'),
(13 , '2000-01-02 16:11' ,'7909065c-e0d8-45c0-99a8-cc4014524512' , 200, 0.120, 0, 15873),
(13 , '2000-01-01 14:15' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 404, 0.119, 0, 15873),
(13 , '2000-01-01 15:32' ,'a32e7adc-3dd6-4cf5-b14b-397d7ef4a87c' , 200, 0.277, 0, 15873),
(13 , '2000-01-02 10:55' ,'7909065c-e0d8-45c0-99a8-cc4014524512' , 200, 0.273, 0, 15873),
(13 , '2000-01-02 18:07' ,'7909065c-e0d8-45c0-99a8-cc4014524512' , 200, 0.432, 0, 15873),
(13 , '2000-01-03 23:31' ,'2a6e7111-f4ce-4262-98b8-b7fa6877adf0' , 200, 0.082, 0, 15873),
--(14,'/image2.jpg'),
--(15,'/image3.jpg'),

--www.example.com
--(16,'/'),
--(17,'/about')
(17 , '2000-01-05 20:21' ,'2a6e7111-f4ce-4262-98b8-b7fa6877adf0' , 200, 0.080, 1, 543)

