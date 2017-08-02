using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeleBot.Models
{
    public class Google
    {
        private CustomsearchService customSearchService = new CustomsearchService(new BaseClientService.Initializer { ApiKey = AppSettings.GoogleApiKey });

        private static Google instance;

        public static Google Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Google();
                }

                return instance;
            }
        }

        public string Search(string query)
        {
            var result = string.Empty;

            try
            {
                var listRequest = this.customSearchService.Cse.List(query);

                listRequest.Cx = AppSettings.GoogleSearchEngineId;

                IList<Result> items = new List<Result>();

                listRequest.Start = 1;

                items = listRequest.Execute().Items;

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item.Pagemap != null)
                        {
                            var imageObject = item.Pagemap.FirstOrDefault(obj => obj.Key == "imageobject").Value;
                            if (imageObject.Any())
                            {
                                var link = imageObject.First().FirstOrDefault(l => l.Key == "url").Value;
                                if (link != null)
                                {
                                    result = link.ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //TODO: log
            }

            return result;
        }

    }
}