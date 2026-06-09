; Farmhollow - Windows-Installer (Inno Setup)
[Setup]
AppName=Farmhollow
AppVersion=1.1.5
AppPublisher=Farmhollow
DefaultDirName={autopf}\Farmhollow
DefaultGroupName=Farmhollow
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\Farmhollow.exe
OutputDir=C:\Users\tobia\farmhollow\Build
OutputBaseFilename=FarmhollowSetup-1.1.5
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible

[Languages]
Name: "de"; MessagesFile: "compiler:Languages\German.isl"
Name: "en"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "C:\Users\tobia\farmhollow\Build\Release\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\Farmhollow"; Filename: "{app}\Farmhollow.exe"
Name: "{autodesktop}\Farmhollow"; Filename: "{app}\Farmhollow.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Farmhollow.exe"; Description: "{cm:LaunchProgram,Farmhollow}"; Flags: nowait postinstall skipifsilent
