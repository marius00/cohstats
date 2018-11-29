#define ApplicationVersion GetFileVersion('..\CoHStats\bin\x86\release\CoHStats.exe')

[Setup]
AppVerName=CoH Stats
AppName=CoH Stats (c) EvilSoft
VersionInfoVersion={#ApplicationVersion}
AppId=cohstats
DefaultDirName={code:DefDirRoot}\CoH Stats
Uninstallable=Yes
OutputDir=..\Installer
SetupIconFile=..\CoHStats\icon.ico


[Tasks]
Name: desktopicon; Description: "Create a &desktop icon"; GroupDescription: "Icons:"
Name: starticon; Description: "Create a &startmenu icon"; GroupDescription: "Icons:"


[Icons]
Name: "{commonprograms}\CoH Stats"; Filename: "{app}\\CoHStats.exe"; Tasks: starticon
Name: "{commondesktop}\CoH Stats"; Filename: "{app}\\CoHStats.exe"; Tasks: desktopicon


[Files]
Source: "..\CoHStats\bin\x86\Release\*"; Excludes: "*.pdb, *.exe.config"; DestDir: "{app}"; Flags: overwritereadonly recursesubdirs createallsubdirs touch ignoreversion

[Setup]
UseSetupLdr=yes
DisableProgramGroupPage=yes
DiskSpanning=no
AppVersion={#ApplicationVersion}
PrivilegesRequired=admin
DisableWelcomePage=Yes
AlwaysShowDirOnReadyPage=Yes
DisableDirPage=No
OutputBaseFilename=CoHStats-{#ApplicationVersion}
LicenseFile=license.txt

[UninstallDelete]
Type: filesandordirs; Name: {app}

[Languages]
Name: eng; MessagesFile: compiler:Default.isl

[Code]
function IsRegularUser(): Boolean;
begin
Result := not (IsAdminLoggedOn or IsPowerUserLoggedOn);
end;

function DefDirRoot(Param: String): String;
begin
if IsRegularUser then
Result := ExpandConstant('{localappdata}')
else
Result := ExpandConstant('{pf}')
end;

