using System;
using System.ComponentModel;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Event_Driven_Demo
{
    public class Function1
    {
        [FunctionName("Function1")]
        public void Run([BlobTrigger("testingpoc1/{name}", Connection = "Connection_String")]Stream myBlob, string name, ILogger log)
        {
            
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var ConnectionStr = Environment.GetEnvironmentVariable("Connection_String");

            BlobContainerClient blobContainerClient = new BlobContainerClient(ConnectionStr, "testingpoc2");

            // Create a new blob for logging
            BlobClient logBlob = blobContainerClient.GetBlobClient("azure_functionsLog.txt");

            string logMsg = $"C# Blob trigger function Processed blob \t{DateTime.Now}\n Name:{name} \n Size: {myBlob.Length} Bytes";

            if (logBlob.Exists())
            {
                // If the blob exists, append the text to it.
                using (MemoryStream stream = new MemoryStream())
                {
                    // Download the current content of the blob
                    logBlob.DownloadTo(stream);

                    // Append the new text
                    stream.Seek(0, SeekOrigin.End);
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(logMsg);
                    writer.Flush();
                    stream.Position = 0;

                    // Upload the updated content back to the blob
                    logBlob.Upload(stream, overwrite: true);
                }
            }
        }
    }
}
