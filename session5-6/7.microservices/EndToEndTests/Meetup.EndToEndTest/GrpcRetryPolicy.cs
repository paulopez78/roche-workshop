using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Grpc.Core;
using Polly;
using Polly.Extensions.Http;

namespace Meetup.EndToEndTest
{
    public static class HttpPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() => HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(10, TimeSpan.FromSeconds(60));

        public static IAsyncPolicy<HttpResponseMessage> RetryPolicy(HttpStatusCode[]? retriedHttpStatusCodes = null,
            StatusCode[]? retriedGrpcStatusCodes = null)
        {
            var jitter = new Random();

            retriedGrpcStatusCodes ??= RetriedGrpcStatusCodes;
            retriedHttpStatusCodes ??= RetriedHttpStatusCodes;

            return Policy.HandleResult<HttpResponseMessage>(
                    r =>
                    {
                        var grpcStatus     = GetStatusCode(r);
                        var httpStatusCode = r.StatusCode;

                        return grpcStatus == null && retriedHttpStatusCodes.Contains(httpStatusCode)
                               || httpStatusCode == HttpStatusCode.OK &&
                               retriedGrpcStatusCodes.Contains(grpcStatus!.Value);
                    }
                )
                .WaitAndRetryAsync(
                    3, // exponential back-off plus some jitter
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                    + TimeSpan.FromMilliseconds(jitter.Next(0, 100))
                );

            static StatusCode? GetStatusCode(HttpResponseMessage response)
            {
                var headers = response.Headers;

                if (!headers.Contains("grpc-status") && response.StatusCode == HttpStatusCode.OK)
                    return StatusCode.OK;

                if (headers.Contains("grpc-status"))
                    return (StatusCode) int.Parse(headers.GetValues("grpc-status").First());

                return null;
            }
        }

        static readonly HttpStatusCode[] RetriedHttpStatusCodes =
        {
            HttpStatusCode.BadGateway, HttpStatusCode.GatewayTimeout, HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.InternalServerError, HttpStatusCode.TooManyRequests, HttpStatusCode.RequestTimeout
        };

        static readonly StatusCode[] RetriedGrpcStatusCodes =
        {
            StatusCode.DeadlineExceeded, StatusCode.Internal, StatusCode.NotFound,
            StatusCode.ResourceExhausted, StatusCode.Unavailable, StatusCode.Unknown
        };
    }
}