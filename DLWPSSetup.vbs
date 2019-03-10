' ****************************************************************************************************
'
' This program is will setup the inital paramters to communicate to the Digiloggers Web Power Switch
' Then will connect and verify communicaiton with the unit and name and description information.
'
' 2015-10-24  Todd Benko - Initial Edit
'
' ****************************************************************************************************
Dim DLWPS ' declare the variables that ww need
Dim MSw 
Dim x
Dim actionslist
' Set variaables values and integere types
x = 0
x = CInt(x)
Maxsw = CInt(MaxSw)
'Set actionslist = CreateObject("System.Collections.Arraylist")

' Main Program now

Set DLWPS = CreateObject("ASCOM.DigitalLoggers.Switch") 

DLWPS.SetupDialog() ' Call the setup dialog to enter the IP address, username and Password
DLWPS.Connected = True
MaxSw = DLWPS.MaxSwitch

Wscript.Echo ("INFORMATION ABOUT THE DRIVER AND THE WEB POWER SWITCH:")
Wscript.Echo ("Device Name: " & DLWPS.Name) 
Wscript.Echo ("Device Description: " & DLWPS.Description)
Wscript.Echo ("Max Switches: " & MaxSw)
Wscript.Echo ("Driver information: " & DLWPS.DriverInfo)
Wscript.Echo ("Interface information: " & DLWPS.InterfaceVersion)
Wscript.Echo (DLWPS.SupportedActions.Count)
Wscript.Echo (DLWPS.SupportedActions.Capacity)
Wscript.Echo ("The Supported Actions in the Action Command:")
' Now read the Actions
Dim obj 
obj = 0
Do while obj < DLWPS.SupportedActions.Count
    Wscript.Echo (DLWPS.SupportedActions.item(obj))
    obj = obj + 1
Loop
Wscript.Echo ("")
Wscript.Echo ("Following is the devices port name and state(0= OFF, 1= ON):")
Wscript.Echo ("")
Do while x < MaxSw
  Wscript.Echo ("Port " & x & " Name: " & DLWPS.GetSwitchName(x) & "  Current State: " & DLWPS.GetSwitchValue(x))
  x = x + 1
Loop 

DLWPS.Connected = False
MsgBox "Press OK to close when done reading the output."

Stop
