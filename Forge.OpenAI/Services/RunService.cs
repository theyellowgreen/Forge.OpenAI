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
using System.Text;
using System.Net;
using System.Collections.Generic;

namespace Forge.OpenAI.Services
{

    /// <summary>
    /// Represents an execution run on a thread.
    /// https://platform.openai.com/docs/api-reference/runs
    /// </summary>
    public class RunService : IRunService
    {

        private readonly OpenAIOptions _options;
        private readonly IApiHttpService _apiHttpService;
        private readonly IProviderEndpointService _providerEndpointService;

        /// <summary>Initializes a new instance of the <see cref="RunService" /> class.</summary>
        /// <param name="options">The options.</param>
        /// <param name="apiHttpService">The API HTTP service.</param>
        /// <param name="providerEndpointService">The provider endpoint service.</param>
        /// <exception cref="System.ArgumentNullException">options
        /// or
        /// apiHttpService
        /// or
        /// providerEndpointService</exception>
        public RunService(OpenAIOptions options, IApiHttpService apiHttpService, IProviderEndpointService providerEndpointService)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (apiHttpService == null) throw new ArgumentNullException(nameof(apiHttpService));
            if (providerEndpointService == null) throw new ArgumentNullException(nameof(providerEndpointService));

            _options = options;
            _apiHttpService = apiHttpService;
            _providerEndpointService = providerEndpointService;
        }

        /// <summary>Initializes a new instance of the <see cref="RunService" /> class.</summary>
        /// <param name="options">The options.</param>
        /// <param name="apiHttpService">The API HTTP service.</param>
        /// <param name="providerEndpointService">The provider endpoint service.</param>
        public RunService(IOptions<OpenAIOptions> options, IApiHttpService apiHttpService, IProviderEndpointService providerEndpointService)
            : this(options?.Value, apiHttpService, providerEndpointService)
        {
        }

        /// <summary>Creates a run asynchronously.</summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///   CreateRunResponse
        /// </returns>
        public async Task<HttpOperationResult<CreateRunResponse>> CreateAsync(CreateRunRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) return new HttpOperationResult<CreateRunResponse>(new ArgumentNullException(nameof(request)), System.Net.HttpStatusCode.BadRequest);

            var validationResult = request.Validate<CreateRunResponse>();
            if (validationResult != null) return validationResult;

