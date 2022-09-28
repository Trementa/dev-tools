﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OpenApi.Generator
{
    public class DefinitionSource
    {
        protected readonly Uri Uri;

        public DefinitionSource(string uri) =>
            Uri = new Uri(uri);

        public virtual async Task<Stream> Read(CancellationToken cancellationToken = default)
        {
            if (Uri.IsFile)
            {
                return OpenFile();
            }

            if (Uri.IsAbsoluteUri)
            {
                return await OpenHttp(cancellationToken);
            }

            throw new Exception($"Unknown Uri type {Uri}");
        }

        protected Stream OpenFile() =>
            new FileStream(Uri.AbsolutePath, FileMode.Open);

        protected async Task<Stream> OpenHttp(CancellationToken cancellationToken)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Uri);
            httpRequestMessage.Headers.Add("Accept", "application/json");
            var httpClient = new HttpClient {Timeout = TimeSpan.FromMinutes(5)};
            var httpResponse = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);

            if(!httpResponse.IsSuccessStatusCode)
                throw new Exception(httpResponse.ReasonPhrase);
            return await httpResponse.Content.ReadAsStreamAsync();
        }

        public async Task<DefinitionSource> Update(CancellationToken cancellationToken = default)
        {
            var stream = await Read(cancellationToken);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://converter.swagger.io/api/convert");
            httpRequestMessage.Headers.Add("Accept", "application/json");
            httpRequestMessage.Content = new StreamContent(stream);
            var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            var httpResponse = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
                throw new Exception(httpResponse.ReasonPhrase);
            return new UpdatedSource(await httpResponse.Content.ReadAsStreamAsync());
        }

        internal class UpdatedSource : DefinitionSource
        {
            protected readonly Stream Stream;

            public UpdatedSource(Stream stream) : base(".") =>
                Stream = stream;

            public override Task<Stream> Read(CancellationToken cancellationToken) =>
                Task.FromResult(Stream);
        }
    }
}
