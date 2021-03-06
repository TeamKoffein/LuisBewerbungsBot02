﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
/*
* KLASSE: Beinhaltet die (Web-) Methoden in C# für die Klasse Xing.aspx, um das Objekt mit den ausgelesenen Profil-
* daten zu verarbeiten und als Textdatei zu speichern.
* 
* */
namespace ProactiveBot
{
    public partial class Xing : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        [WebMethod]
        //Methode: Lies JSON Objekt aus und schreibe diese formatiert zur Kontrolle in einer Textdatei auf dem Desktop gespeichert
        public static bool readXingData(String rawText, String responseName)
        {
            bool success;
            
            CloudStorageAccount csa = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = csa.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("xing");
            CloudBlockBlob blob = container.GetBlockBlobReference(responseName);
            using (StreamWriter writer = new StreamWriter(blob.OpenWrite()))
            {
                writer.Write(value: rawText);
            }
            
            try
            {
                string[] seperatorsToSplit = { ",", "{", "}" };
                string[] splittedText = rawText.Split(seperatorsToSplit, StringSplitOptions.RemoveEmptyEntries);
                writeXingData(splittedText, responseName);
                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                success = false;
            }

            return success;
        }

        //Methode: Schreibe formatiertes Objekt (mit all den Profildaten) als Textdatei und speichere es auf dem Desktop
        private static Boolean writeXingData(string[] xingDataInput, string responseName)
        {
            Boolean success = false;

            // Set a variable to the My Documents path.
            string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Write the string array to a new file 
            using (StreamWriter outputFile = new StreamWriter(mydocpath + @"\XingProfile_"+responseName+".txt"))
            {
                foreach (var line in xingDataInput)
                    outputFile.WriteLine(line);
            }
            success = true;

            return success;

        }
    }
}