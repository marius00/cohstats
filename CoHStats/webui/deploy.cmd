del /q ..\bin\x86\Debug\Resources\static\css\*
del /q ..\bin\x86\Debug\Resources\static\js\*
del /q ..\bin\x86\Release\Resources\static\css\*
del /q ..\bin\x86\Release\Resources\static\js\*
xcopy /e /y build ..\bin\x86\Debug\Resources\
xcopy /e /y build ..\bin\x86\Release\Resources\