using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Botbin {
    internal class Giphy {
        private readonly Uri _baseUri = new Uri("https://api.giphy.com", UriKind.Absolute);
        private readonly HttpClient _client = new HttpClient();
        private readonly string _key;

        public Giphy(string key) => _key = key;

        public Task<Uri> RandomWithTag(string tag) =>
            RequestGif(new Uri(_baseUri, $"v1/gifs/random?api_key={_key}&tag={tag}&rating=R"));

        public Task<Uri> Random() =>
            RequestGif(new Uri(_baseUri, $"v1/gifs/random?api_key={_key}&tag=&rating=R"));

        public Task<Uri> Search(string term) =>
            RequestGif(new Uri(_baseUri,
                $"/v1/gifs/translate?api_key={_key}&s={term}"));

        private async Task<Uri> RequestGif(Uri uri) => new Uri(
            JsonConvert.DeserializeAnonymousType(
                await _client.GetStringAsync(uri)
                , new {data = new {url = string.Empty}}
            ).data.url, UriKind.Absolute
        );
    }
}