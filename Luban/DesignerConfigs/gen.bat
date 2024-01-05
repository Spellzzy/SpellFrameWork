set WORKSPACE=..

set LUBAN_DLL=%WORKSPACE%\Tools\Luban\Luban.dll
set CONF_ROOT=.

dotnet %LUBAN_DLL% ^
    -t all ^
    -c cs-simple-json ^
    -d json ^
    --schemaPath %CONF_ROOT%\Defines\__root__.xml ^
    -x inputDataDir=%CONF_ROOT%\Datas  ^
    -x outputCodeDir=../../Assets/Scripts/LubanGen ^
    -x outputDataDir=..\..\AssetBundle\Cache\StreamingAssets\AssetBundle\data

pause