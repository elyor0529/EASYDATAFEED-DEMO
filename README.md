# EDF 

1. Solution stucture:

	1.1. "WheelsScraper 2" - contains installation of EDF. do not edit anything in this folder. EDF will be autoupdated. so it will be exactly the same as all users have
	
	1.2. "VS Template" - Visual Studio template EDF module. copy "EDF Module.zip" to "c:\Users\_USERNAME_\Documents\Visual Studio 2013\Templates\ProjectTemplates\EDF Module.zip" 
	
	1.3. "EDF dev" - dummy application to start EDF with debugging. 
	
	1.4. "EDF Modules" - the place for all your custom modules. 

2. Creating new module.
	
	2.1. make step 1.2 (only once)
	
	2.2. Open "EDF dev.sln"

	2.3. File->New->Project->EDF Module. Location MUS BE "EDF Modules"

	2.4. Copy .edf file from "your new module\edf" folder to "WheelsScraper 2\WheelsScraper\bin\Debug"

3. Debugging

	3.1. Your new template EDF module is ready. Just press Debug->Start Debugging. EDF will be started, and you can debug your module

4. Other
	
	4.1. "EDF Dev" app is setup to start EasyDataFeed.exe when starting debugging, do not change this

	4.2. When you build your module, all output will go to "WheelsScraper 2\WheelsScraper\bin\Debug". do not change this. so when 
	EDF starts it has latest version of your module
