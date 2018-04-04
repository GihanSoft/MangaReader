using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Updater.Models;

namespace Updater.Controllers
{
    public class Updater
    {
        HttpClient HttpClient { get; set; }
        WebClient WebClient { get; set; }
        internal UpdateInfo UpdateInfo { get; set; }

        private bool? isNeedUpdate;
        public bool IsNeedUpdate
        {
            get
            {
                if (!isNeedUpdate.HasValue)
                    isNeedUpdate = Check().Result;
                return isNeedUpdate.Value;
            }
        }

        internal byte? DlStatus { get; set; }

        public Updater()
        {
            HttpClient = new HttpClient();
            WebClient = new WebClient();
            isNeedUpdate = null;
        }
        private async Task<bool> Check()
        {
            HttpResponseMessage responseMessage;
            responseMessage = await HttpClient.GetAsync(Uris.This.GetUpdate());
            if (responseMessage.IsSuccessStatusCode)
            {
                string jsonResult = await responseMessage.Content.ReadAsStringAsync();
                UpdateInfo = JsonConvert.DeserializeObject<UpdateInfo>(jsonResult);
                if (UpdateInfo.VersionCode > UpdateInfo.CurrentVersionCode)
                    return true;
            }
            return false;
        }

        private void StartInner()
        {
            var toDl = UpdateInfo.Files;
            DlStatus = 0;
            for (int i = 0; i < toDl.Length;)
            {
                var x = toDl[i].Substring(toDl[i].LastIndexOf("/") + 1);
                x = x.Substring(0, x.Length - 2);
                FileInfo file = new FileInfo(x);
                if (file.Exists) file.Delete();
                WebClient.DownloadFile(new Uri(toDl[i]), file.FullName);
                DlStatus = (byte)(++i / toDl.Length * 100);
            }
        }

        private void StartFull()
        {
            var toDl = Uris.This.GetInstallers();
            DlStatus = 0;
            for (int i = 0; i < toDl.Length;)
            {
                var x = toDl[i].Substring(toDl[i].LastIndexOf("/") + 1);
                x = x.Substring(0, x.Length - 2);
                FileInfo file = new FileInfo(Path.Combine("setup", x));
                if (file.Exists) file.Delete();
                WebClient.DownloadFile(new Uri(toDl[i]), file.FullName);
                DlStatus = (byte)(++i / toDl.Length * 100);
            }
        }
        internal void Start()
        {
            if (UpdateInfo == null)
                Check().Wait();
            if (!IsNeedUpdate) return;
            switch (UpdateInfo.UpdateMode)
            {
                case Models.Enums.UpdateMode.InnerUpdate:
                    Task.Run(() => StartInner());
                    break;
                case Models.Enums.UpdateMode.FullUpdate:
                    Task.Run(() => StartFull());
                    break;
            }
        }
    }
}
