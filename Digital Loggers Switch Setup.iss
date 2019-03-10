;
; Script generated by the ASCOM Driver Installer Script Generator 6.0.0.0
; Generated by Chris Rowland on 14/06/2014 (UTC)
;
[Setup]
AppID={{ffeb9085-1492-4929-8ba9-48ae9dc44873}
AppName=ASCOM Digital Loggers Web Power Switch Driver
AppVerName=ASCOM Digital Loggers Web Power Switch Driver 6.0.2
AppVersion=6.0.2.0
AppPublisher=Manoj Koushik
AppPublisherURL=mailto:manoj.koushikk@gmail.com
AppSupportURL=http://tech.groups.yahoo.com/group/ASCOM-Talk/
AppUpdatesURL=http://ascom-standards.org/
VersionInfoVersion=1.0.0.0
MinVersion=0,6.1
DefaultDirName="{cf}\ASCOM\Switch"
DisableDirPage=yes
DisableProgramGroupPage=yes
OutputDir="."
OutputBaseFilename="ASCOM Digital Loggers Web Power Switch Setup"
Compression=lzma
SolidCompression=yes
; Put there by Platform if Driver Installer Support selected
WizardImageFile="C:\Program Files (x86)\ASCOM\Platform 6 Developer Components\Installer Generator\Resources\WizardImage.bmp"
LicenseFile="C:\Program Files (x86)\ASCOM\Platform 6 Developer Components\Installer Generator\Resources\CreativeCommons.txt"
; {cf}\ASCOM\Uninstall\Switch folder created by Platform, always
UninstallFilesDir="{cf}\ASCOM\Uninstall\Switch\Digital Loggers Switch"

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: "{cf}\ASCOM\Uninstall\Switch\Digital Loggers Switch"
; TODO: Add subfolders below {app} as needed (e.g. Name: "{app}\MyFolder")

[Files]
Source: "bin\Release\ASCOM.DigitalLoggers.Switch.dll"; DestDir: "{app}"
; Require a read-me HTML to appear after installation, maybe driver's Help doc
Source: "AscomDigitalLoggers.htm"; DestDir: {app}; Flags: isreadme; 
Source: "ACPTiddler.mov"; DestDir: {app}; 
; TODO: Add other files needed by your driver here (add subfolders above)
Source: "digitalloggerssetup.vbs"; DestDir: {app}
Source: "ACP\digitalloggers.tiddler"; DestDir: "{app}\ACP\"
Source: "ACP\digitalloggers.asp"; DestDir: "{app}\ACP\"



; Only if driver is .NET
[Run]
; Only for .NET assembly/in-proc drivers
Filename: "{dotnet4032}\regasm.exe"; Parameters: "/codebase ""{app}\ASCOM.DigitalLoggers.Switch.dll"""; Flags: runhidden 32bit
Filename: "{dotnet4064}\regasm.exe"; Parameters: "/codebase ""{app}\ASCOM.DigitalLoggers.Switch.dll"""; Flags: runhidden 64bit; Check: IsWin64
; The following will call visual basic script which will initiallize the Web Power switch after installation
Filename: {app}\digitalloggerssetup.vbs; Flags: shellexec

; Only if driver is .NET
[UninstallRun]
; Only for .NET assembly/in-proc drivers
Filename: "{dotnet4032}\regasm.exe"; Parameters: "-u ""{app}\ASCOM.DigitalLoggers.Switch.dll"""; Flags: runhidden 32bit
Filename: "{dotnet4064}\regasm.exe"; Parameters: "-u ""{app}\ASCOM.DigitalLoggers.Switch.dll"""; Flags: runhidden 64bit; Check: IsWin64

[CODE]
//
// Before the installer UI appears, verify that the (prerequisite)
// ASCOM Platform 6.0 or greater is installed, including both Helper
// components. Utility is required for all types (COM and .NET)!
//
function InitializeSetup(): Boolean;
var
   U : Variant;
   H : Variant;
begin
   Result := FALSE;  // Assume failure
   // check that the Utilities object exists, report errors if they don't
   try
      U := CreateOLEObject('ASCOM.Utilities.Util');
   except
      MsgBox('The ASCOM Utilities object has failed to load, this indicates that the ASCOM Platform has not been installed correctly', mbInformation, MB_OK);
   end;
   try
      if (U.IsMinimumRequiredVersion(6,0)) then	// this will work in all locales
         Result := TRUE;
   except
   end;
   if(not Result) then
      MsgBox('The ASCOM Platform 6.0 or greater is required for this driver.', mbInformation, MB_OK);
end;

// Code to enable the installer to uninstall previous versions of itself when a new version is installed
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  UninstallExe: String;
  UninstallRegistry: String;
begin
  if (CurStep = ssInstall) then // Install step has started
	begin
      // Create the correct registry location name, which is based on the AppId
      UninstallRegistry := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}' + '_is1');
      // Check whether an extry exists
      if RegQueryStringValue(HKLM, UninstallRegistry, 'UninstallString', UninstallExe) then
        begin // Entry exists and previous version is installed so run its uninstaller quietly after informing the user
          MsgBox('Setup will now remove the previous version.', mbInformation, MB_OK);
          Exec(RemoveQuotes(UninstallExe), ' /SILENT', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
          sleep(1000);    //Give enough time for the install screen to be repainted before continuing
        end
  end;
end;

