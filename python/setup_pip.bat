@echo off
cd /d "%~dp0"

:: Create Lib\site-packages if not exists
if not exist "Lib\site-packages" mkdir "Lib\site-packages"

:: Download get-pip.py
echo Downloading get-pip.py...
python.exe -c "import urllib.request; urllib.request.urlretrieve('https://bootstrap.pypa.io/get-pip.py', 'get-pip.py')"

:: Install pip
echo Installing pip...
python.exe get-pip.py --no-warn-script-location

:: Install setuptools and wheel first (required for yukkuri-mandarin)
echo Installing setuptools and wheel...
python.exe -m pip install setuptools wheel --no-warn-script-location

:: Install yukkuri-mandarin
echo.
echo Installing yukkuri-mandarin...
python.exe -m pip install yukkuri-mandarin[jieba] --no-warn-script-location
echo.
echo Done!
pause
