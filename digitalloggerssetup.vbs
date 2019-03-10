' ****************************************************************************************************
'
' This program is will setup the inital paramters to communicate to the Digital loggers Web Power Switch
' Then will connect and verify communicaiton with the unit and name and description information.
'
' 2015-10-24  Todd Benko - Initial Edit
' 2019-02-27  Manoj Koushik - Cleanup and update driver to use REST
'
' ****************************************************************************************************
Dim DLWPS ' declare the variables that ww need
Dim MaxSw 
Dim x
Dim displayStr
' Set variaables values and integer types
x = 0
x = CInt(x)
Maxsw = CInt(MaxSw)

' Main Program now

Set DLWPS = CreateObject("ASCOM.DigitalLoggers.Switch") 

DLWPS.SetupDialog() ' Call the setup dialog to enter the IP address, username and Password
DLWPS.Connected = True
MaxSw = DLWPS.MaxSwitch
MaxSw = CInt(MaxSw)

displayStr = "Device Description: " + DLWPS.Description
displayStr = displayStr + "Driver Name: " + DLWPS.Name + vbCrLf
displayStr = displayStr + "Driver information: " + DLWPS.DriverInfo + vbCrLf
displayStr = displayStr + "Interface information: " + CStr(DLWPS.InterfaceVersion) + vbCrLf
displayStr = displayStr + "Max Switches: " + CStr(MaxSw) + vbCrLf
MsgBox(displayStr)
displayStr = "The Supported Actions in the Action Command:" + vbCrLf
' Now read the Actions
Dim obj
obj = 0
Do While obj < DLWPS.SupportedActions.Count
    displayStr = displayStr + DLWPS.SupportedActions.item(obj) + vbCrLf
    obj = obj + 1
Loop
MsgBox(displayStr)
displayStr = "Following is the devices port name and state(0= OFF, 1= ON):" + vbCrLf
Do While x < MaxSw
    displayStr = displayStr + "Port " & x & " Name: " + DLWPS.GetSwitchName(x) + "  Current State: " + CStr(DLWPS.GetSwitch(x)) + vbCrLf
    x = x + 1
Loop
MsgBox(displayStr)
DLWPS.Connected = False