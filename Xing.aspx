﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Xing.aspx.cs" Inherits="ProactiveBot.Xing" %>

<!--
    KLASSE: Diese Klasse ruft den Plugin Xing auf, damit der User sich in sein Account einloggen kann. 
    Das Plugin gibt nach dem Aufruf und nach dem erfolgreichen Login des Users ein Objekt mit den ausgelesenen 
    Daten zurück. Die Methoden zur Verarbeitung sind in Xing.aspx.cs implementiert.
    -->

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <link rel="stylesheet" href="default.css"/>
    <script src="http://cdn.jsdelivr.net/g/filesaver.js"></script>
   <title>liseBot - Xing Login</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
            
            <!-- Header -->
            <header class="text-center">
                <div class="intro-text">
                    <h1>Let me in!</h1>
                    <p>Please login with your information into your Xing account.</p>

                    <!-- Javascript fuer das Plugin und zum Auslesen der Daten (erfolgt durch Xing automatisch-->
                    <script type="xing/login">
                        {
                        "consumer_key": "f08ec0e77c6da5bac295"
                        }
                    </script>

                    <p id='output'></p>
                    <p id='outlog'></p>
                    <br />

                    <!-- Xing Plugin Anbindung -->
                    <script>
                        (function (d) {
                            var js, id = 'lwx';
                            if (d.getElementById(id)) return;
                            js = d.createElement('script'); js.id = id; js.src = "https://www.xing-share.com/plugins/login.js";
                            d.getElementsByTagName('head')[0].appendChild(js)
                        }(document));
                    </script>

                     <script>
                         //'use strict';
                         //Methode: Erhält das (JSON) Objekt nach dem erfolgreichen Login des Users und 
                         //verarbeitet diese, indem readXingData aufgerufen wird und zur Kontrolle das Objekt in einer 
                         //Textdatei gespeichert wird
                         function onXingAuthLogin(response) {
                             var output;
                             var filecontent;
                             var filename;
                             var success;
                             var responseName;

                             //Kontrolle des erfolgreichen Logins
                             if (response.user) {
                                 output = 'Successful login for ' + response.user.display_name;
                                 responseName = response.user.first_name + response.user.last_name;
                             }
                             else if (response.error) {
                                 output = 'No user is logged in :( ';
                             }
                             document.getElementById('output').innerHTML = output;

                             filecontent = JSON.stringify(response);
                             filename = "jsonXing_"+responseName+".json";

                             //Rufe c# Methode auf, um das Objekt auszuwerten
                             success = PageMethods.readXingData(filecontent, responseName);
                             var blob = new Blob([filecontent], { type: "text/plain;charset=utf-8" }); saveAs(blob, filename + ".json");

                             /*
                             //Logge den User nach dem Vorgang aus, wenn C# Methodenaufruf erfolgreich
                             if (success)
                             {
                                 xing.logout(callback, consumer_key);
                                 outlog = 'Successful logout for ' + response.user.display_name;
                             }
                             //xing.logout(callback, consumer_key);
                             */
                         }
                    </script>

                </div>
            </header>
            
           
        </div>
    </form>
</body>
</html>
