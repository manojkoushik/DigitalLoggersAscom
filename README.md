ASCOM Switch Driver For Digital Loggers Web Power Switch

This driver will let you control a Digital Loggers Web Power Switch via an ASCOM switch interface. Not all versions have been fully tested yet. Extensive testing has been done on a LPC9 on firmware version 1.7.9.0.

Installation:
Install and configure the switch as specified by Digital Loggers users manual for Ethernet/WiFi connectivity

Set the Outlet Names and functionality in the Digital Loggers web interface. The ASCOM driver will make this information available to ASCOM clients

Note the IP address/URI, username and password that you use and check that this works through the web interface. Note that you could have a non-admin user with not limited access to the range of outlets

Install this driver, or, you have already installed the driver and are reading this at the end of the install

At the end of ASCOM driver installation a vbs script will be run automatically that will run the ASCOM driver setup. It will also poll the switch with the supplied login information and fetch information on the switch (name, outlets etc.)

The script can be run again later and is found in "%ProgramFiles(x86)%\Common Files\ASCOM\Switch\DigitalLoggersSetup.vbs" (unless you installed the ASCOM driver into a non-standard folder)

Bonus ACP Sample User Interfaces:
Included in the installation is support for ACP. There is a server side asp file to fetch status from the Digital Loggers switch and return it to the web user. There is also a tiddler source file so the web user can easily incorporate Digital Loggers control into their ACP web interface.
Server Side script
The script is located in "%ProgramFiles(x86)%\Common Files\ASCOM\Switch\ACP\digitalloggers.asp" (unless you installed the ASCOM driver into a non-standard folder)

Copy this to "%PUBLIC%\Documents\ACP Web Data\Doc Root\ac" (unless you have a non-standard ACP web root)

Tiddler
Tiddler source can be found in "%ProgramFiles(x86)%\Common Files\ASCOM\Switch\ACP\digitalloggers.tiddler" (unless you installed the ASCOM driver into a non-standard folder)

Open this file in your favorite text editor

Select the entire source contents and copy it (ctrl-c on Windows)

Open ACP web interface and go into authoring mode

Create a new tiddler from the right hand bar, give it a name (e.g. "Observatory Power Control")

In the contents window, paste (ctrl-v on Windows) the source contents you copied earlier

When you click done, the tiddler should now work with the asp server side script to connect to Digital Loggers, poll data and display it. This assumes that on the ACP machine you have already finished driver setup and configuration:

A table should be displayed. At the top of the table, if available, the name of the switch as configured in the Digital Loggers switch, the Model and the firmware version of the switch, are displayed

First column displays the outlet number

Second column displays the name of the outlet as configured in the Digital Loggers switch

Third column is the current state of the outlet (On/Off)

Fourth column gives you control over the outlet. This is context sensitive. That is, if currently the outlet if Off, this will give you the option to turn it On. And vice-versa

Final column gives you the ability to cycle/reboot an outlet. Again this is context sensitive and will only be active if the outlet is currently On (An outlet that is Off cannot be cycled)

If a particular outlet is Locked (you can do this from the control buttons on the switch itself. See Digital Loggers Manual), then the control columns are locked out and shown as "LOCKED"

If a particular outlet is not accessible to the Digital Loggers user (see Digital Loggers Manual) that was used to configure the ASCOM driver, then the row for this outlet will be blanked out
