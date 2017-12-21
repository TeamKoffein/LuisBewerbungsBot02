using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Text;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;

namespace Bewerbungs.Bot.Luis
{
    //ProactiveBot.Communications
    public class DebugActivityLogger : IActivityLogger
    {
        public async Task LogAsync(IActivity activity)
        {
            Debug.WriteLine($"From:{activity.From.Id} - To:{activity.Recipient.Id} - Message:{activity.AsMessageActivity()?.Text}");
            string path = activity.Conversation.Id + ".txt";
            string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            CloudStorageAccount csa = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = csa.CreateCloudBlobClient();
            CloudBlobContainer container=blobClient.GetContainerReference("logging");
            CloudBlockBlob blob = container.GetBlockBlobReference(path);
            string contents = $"From:{activity.From.Id} - To:{activity.Recipient.Id} - Message:{activity.AsMessageActivity()?.Text}";

            string oldContent;
            if (!blob.Exists())
            {
                oldContent = "";
            }
            else
            {
                using (StreamReader reader = new StreamReader(blob.OpenRead()))
                {
                    oldContent = reader.ReadToEnd();
                }
            }

            using (StreamWriter writer = new StreamWriter(blob.OpenWrite()))
            {
                writer.Write(oldContent + Environment.NewLine);
                writer.Write(contents + Environment.NewLine);
            }
        /*
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(destPath))
                {
                }
            }

            try
            {
                //Open the File
                StreamWriter sw = new StreamWriter(destPath, true, Encoding.ASCII);
                sw.WriteLine($"From:{activity.From.Id} - To:{activity.Recipient.Id} - Message:{activity.AsMessageActivity()?.Text}");
                //close the file
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }*/
        }
    }
}