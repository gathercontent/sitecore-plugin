﻿using System;
using System.IO;
using System.Net;
using System.Text;
using GatherContent.Connector.Entities;
using Newtonsoft.Json;

namespace GatherContent.Connector.GatherContentService.Services.Abstract
{
    using System.Reflection;

    public abstract class BaseService
    {
        protected virtual string ServiceUrl
        {
            get
            {
                return string.Empty;
            }
        }

        private static string _apiUrl;
        private static string _userName;
        private static string _apiKey;

#if SC81
        private static string _cmsVersion = "8.1";
#else
#if SC80
            private static string _cmsVersion = "8.0";
#else
#if SC72
            private static string _cmsVersion = "7.2";
#else
        private static string _cmsVersion = "8";
#endif
#endif
#endif

        private static readonly string _integrationVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        protected BaseService(GCAccountSettings accountSettings)
        {
            _apiUrl = accountSettings.ApiUrl;
            _apiKey = accountSettings.ApiKey;
            _userName = accountSettings.Username;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;


        }

        protected static WebRequest CreateRequest(string url)
        {
            if (!_apiUrl.EndsWith("/"))
            {
                _apiUrl = _apiUrl + "/";
            }
            HttpWebRequest webrequest = WebRequest.Create(_apiUrl + url) as HttpWebRequest;

            if (webrequest != null)
            {
                string token = GetBasicAuthToken(_userName, _apiKey);
                webrequest.Accept = "application/vnd.gathercontent.v0.5+json";
                webrequest.Headers.Add("Authorization", "Basic " + token);

                webrequest.UserAgent = $"Integration-Sitecore-{_cmsVersion}/{_integrationVersion}";

                return webrequest;
            }

            return null;
        }

        private static string GetBasicAuthToken(string userName, string apiKey)
        {
            string tokenStr = string.Format("{0}:{1}", userName, apiKey);
            return Base64Encode(tokenStr);
        }

        private static string Base64Encode(string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }

        protected static string ReadResponse(WebRequest webrequest)
        {
            using (Stream responseStream = webrequest.GetResponse().GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var responseReader = new StreamReader(responseStream))
                    {
                        return responseReader.ReadToEnd();
                    }
                }
            }

            return null;
        }

        protected static Stream ReadBinaryResponse(WebRequest webrequest)
        {
            using (Stream responseStream = webrequest.GetResponse().GetResponseStream())
            {
                if (responseStream != null)
                {
                    var memoryStream = new MemoryStream();
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    responseStream.CopyTo(memoryStream);
                    return memoryStream;
                }
            }
            return null;
        }


        protected static T ReadResponse<T>(WebRequest webrequest) where T : class
        {
            T result = null;
            using (var responseStream = webrequest.GetResponse().GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var responseReader = new StreamReader(responseStream))
                    {
                        var json = responseReader.ReadToEnd();
                        result = JsonConvert.DeserializeObject<T>(json);
                    }
                }
            }
            return result;
        }

        

        protected static void AddPostData(string data, WebRequest webrequest)
        {
            var byteArray = Encoding.UTF8.GetBytes(data);
            webrequest.ContentType = "application/x-www-form-urlencoded";
            webrequest.ContentLength = byteArray.Length;

            var dataStream = webrequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
        }
    }
}
