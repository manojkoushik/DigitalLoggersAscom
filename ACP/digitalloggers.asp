<%@LANGUAGE="JSCRIPT"%>
<script language="JScript" type="text/jscript" runat="server">

var DLWPS = new ActiveXObject("ASCOM.DigitalLoggers.Switch");
var query = Request.QueryString("cmd");

Response.ContentType = "text/json";
var JSON = "";
if(!DLWPS.Connected) {  // need to make sure Digiloggers WPS is connected.
    try {
        DLWPS.Connected=1;
     }
     catch (ex) {}
     ;
 }

if (query == "get") getStatus();
else if (query == "set") setStatus();


function getStatus() {
    try {
        Console.PrintLine("Getting status of all outlets...");
        var cName = DLWPS.Description;
        JSON += "_relayName('" + escape(cName) + "');";

        var i;
        for (i = 0; i < 8; i++) getOutletStatus(i);
        Console.PrintLine("Done");
    }
    catch (ex) {}
    ;
}

function getOutletStatus(o) {
    var name = DLWPS.GetSwitchName(o);
    if (name != "---") {
        JSON += "_outletState('" + o + "','" + escape(name) +"','";
        JSON += escape(DLWPS.GetSwitch(o)==1 ? "ON":"OFF") + "','";
        JSON += escape(DLWPS.CanWrite(o) ? "TRUE":"FALSE") + "');";
    } else JSON += "_outletState('" + o + "','---', '---', '---');";
}

function setStatus() {
    var outlet = Request.QueryString("outlet");
    var state = Request.QueryString("state");

    try {
        var curState = DLWPS.GetSwitch(outlet);
        var name = DLWPS.GetSwitchName(outlet);
        Console.PrintLine("Setting " + name + " to " + state);

        if (state == "on" && !curState) DLWPS.SetSwitch(outlet, true);
        else if (state == "off" && curState) DLWPS.SetSwitch(outlet, false);
        else if (state == "cycle" && curState) DLWPS.Action("pulse", outlet);
        getOutletStatus(outlet);
        Console.PrintLine("Done");
    }
    catch (ex) {}
    ;
}

DLWPS.Connected=0 
Response.Write(JSON);                                                   

</script>


