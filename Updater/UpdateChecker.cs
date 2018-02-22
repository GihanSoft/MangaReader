using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    public static class UpdateChecker
    {
        static HttpClient HttpClient { get; }
        /// <summary>
        /// update api uri
        /// </summary>
        static Uri UriUpdate { get; }

        static UpdateChecker()
        {
            HttpClient = new HttpClient();

            string uriApi = Properties.Resources.ApiUrl;
            if (uriApi.Last() != '/') // check and edit path form for combine 
                uriApi += '/';

            string updateLocation = Properties.Resources.UpdateLocation;
            if (updateLocation.First() == '/') // check and edit path form for combine
                updateLocation = updateLocation.Substring(1);

            string uriUpdate = uriApi;
            uriUpdate += updateLocation;
            UriUpdate = new Uri(uriUpdate);
        }

        public static async Task<bool> Check()
        {
            HttpResponseMessage responseMessage;
            responseMessage = await HttpClient.GetAsync(UriUpdate);
            if(responseMessage.IsSuccessStatusCode)
            {
                string jsonResult = await responseMessage.Content.ReadAsStringAsync();
                UpdateData updateData = JsonConvert.DeserializeObject<UpdateData>(jsonResult);
                if (updateData.VersionCode > UpdateData.ProgramVersionCode)
                    return true;
            }
            return false;
        }
    }
}
