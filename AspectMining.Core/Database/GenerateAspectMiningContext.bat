set path=C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\;%PATH%
sqlmetal /server:GANGREL /database:AspectMining /user:AspectUser /password:otinanai /views /functions /sprocs  /namespace:AspectMining.Core.Database /context:AspectMiningContext /code:AspectMiningContext.cs /serialization:Unidirectional
pause