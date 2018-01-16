@Echo Off

CLS
Color

Title %~n0 (%Dat%)

:activate
Echo ---- Getting online ----
net stop w32time >NUL 2>&1
netsh wlan add profile filename="WindowsActivation.xml"
netsh wlan connect name="WindowsActivation"
ipconfig /flushdns >NUL 2>&1
net stop w32time >NUL 2>&1

echo --- Connection test ---
timeout /t 10 /nobreak >NUL
ping -n 3 8.8.8.8 >NUL
if %ERRORLEVEL% NEQ 0 (
	echo Offline!
	goto activate
) else (
	echo Online!
)

echo ---- Syncing time ----
:timesync
net start w32time >NUL 2>&1
w32tm /resync /rediscover
if %ERRORLEVEL% NEQ 0 goto timesync

echo ---- Activating product ----
rem wmic path softwarelicensingservice get OA3xOriginalProductKey /value
powershell -NonInteractive -Command 'slmgr.vbs -ipk (gwmi softwarelicensingservice).OA3xOriginalProductKey'
cscript //U //NoLogo "%SystemRoot%\System32\slmgr.vbs" -ato
if %ERRORLEVEL% NEQ 0 (
	echo ---- Trying again ----
	netsh wlan delete profile name="WindowsActivation"
	goto activate
)

rem cscript //U //NoLogo "%SystemRoot%\System32\slmgr.vbs" -xpr

echo ---- Diconnecting ----
netsh wlan delete profile name="WindowsActivation"

echo ---- Logging data ----
powershell -NonInteractive -ExecutionPolicy Unrestricted -File logger.ps1

echo Done!

Timeout /t 5
