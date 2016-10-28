##Local DB

SqlLocalDB.exe create MagiQL-Tests -s

OR 

SqlLocalDB.exe start MagiQL-Tests

Run Setup Sql Scripts

To allow iis to use localdb add the following to the processmodel entry in applicationHost.config and restart

>loadUserProfile="true" setProfileEnvironment="true"
 
 see https://blogs.msdn.microsoft.com/sqlexpress/2011/12/08/using-localdb-with-full-iis-part-1-user-profile/