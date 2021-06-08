using Microsoft.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GuestBook_Data;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private CloudQueue queue;
        private CloudBlobContainer container;


        public override void Run()
        {
            Trace.TraceInformation("WorkerRole is running");

            while (true)
            {
                try
                {
                    CloudQueueMessage message = this.queue.GetMessage();

                    if (message != null)
                    {
                        var messageParts = message.AsString.Split(new char[] { ',' });
                        var imageBlobName = messageParts[0];
                        var partitionKey = messageParts[1];
                        var rowKey = messageParts[2];

                        Trace.TraceInformation("Processing blob image: '{0}'.", imageBlobName);

                        string thumbnailFileName = System.Text.RegularExpressions.Regex
                            .Replace(imageBlobName, "([^\\.]+)(\\.[^\\.]+)?$", "$1-thumb$2");

                        CloudBlockBlob inputBlobFile = container.GetBlockBlobReference(imageBlobName);
                        CloudBlockBlob outputBlobFile = container.GetBlockBlobReference(thumbnailFileName);

                        using (Stream input = inputBlobFile.OpenRead())
                        using (Stream output = outputBlobFile.OpenWrite())
                        {
                            this.ProcessImage(input, output);

                            outputBlobFile.Properties.ContentType = "image/jpeg";
                            string thumbnailBlobUri = outputBlobFile.Uri.ToString();

                            GuestBookDataSource dataSource = new GuestBookDataSource();

                            dataSource.UpdateImageThumbnail(partitionKey, rowKey, thumbnailBlobUri);

                            this.queue.DeleteMessage(message);

                            Trace.TraceInformation("Thumbnail successfully generated in blob file '{0}'.", thumbnailBlobUri);
                        }
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (StorageException e)
                {
                    Trace.TraceInformation("Exception while processing queue item. Error: '{0}'", e.Message);
                    Thread.Sleep(3000);
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.


            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));


            CloudBlobClient blobClientStorage = storageAccount.CreateCloudBlobClient();
            this.container = blobClientStorage.GetContainerReference("guestbookpics");

            CloudQueueClient queueStorage = storageAccount.CreateCloudQueueClient();
            this.queue = queueStorage.GetQueueReference("guestthumbs");

            Trace.TraceInformation("Creating container and queue...");

            bool storageInitialized = false;

            while (!storageInitialized)
            {
                try
                {
                    this.container.CreateIfNotExists();
                    var permissions = this.container.GetPermissions();
                    permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                    this.container.SetPermissions(permissions);

                    this.queue.CreateIfNotExists();

                    storageInitialized = true;
                }
                catch (StorageException e)
                {
                    var requestInformation = e.RequestInformation;

                    var errorCode = requestInformation.ExtendedErrorInformation.ErrorCode;
                    var statusCode = (HttpStatusCode)requestInformation.HttpStatusCode;
                    if (statusCode == HttpStatusCode.NotFound)
                    {
                        Trace.TraceError("Storage services initialization failure. Message: '{0}'", e.Message);
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole has been started");

            return result;
        }


        public void ProcessImage(Stream input, Stream output)
        {
            int width;
            int height;
            var originalImage = new Bitmap(input);

            if (originalImage.Width > originalImage.Height)
            {
                width = 128;
                height = 128 * originalImage.Height / originalImage.Width;
            }
            else
            {
                height = 128;
                width = 128 * originalImage.Width / originalImage.Height;
            }

            Bitmap thumbnailImage = null;
            try
            {
                thumbnailImage = new Bitmap(width, height);

                using (Graphics graphics = Graphics.FromImage(thumbnailImage))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(originalImage, 0, 0, width, height);
                }

                thumbnailImage.Save(output, ImageFormat.Jpeg);
            }
            finally
            {
                if (thumbnailImage != null)
                {
                    thumbnailImage.Dispose();
                }
            }
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
