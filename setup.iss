; RswareDesign Inno Setup Script
; Self-contained .NET 8 WPF application installer with diagnostics tools

#define MyAppName "RswareDesign"
#define MyAppVersion "1.3.0"
#define MyAppPublisher "Rsware"
#define MyAppExeName "RswareDesign.exe"
#define MyAppURL "https://github.com/Dr-Vical/Mock_Material"

[Setup]
AppId={{B8E4F2A1-3C5D-4E6F-A7B8-9D0E1F2A3B4C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=..\setup
OutputBaseFilename=RswareDesign_v{#MyAppVersion}_Setup
SetupIconFile=src\RswareDesign\app.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
korean.DiagTool=진단 도구 (문제 해결용)
english.DiagTool=Diagnostics Tool (Troubleshooting)
korean.DiagDesc=시스템 정보, 시리얼 포트, DLL 무결성 검사 도구를 설치합니다.
english.DiagDesc=Installs system info, serial port, and DLL integrity check tools.
korean.RunDiag=진단 도구 실행 (문제 발생 시 사용)
english.RunDiag=Run Diagnostics Tool (use when issues occur)

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "diagtools"; Description: "{cm:DiagDesc}"; GroupDescription: "{cm:DiagTool}"; Flags: checkedonce

[Files]
; Main application
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Diagnostics tools (only when task selected)
Source: "tools\diagnostics.ps1"; DestDir: "{app}\tools"; Tasks: diagtools; Flags: ignoreversion
Source: "tools\run-diagnostics.bat"; DestDir: "{app}\tools"; Tasks: diagtools; Flags: ignoreversion

[Icons]
; App shortcuts
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

; Diagnostics shortcut
Name: "{group}\{cm:DiagTool}"; Filename: "{app}\tools\run-diagnostics.bat"; Tasks: diagtools; IconFilename: "{sys}\shell32.dll"; IconIndex: 14

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{app}\tools\run-diagnostics.bat"; Description: "{cm:RunDiag}"; Flags: nowait postinstall skipifsilent unchecked shellexec

[Code]
// Display install summary before copying files
function NextButtonClick(CurPageID: Integer): Boolean;
var
  AppSize: Int64;
begin
  Result := True;
end;
