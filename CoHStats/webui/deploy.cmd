del /q ..\bin\x86\Debug\Resources\css\*
del /q ..\bin\x86\Debug\Resources\js\*
del /q ..\bin\x86\Release\Resources\css\*
del /q ..\bin\x86\Release\Resources\js\*
xcopy /e /y dist ..\bin\x86\Debug\Resources\
xcopy /e /y dist ..\bin\x86\Release\Resources\