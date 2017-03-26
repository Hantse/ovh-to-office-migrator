using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OTOM.Models;
using OVHApi;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Net;

namespace OTOM.Business
{
    public class OvhExportToAzureBl
    {
        private ApiClientExport currentClient;

        public async Task StartExportForUser(ApiClientExport client, string sasUrl, OvhApiClient api, string org, string service)
        {
            currentClient = client;

            await GetExport(client, api, org, service);

            await GetExportUrl(client, api, org, service);

            await StartExportToAzure(client, sasUrl);
        }

        private async Task StartExportToAzure(ApiClientExport client, string sasUrl)
        {
            CloudBlobContainer blobContainer = new CloudBlobContainer(new Uri(sasUrl));
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(Guid.NewGuid().ToString() + ".pst");

            WebClient wc = new WebClient();
            client.Status = "Upload in progress ...";

            try
            {
                await blockBlob.UploadFromStreamAsync(wc.OpenRead(client.ExportUrl));
            }
            catch (Exception e)
            {
                client.Status = "Upload error ...";
            }

            client.Status = "Upload finish !";
        }

        private async Task GetExportUrl(ApiClientExport client, OvhApiClient api, string org, string service)
        {

            OvhApi.Models.Email.Exchange.Task taskUrl = await api.CreateEmailExchangeServiceAccountExporturl(org, service, client.Email);
            OvhApi.Models.Email.Exchange.Task task;

            await Task.Delay(15000);

            do
            {
                task = await api.GetEmailExchangeServiceAccountTasks(org, service, client.Email, taskUrl.Id);
                if (task.Status == OvhApi.Models.Email.Exchange.TaskStatusEnum.Done) break;

                await Task.Delay(60000);

            } while (task.Status != OvhApi.Models.Email.Exchange.TaskStatusEnum.Done);

            OvhApi.Models.Email.Exchange.ExportUrl exportUrl = await api.GetEmailExchangeServiceAccountExporturl(org, service, client.Email);

            client.ExportUrl = exportUrl.Url;
        }

        private async Task GetExport(ApiClientExport client, OvhApiClient api, string org, string service)
        {
            OvhApi.Models.Email.Exchange.Export task = await api.GetEmailExchangeServiceAccountExport(org, service, client.Email);

            do
            {
                task = await api.GetEmailExchangeServiceAccountExport(org, service, client.Email);
                client.Status = task.PercentComplete + "%";
                if (task.PercentComplete == 100)
                    break;

                await Task.Delay(60000);
            } while (task.PercentComplete != 100);
        }
    }
}
