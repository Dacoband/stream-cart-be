using Appwrite.Services;
using Appwrite;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shared.Common.Settings;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Appwrite.Models;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;

namespace Shared.Common.Services.Appwrite
{
    public class AppwriteService : IAppwriteService
    {
        private readonly AppwriteSetting _appwriteSetting;
        private readonly HttpClient _httpClient;
        private readonly Client _client;
        private readonly Storage _storage;
        public AppwriteService(IOptions<AppwriteSetting> appwriteSetting, HttpClient httpClient)
        {
            _appwriteSetting = appwriteSetting.Value ?? throw new ArgumentNullException(nameof(appwriteSetting));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));


            //_client = new Client()
            // .SetEndpoint(_appwriteSetting.Endpoint)
            //.SetProject(_appwriteSetting.ProjectID)
            //.SetKey(_appwriteSetting.APIKey);

            _client = new Client()
             .SetEndpoint("https://fra.cloud.appwrite.io/v1")
            .SetProject("684c0165002454b6ce62")
            .SetKey("standard_ee8de2c92542ba0c3deaaea4d115aeb4a31c569edf74104426a4ab5cfd8fe7493576ef3ca77ce5873898308ef656d61595eca301c963c7035092ec08a4565c42c33bd05c6974fd0c618744c4e1f727f23bd3998dd8a7caa43ebb46470afd74d122d989d82ee11daf4281d70759dc4a24d05fee89c99d6eff02ffa85d66c2161a");

            _storage = new Storage(_client);
        }
        public async Task<string> UploadImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("Invalid image");

            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var fileContent = new StreamContent(memoryStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);

            var formData = new MultipartFormDataContent
    {
        { fileContent, "file", image.FileName },
        { new StringContent("unique()"), "fileId" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_appwriteSetting.Endpoint}/storage/buckets/{_appwriteSetting.BucketID}/files")
            {
                Content = formData
            };

            request.Headers.Add("X-Appwrite-Project", _appwriteSetting.ProjectID);
            request.Headers.Add("X-Appwrite-Key", _appwriteSetting.APIKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Upload failed: {response.StatusCode} - {body}");
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(resultJson);
            var fileId = result.RootElement.GetProperty("$id").GetString();

            // Tạo URL ảnh view được
            return $"{_appwriteSetting.Endpoint}/storage/buckets/{_appwriteSetting.BucketID}/files/{fileId}/view?project={_appwriteSetting.ProjectID}";
        }


    }
}
