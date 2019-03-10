//#define TEST
//tabs=4
// --------------------------------------------------------------------------------
//
// ASCOM Switch driver for DigitalLoggers
//
// Implements:	ASCOM Switch interface version: <To be completed by driver developer>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// dd-mmm-yyyy	XXX	6.0.0	Initial edit, created from ASCOM driver template
// --------------------------------------------------------------------------------
// 14-Oct-2014 Chris Rowland 6.0.0 - 1.0.0.0 - initial driver Release
// 11-Nov-2015 Todd Benko    6.0.0 - 1.0.1.0 - added the action(pulse) and the ACP tiddler examples that will
//                                             work with this release.  Changed the name over to 
//                                             Digital Loggers Web Power Switch
// 26-Feb-2019 Manoj Koushik 6.0.1 - 2.0.0.0 - Changed Switch access to be over REST and JSON instead of parsing HTML so 
//                                             it works with newer switches. Code clean up
//
//
// This is used to define code in the template that is specific to one class implementation
// unused code canbe deleted and this definition removed.
#define Switch

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;

namespace ASCOM.DigitalLoggers
{
    //
    // Your driver's DeviceID is ASCOM.DigitalLoggers.Switch
    //
    // The Guid attribute sets the CLSID for ASCOM.DigitalLoggers.Switch
    // The ClassInterface/None addribute prevents an empty interface called
    // _DigitalLoggers from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Switch Driver for DigitalLoggers.
    /// </summary>
    [Guid("158a2313-67af-4c0e-bbbd-c1047de76045")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Switch : ISwitchV2
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.DigitalLoggers.Switch";
       /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "Ascom driver for DigitalLoggers Web Power Switch series";

        internal static bool traceState;

        ///// <summary>
        ///// Private variable to hold the connected state
        ///// </summary>
        //private bool connectedState;

        /// <summary>
        /// Private variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        private TraceLogger tl;

        internal static string IpAddress { get; set; }
        internal static string UserName { get; set; }
        internal static string Password { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalLoggers"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Switch()
        {
            ReadProfile(); // Read device configuration from the ASCOM Profile store

            tl = new TraceLogger("", "DigitalLoggers");
            tl.Enabled = traceState;
            tl.LogMessage("Switch", "Starting initialisation");

            //connectedState = false; // Initialise connected to false
            //TODO: Implement your additional construction here

            tl.LogMessage("Switch", "Completed initialisation");
        }


        //
        // PUBLIC COM INTERFACE ISwitchV2 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsConnected)
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm())
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", "Returning arraylist");
                ArrayList al = new ArrayList();
                al.Add("Action(\"Pulse\", PortNumber)");  // This action will issue a reboot to the port number
                return al;
                //return new ArrayList();
            }
        } 

        public string Action(string actionName, string actionParameters)
        {   //  SupportedActions ("pulse", portNumber:[0 to max number])
            short id;
            string returnval = "";
            switch (actionName.ToLowerInvariant())
            {  
               case "pulse":
                    if (!short.TryParse(actionParameters, out id))
                    {
                        throw new ASCOM.ActionNotImplementedException(actionParameters + " is not a valid number by this driver");
                    }

                    isValid("PulseSwitch", id);
                    isWritable("PulseSwitch", id);
                    CheckConnected("PulseSwitch");
                    try
                    {
                        tl.LogMessage("PulseSwitch", string.Format("PulseSwitch({0}) = {1}", id, "Cycle"));
                        webDevice.Relay_Pulse(id);
                    }
                    catch (Exception ex)
                    {
                        tl.LogIssue("PulseSwitch", "WebDevice error: " + ex.ToString());
                        throw new DriverException("Pulse Switch, webdevice error: " + ex.Message);
                    }
                    return returnval;
                default:
                    throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
            }      
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            throw new ASCOM.MethodNotImplementedException("CommandBlind");
        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            throw new ASCOM.MethodNotImplementedException("CommandBool");
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        public void Dispose()
        {
            // Clean up the tracelogger and util objects
            tl.Enabled = false;
            tl.Dispose();
            tl = null;
        }

        public bool Connected
        {
            get
            {
                tl.LogMessage("Connected Get", IsConnected.ToString());
                return IsConnected;
            }
            set
            {
                tl.LogMessage("Connected Set", value.ToString());
                if (value == IsConnected)
                    return;

                if (value)
                {
                    try
                    {
                        //connectedState = true;
                        tl.LogMessage("Connected Set", "Connecting to device ");
                        webDevice = new WebPowerSwitch_Device();
                        webDevice.Connect();
                        webDevice.RefreshStatus();
                    }
                    catch (Exception ex)
                    {
                        //connectedState = false;
                        webDevice.Disconnect();
                        webDevice = null;
                        tl.LogIssue("set Connected", "error: " + ex.ToString());
                        throw new DriverException("Connect error", ex);
                    }
                }
                else
                {
                    if (webDevice != null)
                    {
                        webDevice.Disconnect();
                        webDevice = null;
                    }
                    //connectedState = false;
                    tl.LogMessage("Connected Set", "Disconnecting from port ");
                    // TODO disconnect from the device
                }
            }
        }

        public string Description
        {
            // TODO customise this device description
            get
            {
                var description = "";
                if (IsConnected)
                {
                    description = webDevice.ControllerName + " Digital Loggers Model:" + webDevice.ControllerModel + " Fw Version: " + webDevice.ControllerVersion;
                }
                tl.LogMessage("Description Get", description);
                return description;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}, version {1}", this.Name, version);
                sb.AppendFormat("\r\n{0}", driverDescription);
                sb.AppendFormat("\r\nOriginal Copyright Chris Rowland");
                sb.AppendFormat("\r\nAmmended by Todd Benko <benkotodd@gmail.com>");
                sb.AppendFormat("\r\nAmmended by Manoj Koushik <manoj.koushik@gmail.com>");
                tl.LogMessage("DriverInfo Get", sb.ToString());
                return sb.ToString();
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                tl.LogMessage("InterfaceVersion Get", "2");
                return Convert.ToInt16("2");
            }
        }

        public string Name
        {
            get
            {
                string name = "Ascom DigitalLoggers WPS Driver";
                tl.LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion

        #region ISwitchV2 Implementation

        private short maxSwitch = 8;
        private WebPowerSwitch_Device webDevice;

        /// <summary>
        /// The number of switches managed by this driver
        /// </summary>
        public short MaxSwitch
        {
            get
            {
                tl.LogMessage("MaxSwitch Get", webDevice.maxOutlets.ToString());
                return webDevice.maxOutlets;
            }
        }

        /// <summary>
        /// Return the name of switch n
        /// </summary>
        /// <param name="id">The switch number to return</param>
        /// <returns>
        /// The name of the switch
        /// </returns>
        public string GetSwitchName(short id)
        {
            isValid("GetSwitchName", id);
            CheckConnected("GetSwitchName");
            try
            {
                var name = webDevice.RelayName(id);
                tl.LogMessage("GetSwitchName", string.Format("GetSwitchName({0}) - {1}", id, name));
                return name;
            }
            catch (Exception ex)
            {
                tl.LogIssue("GetSwitchName", "WebDevice error: " + ex.ToString());
                throw new DriverException("webdevice error " + ex.Message);
            }
        }

        /// <summary>
        /// Sets a switch name to a specified value
        /// </summary>
        /// <param name="id">The number of the switch whose name is to be set</param>
        /// <param name="name">The name of the switch</param>
        public void SetSwitchName(short id, string name)
        {
            tl.LogMessage("SetSwitchName", string.Format("SetSwitchName({0}) = {1} - not implemented", id, name));
            throw new MethodNotImplementedException("SetSwitchName");
        }

        /// <summary>
        /// Gets the switch description.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public string GetSwitchDescription(short id)
        {
            isValid("GetSwitchDescription", id);
            CheckConnected("GetSwitchDescription");
            try
            {
                var name = webDevice.RelayName(id);
                tl.LogMessage("GetSwitchDescription", string.Format("GetSwitchDescription({0}) - returns name {1}", id, name));
                return name;
            }
            catch (Exception ex)
            {
                tl.LogIssue("GetSwitchDescription", "WebDevice error: " + ex.ToString());
                throw new DriverException("webdevice error " + ex.Message);
            }
        }

        /// <summary>
        /// Reports if the specified switch can be written to.
        /// This is false if the switch cannot be written to, for example a limit switch or a sensor.
        /// The default is true.
        /// </summary>
        /// <param name="id">The number of the switch whose write state is to be returned</param><returns>
        ///   <c>true</c> if the switch can be written to, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="MethodNotImplementedException">If the method is not implemented</exception>
        /// <exception cref="InvalidValueException">If id is outside the range 0 to MaxSwitch - 1</exception>
        public bool CanWrite(short id)
        {
            isValid("CanWrite", id);
            // default behavour is to report 
            bool canWrite = webDevice.RelayAvailable(id) && webDevice.RelayAccessible(id);
            tl.LogMessage("CanWrite", string.Format("CanWrite({0}) - default true", canWrite));
            return canWrite;
        }

        #region boolean switch members

        /// <summary>
        /// Return the state of switch n
        /// </summary>
        /// <param name="id">The switch number to return</param>
        /// <returns>
        /// True or false
        /// </returns>
        public bool GetSwitch(short id)
        {
            isValid("GetSwitch", id);
            CheckConnected("GetSwitch");
            try
            {
                var state = webDevice.Status(id);
                tl.LogMessage("GetSwitch", string.Format("GetSwitch({0}) - {1}", id, state));
                return state;
            }
            catch (Exception ex)
            {
                tl.LogIssue("GetSwitch", "WebDevice error: " + ex.ToString());
                throw new DriverException("webdevice error: " + ex.Message);
            }
        }

        /// <summary>
        /// Sets a switch to the specified state
        /// If the switch cannot be set then throws a MethodNotImplementedException.
        /// A multi-value switch must throw a not implemented exception
        /// setting it to false will set it to its minimum value.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        public void SetSwitch(short id, bool state)
        {
            isValid("SetSwitch", id);
            isWritable("SetSwitch", id);
            CheckConnected("SetSwitch");
            try
            {
                tl.LogMessage("SetSwitch", string.Format("SetSwitch({0}) = {1}", id, state));
                webDevice.Relay_Set(id, state);
            }
            catch (Exception ex)
            {
                tl.LogIssue("SetSwitch", "WebDevice error: " + ex.ToString());
                throw new DriverException("webdevice error: " + ex.Message);
            }
        }

        #endregion

        #region analogue members

        /// <summary>
        /// returns the maximum value for this switch
        /// boolean switches must return 1.0
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double MaxSwitchValue(short id)
        {
            isValid("MaxSwitchValue", id);
            // boolean switch implementation:
            return 1;
        }

        /// <summary>
        /// returns the minimum value for this switch
        /// boolean switches must return 0.0
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double MinSwitchValue(short id)
        {
            isValid("MinSwitchValue", id);
            // boolean switch implementation:
            return 0;
        }

        /// <summary>
        /// returns the step size that this switch supports. This gives the difference between
        /// successive values of the switch.
        /// The number of values is ((MaxSwitchValue - MinSwitchValue) / SwitchStep) + 1
        /// boolean switches must return 1.0, giving two states.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double SwitchStep(short id)
        {
            isValid("SwitchStep", id);
            // boolean switch implementation:
            return 1;
        }

        /// <summary>
        /// returns the analogue switch value for switch id
        /// boolean switches must throw a not implemented exception
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double GetSwitchValue(short id)
        {
            tl.LogMessage("GetSwitchValue", string.Format("GetSwitchValue({0}) - not implemented. Boolean switch.", id));
            throw new MethodNotImplementedException("SetSwitchName");
        }

        /// <summary>
        /// set the analogue value for this switch.
        /// If the switch cannot be set then throws a MethodNotImplementedException.
        /// If the value is not between the maximum and minimum then throws an InvalidValueException
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void SetSwitchValue(short id, double value)
        {
            tl.LogMessage("SetSwitchValue", string.Format("SetSwitchValue({0}) = {1} - not implemented. Boolean switch.", id, value));
            throw new MethodNotImplementedException("SetSwitchName");
        }

        #endregion
        #endregion

        #region private methods

        /// <summary>
        /// Checks that the switch id is in range and throws an InvalidValueException if it isn't
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="id">The id.</param>
        private void isValid(string message, short id)
        {
            if (id < 0 || id >= maxSwitch)
            {
                tl.LogMessage(message, string.Format("Switch {0} out of range [0 to {1}]", id, maxSwitch - 1));
                throw new InvalidValueException(message, id.ToString(), string.Format("0 to {0}", maxSwitch - 1));
            }
        }

        private void isWritable(string message, short id)
        {
            if (!webDevice.RelayAccessible(id) || !webDevice.RelayAvailable(id))
            {
                tl.LogMessage(message, string.Format("No permission to access to Switch {0}", id, maxSwitch - 1));
                throw new InvalidValueException(message, id.ToString(), string.Format("No access to {0}", id));
            }
        }
        // here are some useful properties and methods that can be used as required
        // to help with driver development

        #region ASCOM Registration

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "Switch";
                if (bRegister)
                {
                    P.Register(driverID, driverDescription);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return (webDevice != null && webDevice.IsConnected);
                //return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
                traceState = Convert.ToBoolean(driverProfile.GetValue(driverID, "Trace Level", string.Empty, false.ToString()));
                UserName = driverProfile.GetValue(driverID, "UserName", string.Empty, "admin");
                Password = driverProfile.GetValue(driverID, "Password", string.Empty, "4321");
#if TEST
                IpAddress = driverProfile.GetValue(driverID, "IpAddress", string.Empty, "lpc.digital-loggers.com");
#else
                IpAddress = driverProfile.GetValue(driverID, "IpAddress", string.Empty, "192.168.0.100");
#endif
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
                driverProfile.WriteValue(driverID, "Trace Level", traceState.ToString());
                driverProfile.WriteValue(driverID, "IpAddress", IpAddress);
                driverProfile.WriteValue(driverID, "UserName", UserName);
                driverProfile.WriteValue(driverID, "Password", Password);
            }
        }

        #endregion

    }
}
