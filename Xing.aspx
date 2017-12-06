<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Xing.aspx.cs" Inherits="ProactiveBot.Xing" %>

<!--
    # Xing Dialog -> alternative zu cards: Links
    # Statt Textdatei direkt string nutzen (überspringe Text)
    -->

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
   <script src="http://cdn.jsdelivr.net/g/filesaver.js"></script>
   <title>liseBot - Xing Login</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
            <h1>Xing - LogIn</h1>
            <p>Please login with your information into your Xing account.</p>

            <script>
                //'use strict';
                function onXingAuthLogin(response) {
                    var output;
                    var filecontent;
                    var filename

                    if (response.user) {
                        output = 'Successful login for ' + response.user.display_name;
                    }
                    else if (response.error) {
                        output = 'No user is logged in :( ';
                    }
                    document.getElementById('output').innerHTML = output;

                    filecontent = JSON.stringify(response);
                    filename = "jsonXing";

                    PageMethods.readXingData(filecontent);
                    var blob = new Blob([filecontent], { type: "text/plain;charset=utf-8" }); saveAs(blob, filename + ".json");
                }
            </script>

            <!-- Javascript fuer das Plugin und zum Auslesen der Daten (erfolgt durch Xing automatisch-->
            <script type="xing/login">
                {
                "consumer_key": "24e60f9f3ef44685ec15"
                }
            </script>

            <p id='output'></p>
            <br />

            <script>
                (function (d) {
                    var js, id = 'lwx';
                    if (d.getElementById(id)) return;
                    js = d.createElement('script'); js.id = id; js.src = "https://www.xing-share.com/plugins/login.js";
                    d.getElementsByTagName('head')[0].appendChild(js)
                }(document));
            </script>
        </div>
    </form>
</body>
</html>
