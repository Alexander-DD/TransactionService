using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace TransactionService.Tests.Integration
{
    public class ErrorFormatTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ErrorFormatTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Error_ShouldBeInProblemDetailsFormat()
        {
            var command = new
            {
                id = Guid.Empty,
                clientId = Guid.Empty,
                dateTime = DateTime.UtcNow.AddDays(1),
                amount = -1m
            };

            var response = await _client.PostAsJsonAsync("/api/v1/credit", command);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problem.Should().NotBeNull();
            problem!.Type.Should().Contain("rfc9457");
            problem.Title.Should().NotBeNull();
            problem.Status.Should().Be(400);
            problem.Detail.Should().NotBeNull();
            problem.Instance.Should().NotBeNull();
        }
    }
}