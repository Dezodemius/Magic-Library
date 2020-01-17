using System.Net.Http;
using System.Collections.Generic;
using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace DbIndexingUtil
{
    /// <summary>
    /// Провайдер для ES.
    /// </summary>
    public static class ElasticsearchProvider
    {
        #region Поля

        /// <summary>
        /// URL Elasticsearch.
        /// </summary>
        private static readonly Uri elasticsearchUrl = new Uri("http://localhost:9200/");

        /// <summary>
        /// HTTP-клиент.
        /// </summary>
        private static readonly HttpClient httpClient = new HttpClient() { BaseAddress = elasticsearchUrl };

        /// <summary>
        /// Путь к документам индекса.
        /// </summary>
        private static readonly string indexPath = "books_index";

        private static readonly string doc = "_doc";

        private static readonly string search = "_search";

        private static readonly string pathMask = "/{0}/{1}";

        private static readonly string mediaType = "application/json";

        #endregion

        #region Методы

        /// <summary>
        /// Метод для отправки сущностей в Elasticsearch.
        /// </summary>
        /// <param name="entities">Список сущностей.</param>
        public static void SendDataToElasticsearch(IEnumerable<Core.Entity> entities)
        {
            foreach (var entity in entities)
            {
                var stringContent = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, mediaType);
                var requestPath = new Uri(elasticsearchUrl, indexPath);
                var result = httpClient.PostAsync(string.Format(pathMask, indexPath, doc), stringContent).Result;
            }
        }

        /// <summary>
        /// Метод для поиска сущностей в Elasticsearch.
        /// </summary>
        /// <param name="entities">Список сущностей.</param>
        public static List<Core.Book> Search(string searchRequest)
        {
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), "/books_index/_search"))
            {
                var searchContent = "{\"query\":{\"bool\":{\"must\":{\"query_string\":{\"query\":\"" + searchRequest + "\"}}}}}";
                request.Content = new StringContent(searchContent);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                var response = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;

                var total = 10;
                var responseBooks = new List<Core.Book>();
                for (int i = 0; i < total; i++)
                    responseBooks.Add(JsonConvert.DeserializeObject<Core.Book>(JObject.Parse(response).SelectToken($"$.['hits'].['hits'][{i}].['_source']").ToString()));

                return responseBooks;
            }
        }

        #endregion
    }
}
