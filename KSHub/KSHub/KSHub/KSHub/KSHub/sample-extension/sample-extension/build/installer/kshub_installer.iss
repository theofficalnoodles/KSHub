; kshub_installer.iss - template, placeholders replaced by build script
[Setup]
AppName=KSHub
AppVersion=0.1.0
DefaultDirName={pf}\KSHub
DefaultGroupName=KSHub
OutputBaseFilename=KSHub-Setup
Compression=lzma
SolidCompression=yes

[Files]
; All files from the build input folder will be copied by the build script into installer_input
Source: "{{InstallerInput}}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{group}\KSHub"; Filename: "{app}\KSHub.exe"
Name: "{group}\Uninstall KSHub"; Filename: "{uninstallexe}"

[Run]
; Launch after install
Filename: "{app}\KSHub.exe"; Description: "Launch KSHub"; Flags: nowait postinstall skipifsilent