            return await _apiHttpService.PostAsync<CreateRunRequest, CreateRunResponse>(GetCreateUri(request.ThreadId), request, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Creates a thread and run in one request asynchronously.</summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///   CreateThreadAndRunResponse
        /// </returns>
        public async Task<HttpOperationResult<CreateThreadAndRunResponse>> CreateThreadAndRunAsync(CreateThreadAndRunRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) return new HttpOperationResult<CreateThreadAndRunResponse>(new ArgumentNullException(nameof(request)), System.Net.HttpStatusCode.BadRequest);

            var validationResult = request.Validate<CreateThreadAndRunResponse>();
            if (validationResult != null) return validationResult;

            return await _apiHttpService.PostAsync<CreateThreadAndRunRequest, CreateThreadAndRunResponse>(_options.RunThreadAndRunCreateUri, request, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Gets a run data asynchronously.</summary>
        /// <param name="threadId">The thread identifier.</param>
        /// <param name="runId">The run identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///   RunResponse
        /// </returns>
        public async Task<HttpOperationResult<RunResponse>> GetAsync(string threadId, string runId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(threadId)) return new HttpOperationResult<RunResponse>(new ArgumentNullException(nameof(threadId)), System.Net.HttpStatusCode.BadRequest);
            if (string.IsNullOrWhiteSpace(runId)) return new HttpOperationResult<RunResponse>(new ArgumentNullException(nameof(runId)), System.Net.HttpStatusCode.BadRequest);

            return await _apiHttpService.GetAsync<RunResponse>(GetUri(threadId, runId), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Gets the list of runs asynchronously.</summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///   RunListResponse
        /// </returns>
        public async Task<HttpOperationResult<RunListResponse>> GetAsync(RunListRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) return new HttpOperationResult<RunListResponse>(new ArgumentNullException(nameof(request)), System.Net.HttpStatusCode.BadRequest);

            var validationResult = request.Validate<RunListResponse>();
            if (validationResult != null) return validationResult;

            return await _apiHttpService.GetAsync<RunListResponse>(GetListUri(request), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Modifies a run asynchronously</summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///   ModifyRunResponse
        /// </returns>
        public async Task<HttpOperationResult<ModifyRunResponse>> ModifyAsync(ModifyRunRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) return new HttpOperationResult<ModifyRunResponse>(new ArgumentNullException(nameof(request)), System.Net.HttpStatusCode.BadRequest);

            var validationResult = request.Validate<ModifyRunResponse>();
            if (validationResult != null) return validationResult;

            return await _apiHttpService.PostAsync<ModifyRunRequest, ModifyRunResponse>(GetModifyUri(request), request, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// When a run has the status: "requires_action" and required_action.type is submit_tool_outputs, 
        /// this endpoint can be used to submit the outputs from the tool calls once they're all completed. 
        /// All outputs must be submitted in a single request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///   SubmitToolOutputsToRunResponse
        /// </returns>
        public async Task<HttpOperationResult<SubmitToolOutputsToRunResponse>> SubmitToolOutputsToRunAsync(SubmitToolOutputsToRunRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) return new HttpOperationResult<SubmitToolOutputsToRunResponse>(new ArgumentNullException(nameof(request)), System.Net.HttpStatusCode.BadRequest);

            var validationResult = request.Validate<SubmitToolOutputsToRunResponse>();
            if (validationResult != null) return validationResult;

            return await _apiHttpService.PostAsync<SubmitToolOutputsToRunRequest, SubmitToolOutputsToRunResponse>(GetSubmitToolOutputsToRunUri(request), request, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Cancels a run that is in_progress.</summary>
        /// <param name="threadId">The thread identifier.</param>
        /// <param name="runId">The run identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///   RunResponse
        /// </returns>
        public async Task<HttpOperationResult<RunResponse>> CancelAsync(string threadId, string runId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(threadId)) return new HttpOperationResult<RunResponse>(new ArgumentNullException(nameof(threadId)), System.Net.HttpStatusCode.BadRequest);
            if (string.IsNullOrWhiteSpace(runId)) return new HttpOperationResult<RunResponse>(new ArgumentNullException(nameof(runId)), System.Net.HttpStatusCode.BadRequest);

            return await _apiHttpService.PostAsync<object, RunResponse>(CancelUri(threadId, runId), null, null, cancellationToken).ConfigureAwait(false);
        }

        private string GetCreateUri(string threadId)
        {
            return string.Format(_providerEndpointService.BuildBaseUri(), string.Format(_options.RunCreateUri, threadId));
        }

        private string GetUri(string threadId, string runId)
        {
            return string.Format(_providerEndpointService.BuildBaseUri(), string.Format(_options.RunGetUri, threadId, runId));
        }

        private string GetListUri(RunListRequest request)
        {
            StringBuilder sb = new StringBuilder(string.Format(_providerEndpointService.BuildBaseUri(), string.Format(_options.RunListUri, request.ThreadId)));

            List<string> queryParams = new List<string>();

            if (!string.IsNullOrEmpty(request.Order)) queryParams.Add($"order={WebUtility.UrlEncode(request.Order)}");

            if (!string.IsNullOrEmpty(request.After)) queryParams.Add($"after={WebUtility.UrlEncode(request.After)}");

            if (request.Limit.HasValue) queryParams.Add($"limit={request.Limit.Value}");

            if (!string.IsNullOrEmpty(request.Before)) queryParams.Add($"before={WebUtility.UrlEncode(request.Before)}");

            if (queryParams.Count > 0) sb.Append($"?{string.Join("&", queryParams)}");

            return sb.ToString();
        }

        private string GetModifyUri(ModifyRunRequest request)
        {
            return string.Format(_providerEndpointService.BuildBaseUri(), string.Format(_options.RunModifyUri, request.ThreadId, request.RunId));
        }

        private string GetSubmitToolOutputsToRunUri(SubmitToolOutputsToRunRequest request)
        {
            return string.Format(_providerEndpointService.BuildBaseUri(), string.Format(_options.RunSubmitToolOutputsToRunUri, request.ThreadId, request.RunId));
        }

        private string CancelUri(string threadId, string runId)
        {
            return string.Format(_providerEndpointService.BuildBaseUri(), string.Format(_options.RunCancelUri, threadId, runId));
        }

    }

}
