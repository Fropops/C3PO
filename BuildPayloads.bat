set msbuild="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set buildDir=e:\Share\Projects\C2Sharp\tmpbuild

set agentProj=e:\Share\Projects\C2Sharp\Agent\Agent.csproj
set decoderProj=E:\Share\Projects\C2Sharp\Payload\DecoderDll\DecoderDll.csproj
set patcherProj=E:\Share\Projects\C2Sharp\Payload\PatcherDll\PatcherDll.csproj
set starterProj=E:\Share\Projects\C2Sharp\Payload\Starter\Starter.csproj
set serviceProj=E:\Share\Projects\C2Sharp\Payload\Service\Service.csproj

set destx86Dir=E:\Share\Projects\C2Sharp\Payloads\x86
set destx64Dir=E:\Share\Projects\C2Sharp\Payloads\x64
set scriptDir=E:\Share\Projects\C2Sharp\Payload\Scripts

echo Scripts
copy %scriptDir%\*.* %destx86Dir%\ 
copy %scriptDir%\*.* %destx64Dir%\ 

echo Agent
%msbuild% %agentproj% /p:configuration=releasex86 /p:platform=x86 /p:outputpath=%builddir%
copy %builddir%\Agent.exe %destx86Dir%\
del %builddir%\* /f /q

%msbuild% %agentproj% /p:configuration=releasex64 /p:platform=x64 /p:outputpath=%builddir%
copy %builddir%\Agent.exe %destx64Dir%\
del %builddir%\* /f /q


echo Patcher
%msbuild% %patcherProj% /p:configuration=releasex86 /p:platform=AnyCPU /p:outputpath=%builddir%
copy %builddir%\Patcher.dll %destx86Dir%\
del %builddir%\* /f /q

%msbuild% %patcherProj% /p:configuration=releasex64 /p:platform=AnyCPU /p:outputpath=%builddir%
copy %builddir%\Patcher.dll %destx64Dir%\
del %builddir%\* /f /q

echo Starter
%msbuild% %starterProj% /p:configuration=releasex86 /p:platform=AnyCPU /p:outputpath=%builddir%
copy %builddir%\Starter.exe %destx86Dir%\
del %builddir%\* /f /q

%msbuild% %starterProj% /p:configuration=releasex64 /p:platform=AnyCPU /p:outputpath=%builddir%
copy %builddir%\Starter.exe %destx64Dir%\
del %builddir%\* /f /q

echo Service
%msbuild% %serviceProj% /p:configuration=releasex86 /p:platform=AnyCPU /p:outputpath=%builddir%
copy %builddir%\Service.exe %destx86Dir%\
del %builddir%\* /f /q

%msbuild% %serviceProj% /p:configuration=releasex64 /p:platform=AnyCPU /p:outputpath=%builddir%
copy %builddir%\Service.exe %destx64Dir%\
del %builddir%\* /f /q




PAUSE