using BusinessLayer.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace BusinessLayer
{
    public class FetchHtml : IFetchHtml
    {
        public string GetHtmlByUrl(string url)
        {
            string result = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader streamReader = new StreamReader(responseStream))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }
    }
}
