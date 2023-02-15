set msbuild="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set buildDir=e:\Share\Projects\C2Sharp\tmpbuild
set agentProj=e:\Share\Projects\C2Sharp\Agent\Agent.csproj
set stageProj=E:\Share\Projects\C2Sharp\Launcher\Stage1\Stage1.csproj
set outPath=e:\Share\Projects\C2Sharp\AgentBuilt
set teamServerSrcDir=E:\Share\Projects\C2Sharp\TeamServer\Source
set CommanderSrcDir=E:\Share\Projects\C2Sharp\Commander\Source

%msbuild% %agentProj% /p:Configuration=ReleaseX86 /p:Platform=x86 /p:OutputPath=%buildDir%
copy %buildDir%\Agent.exe %outPath%\Agent-x86.exe
del %buildDir%\* /F /Q

%msbuild% %agentProj% /p:Configuration=ReleaseX64 /p:Platform=x64 /p:OutputPath=%buildDir%
copy %buildDir%\Agent.exe %outPath%\Agent.exe
del %buildDir%\* /F /Q

%msbuild% %stageProj% /p:Configuration=ReleaseX86 /p:Platform=x86 /p:OutputPath=%buildDir%
copy %buildDir%\Stage1.dll %outPath%\Stage1-x86.dll
del %buildDir%\* /F /Q

%msbuild% %stageProj% /p:Configuration=ReleaseX64 /p:Platform=x64 /p:OutputPath=%buildDir%
copy %buildDir%\Stage1.dll %outPath%\Stage1.dll
del %buildDir%\* /F /Q

%msbuild% %agentProj% /p:Configuration=Release-ServiceX64 /p:Platform=x64 /p:OutputPath=%buildDir%
copy %buildDir%\Agent.exe %outPath%\Agent-Service.exe
del %buildDir%\* /F /Q

copy %outPath%\* %teamServerSrcDir%\

copy %outPath%\* %CommanderSrcDir%\

PAUSE