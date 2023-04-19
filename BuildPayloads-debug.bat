set msbuild="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set buildDir=e:\Share\Projects\C2Sharp\tmpbuild

set agentProj=e:\Share\Projects\C2Sharp\Agent\Agent.csproj
set decoderProj=E:\Share\Projects\C2Sharp\Payload\DecoderDll\DecoderDll.csproj
set patcherProj=E:\Share\Projects\C2Sharp\Payload\PatcherDll\PatcherDll.csproj
set injProj=E:\Share\Projects\C2Sharp\Payload\InjectDll\InjectDll.csproj
set starterProj=E:\Share\Projects\C2Sharp\Payload\Starter\Starter.csproj
set serviceProj=E:\Share\Projects\C2Sharp\Payload\Service\Service.csproj

set destx64Dir=E:\Share\Projects\C2Sharp\Payloads\x64
set scriptDir=E:\Share\Projects\C2Sharp\Payload\Scripts

del %destx64Dir%\* /f /q

echo Scripts 
copy %scriptDir%\*.* %destx64Dir%\ 

%msbuild% %agentproj% /p:configuration=debug /p:platform=AnyCPU /p:outputpath=%builddir%
copy %builddir%\Agent.exe %destx64Dir%\
del %builddir%\* /f /q


echo Patcher
%msbuild% %patcherProj% /p:configuration=debug /p:platform=AnyCPU /p:outputpath=%builddir%
copy %builddir%\Patcher.dll %destx64Dir%\
del %builddir%\* /f /q

echo Inject
%msbuild% %injProj% /p:configuration=debug /p:platform=AnyCPU /p:outputpath=%builddir%
copy %builddir%\Inject.dll %destx64Dir%\
del %builddir%\* /f /q

echo Starter
%msbuild% %starterProj% /p:configuration=debug /p:platform=AnyCPU /p:outputpath=%builddir%
copy %builddir%\Starter.exe %destx64Dir%\
del %builddir%\* /f /q

echo Service

%msbuild% %serviceProj% /p:configuration=debug /p:platform=AnyCPU /p:outputpath=%builddir%
copy %builddir%\Service.exe %destx64Dir%\
del %builddir%\* /f /q




PAUSE