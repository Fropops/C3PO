cls

set msbuild="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set buildDir=e:\Share\Projects\C3PO\tmpbuild

set agentProj=e:\Share\Projects\C3PO\Agent\Agent.csproj
set decoderProj=E:\Share\Projects\C3PO\Payload\DecoderDll\DecoderDll.csproj
set patcherProj=E:\Share\Projects\C3PO\Payload\PatcherDll\PatcherDll.csproj
set injectProj=E:\Share\Projects\C3PO\Payload\InjectDll\InjectDll.csproj
set starterProj=E:\Share\Projects\C3PO\Payload\Starter\Starter.csproj
set serviceProj=E:\Share\Projects\C3PO\Payload\Service\Service.csproj

set destx86Dir=E:\Share\Projects\C3PO\Payloads\x86
set destx64Dir=E:\Share\Projects\C3PO\Payloads\x64
set destdebugDir=E:\Share\Projects\C3PO\Payloads\debug
set scriptDir=E:\Share\Projects\C3PO\Payload\Scripts

del %destx86Dir%\* /f /q
del %destx64Dir%\* /f /q
del %destdebugDir%\* /f /q

echo Scripts
copy %scriptDir%\*.* %destx86Dir%\ 
copy %scriptDir%\*.* %destx64Dir%\ 
copy %scriptDir%\*.* %destdebugDir%\ 

echo Agent
%msbuild% %agentproj% /p:configuration=release /p:platform=x86 /p:outputpath=%builddir%
copy %builddir%\Agent.exe %destx86Dir%\
del %builddir%\* /f /q

%msbuild% %agentproj% /p:configuration=release /p:platform=x64 /p:outputpath=%builddir%
copy %builddir%\Agent.exe %destx64Dir%\
del %builddir%\* /f /q

%msbuild% %agentproj% /p:configuration=debug /p:platform=x64 /p:outputpath=%builddir%
copy %builddir%\Agent.exe %destdebugDir%\
del %builddir%\* /f /q

echo Patcher
%msbuild% %patcherProj% /p:configuration=release /p:platform=x86 /p:outputpath=%builddir%
copy %builddir%\Patcher.dll %destx86Dir%\
del %builddir%\* /f /q

%msbuild% %patcherProj% /p:configuration=release /p:platform=x64 /p:outputpath=%builddir%
copy %builddir%\Patcher.dll %destx64Dir%\
del %builddir%\* /f /q

%msbuild% %patcherProj% /p:configuration=debug /p:platform=x64 /p:outputpath=%builddir%
copy %builddir%\Patcher.dll %destdebugDir%\
del %builddir%\* /f /q

echo Inject
%msbuild% %injectProj% /p:configuration=release /p:platform=x86 /p:outputpath=%builddir%
copy %builddir%\Inject.dll %destx86Dir%\
del %builddir%\* /f /q

%msbuild% %injectProj% /p:configuration=release /p:platform=x64 /p:outputpath=%builddir%
copy %builddir%\Inject.dll %destx64Dir%\
del %builddir%\* /f /q

%msbuild% %injectProj% /p:configuration=debug /p:platform=x64 /p:outputpath=%builddir%
copy %builddir%\Inject.dll %destdebugDir%\
del %builddir%\* /f /q


echo Starter
%msbuild% %starterProj% /p:configuration=release /p:platform=x86 /p:outputpath=%builddir%
copy %builddir%\Starter.exe %destx86Dir%\
del %builddir%\* /f /q

%msbuild% %starterProj% /p:configuration=release /p:platform=x64 /p:outputpath=%builddir%
copy %builddir%\Starter.exe %destx64Dir%\
del %builddir%\* /f /q

%msbuild% %starterProj% /p:configuration=debug /p:platform=x64 /p:outputpath=%builddir%
copy %builddir%\Starter.exe %destdebugDir%\
del %builddir%\* /f /q

echo Service
%msbuild% %serviceProj% /p:configuration=release /p:platform=x86 /p:outputpath=%builddir%
copy %builddir%\Service.exe %destx86Dir%\
del %builddir%\* /f /q

%msbuild% %serviceProj% /p:configuration=release /p:platform=x64 /p:outputpath=%builddir%
copy %builddir%\Service.exe %destx64Dir%\
del %builddir%\* /f /q

%msbuild% %serviceProj% /p:configuration=debug /p:platform=x64 /p:outputpath=%builddir%
copy %builddir%\Service.exe %destdebugDir%\
del %builddir%\* /f /q




PAUSE