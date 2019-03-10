using System;
using System.Net;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace ASCOM.DigitalLoggers
{
    [System.Runtime.InteropServices.ComVisible(false)]
    internal class WebPowerSwitch_Device
    {
        // relay states and names.  Passed as switch devices
        // indexes 0 to 7 are used for relay Ids 1 to 8
        private bool[] relayStatus;
        private string[] relayName;
        private bool[] relayAccessible;
        private bool[] relayAvailable;
        internal short maxOutlets { get; } = 8;

        private DateTime nextUpdateTime = DateTime.MinValue;
        private readonly TimeSpan updateInterval = TimeSpan.FromSeconds(10);

        internal WebPowerSwitch_Device()
        {
            relayStatus = new bool[8];
            relayName = new string[8];
            relayAccessible = new bool[8];
            relayAvailable = new bool[8];

            SetAllowUnsafeHeaderParsing20();
        }

        internal void Connect()
        {
            WebClient = CreateWebClient();
        }

        internal void Disconnect()
        {
            if (WebClient == null)
            {
                return;
            }
            WebClient.Dispose();
            WebClient = null;
        }

        internal bool IsConnected
        {
            get
            {
                return WebClient != null;
            }
        }

        /// <summary>
        /// Get the controller name
        /// </summary>
        internal string ControllerName { get; private set; }
        internal string ControllerModel { get; private set; }
        internal string ControllerVersion { get; private set; }
        /// <summary>
        /// Get the name of the specified relay
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        internal string RelayName(short i)
        {
            RefreshStatusCheck();
            return relayName[i];
        }

        internal bool RelayAvailable(short i)
        {
            RefreshStatusCheck();
            return relayAvailable[i];
        }

        internal bool RelayAccessible(short i)
        {
            RefreshStatusCheck();
            return relayAccessible[i];
        }
        /// <summary>
        /// Returns the current state of a relay.  True = on, False = off
        /// uses the saved values
        /// </summary>
        /// <param name="intRelay"></param>
        /// <returns></returns>
        internal bool Status(short intRelay)
        {
            RefreshStatusCheck();
            return relayStatus[intRelay];
        }

        /// <summary>
        /// Set the state of a relay, the relay ID is one more than the index supplied
        /// </summary>
        /// <param name="intRelay"></param>
        /// <param name="state"></param>
        internal void Relay_Set(int intRelay, bool state)
        {
            WebClient.Headers.Add("Accept", "application/json");
            WebClient.Headers.Add("Content-Type", "application/json");
            WebClient.Headers.Add("X-CSRF", "x");
            string address = string.Format("{0}outlets/{1}/state/", DeviceUri, intRelay);
            var response = WebClient.UploadString(new Uri(address), "PUT", state.ToString().ToLower());
            relayStatus[intRelay] = state;
            RefreshStatus();
        }

        /// <summary>
        /// Pulse or cycle the state of a relay, the relay ID is one more than the index supplied
        /// </summary>
        /// <param name="intRelay"></param>
        internal void Relay_Pulse(int intRelay)
        {
            WebClient.Headers.Add("Accept", "application/json");
            WebClient.Headers.Add("X-CSRF", "x");
            string address = string.Format("{0}outlets/{1}/cycle/", DeviceUri, intRelay);
            var response = WebClient.UploadString (new Uri(address), "POST", "");
            RefreshStatus();
        }

        private System.Net.WebClient _objWebRequest;

        private System.Net.WebClient WebClient
        {
            get { return _objWebRequest; }
            set { _objWebRequest = value; }
        }

        private System.Net.WebClient CreateWebClient()
        {
            System.Net.WebClient wc = new WebClient();

            string loginPassword = string.Format("{0}:{1}", Switch.UserName, Switch.Password);
            byte[] bytLoginPassword = System.Text.Encoding.UTF8.GetBytes(loginPassword);
            string loginPasswordEncoded = Convert.ToBase64String(bytLoginPassword);

            wc.Headers.Add(string.Format("Authorization: Basic {0}", loginPasswordEncoded));
            return wc;
        }

        private string DeviceUri
        {
            get
            {
                string reply = string.Format(@"http://{0}/restapi/relay/", Switch.IpAddress);
                return reply;
            }
        }

        /// <summary>
        /// http://social.msdn.microsoft.com/forums/en-US/netfxnetcom/thread/ff098248-551c-4da9-8ba5-358a9f8ccc57/
        /// Allows talking to servers that only terminate with CR instead of the standard CRLF
        /// </summary>
        /// <returns></returns>
        private static bool SetAllowUnsafeHeaderParsing20()
        {
            //Get the assembly that contains the internal class
            System.Reflection.Assembly aNetAssembly = System.Reflection.Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
            if (aNetAssembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (aSettingsType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created allready the property will create it for us.
                    object anInstance = aSettingsType.InvokeMember("Section",
                      System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic, null, null, new object[] { });

                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework is unsafe header parsing should be allowed or not
                        System.Reflection.FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a status update is needed and do so.
        /// </summary>
        private void RefreshStatusCheck()
        {
            if (DateTime.Now > nextUpdateTime)
            {
                RefreshStatus();
            }
        }

        /// <summary>
        /// Ask the device to send the current status.
        /// </summary>
        public void RefreshStatus()
        {
            string strUriIndex = string.Format("{0}", DeviceUri);
            JavaScriptSerializer js = new JavaScriptSerializer();
            ControllerName = WebClient.DownloadString(new Uri(strUriIndex + "name/"));
            ControllerModel = "N/A";
            ControllerVersion = "N/A";
            var response = WebClient.DownloadString(new Uri(strUriIndex + "model/"));
            dynamic cModel = js.Deserialize<dynamic>(response);
            if (cModel.GetType() == typeof(String)) ControllerModel = cModel;    
            response = WebClient.DownloadString(new Uri(strUriIndex + "version/"));
            dynamic cVer = js.Deserialize<dynamic>(response);
            if (cVer.GetType() == typeof(String)) ControllerVersion = cVer;

            response = WebClient.DownloadString(new Uri(strUriIndex + "outlets/"));
            dynamic outlets = js.Deserialize<dynamic>(response);
            for (int i = 0; i < 8; i++)
            {
                if (outlets[i]["name"].GetType() == typeof(String))
                {
                    relayStatus[i] = outlets[i]["physical_state"];
                    relayName[i] = outlets[i]["name"];
                    relayAccessible[i] = true;
                    relayAvailable[i] = outlets[i]["locked"];
                    relayAvailable[i] = !relayAvailable[i];
                } else
                {
                    relayName[i] = "---";
                    relayAccessible[i] = false;
                    relayAvailable[i] = false;
                }
            }
            nextUpdateTime = DateTime.Now + updateInterval;
        }
    }
}
