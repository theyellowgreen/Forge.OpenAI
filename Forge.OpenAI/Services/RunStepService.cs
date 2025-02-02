﻿using Forge.OpenAI.Interfaces.Infrastructure;
using Forge.OpenAI.Interfaces.Providers;
using Forge.OpenAI.Interfaces.Services;
using Forge.OpenAI.Settings;
using Forge.OpenAI.Models.Common;
using Forge.OpenAI.Models.Threads;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using System.Threading;
using Forge.OpenAI.Models.Runs;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace Forge.OpenAI.Services
{

    /// <summary>
    /// Represents an execution run on a thread.
    /// https://platform.openai.com/docs/api-reference/runs
    /// </summary>
    public class RunStepService : IRunStepService
    {

        private readonly OpenAIOptions _options;
        private readonly IApiHttpService _apiHttpService;
        private readonly IProviderEndpointService _providerEndpointService;

        /// <summary>Initializes a new instance of the <see cref="RunStepService" /> class.</summary>
        /// <param name="options">The options.</param>
        /// <param name="apiHttpService">The API HTTP service.</param>
        /// <param name="providerEndpointService">The provider endpoint service.</param>
        /// <exception cref="System.ArgumentNullException">options
        /// or
        /// apiHttpService
        /// or
        /// providerEndpointService</exception>
        public RunStepService(OpenAIOptions options, IApiHttpService apiHttpService, IProviderEndpointService providerEndpointService)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (apiHttpService == null) throw new ArgumentNullException(nameof(apiHttpService));
            if (providerEndpointService == null) throw new ArgumentNullException(nameof(providerEndpointService));

            _options = options;
            _apiHttpService = apiHttpService;
            _providerEndpointService = providerEndpointService;
        }

        /// <summary>Initializes a new instance of the <see cref="RunStepService" /> class.</summary>
        /// <param name="options">The options.</param>
        /// <param name="apiHttpService">The API HTTP service.</param>
        /// <param name="providerEndpointService">The provider endpoint service.</param>
        public RunStepService(IOptions<OpenAIOptions> options, IApiHttpService apiHttpService, IProviderEndpointService providerEndpointService)
            : this(options?.Value, apiHttpService, providerEndpointService)
        {
        }

        /// <summary>Gets a run data asynchronously.</summary>
        /// <param name="threadId">The thread identifier.</param>
        /// <param name="runId">The run identifier.</param>
        /// <param name="stepId">The step identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///   RunStepResponse
        /// </returns>
        public async Task<HttpOperationResult<RunStepResponse>> GetAsync(string threadId, string runId, string stepId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(threadId)) return new HttpOperationResult<RunStepResponse>(new ArgumentNullException(nameof(threadId)), System.Net.HttpStatusCode.BadRequest);
            if (string.IsNullOrWhiteSpace(runId)) return new HttpOperationResult<RunStepResponse>(new ArgumentNullException(nameof(runId)), System.Net.HttpStatusCode.BadRequest);
            if (string.IsNullOrWhiteSpace(stepId)) return new HttpOperationResult<RunStepResponse>(new ArgumentNullException(nameof(stepId)), System.Net.HttpStatusCode.BadRequest);

            return await _apiHttpService.GetAsync<RunStepResponse>(GetUri(threadId, runId, stepId), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Gets the list of run steps asynchronously.</summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///   RunStepsListResponse
        /// </returns>
        public async Task<HttpOperationResult<RunStepsListResponse>> GetAsync(RunStepsListRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) return new HttpOperationResult<RunStepsListResponse>(new ArgumentNullException(nameof(request)), System.Net.HttpStatusCode.BadRequest);

            var validationResult = request.Validate<RunStepsListResponse>();
            if (validationResult != null) return validationResult;

            return await _apiHttpService.GetAsync<RunStepsListResponse>(GetListUri(request), cancellationToken).ConfigureAwait(false);
        }

        private string GetUri(string threadId, string runId, string stepId)
        {
            return string.Format(_providerEndpointService.BuildBaseUri(), string.Format(_options.RunStepsGetUri, threadId, runId, stepId));
        }

        private string GetListUri(RunStepsListRequest request)
        {
            StringBuilder sb = new StringBuilder(string.Format(_providerEndpointService.BuildBaseUri(), string.Format(_options.RunStepsListUri, request.ThreadId, request.RunId)));

            List<string> queryParams = new List<string>();

            if (!string.IsNullOrEmpty(request.Order)) queryParams.Add($"order={WebUtility.UrlEncode(request.Order)}");

            if (!string.IsNullOrEmpty(request.After)) queryParams.Add($"after={WebUtility.UrlEncode(request.After)}");

            if (request.Limit.HasValue) queryParams.Add($"limit={request.Limit.Value}");

            if (!string.IsNullOrEmpty(request.Before)) queryParams.Add($"before={WebUtility.UrlEncode(request.Before)}");

            if (queryParams.Count > 0) sb.Append($"?{string.Join("&", queryParams)}");

            return sb.ToString();
        }

    }

}
