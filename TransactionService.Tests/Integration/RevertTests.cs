using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using TransactionService.Application.DTOs;

namespace TransactionService.Tests.Integration
{
    public class RevertTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public RevertTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Revert_ShouldRestoreBalance()
        {
            var clientId = Guid.NewGuid();
            var creditId = Guid.NewGuid();
            var credit = new
            {
                id = creditId,
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 30m
            };
            await _client.PostAsJsonAsync("/api/v1/credit", credit);

            var debitId = Guid.NewGuid();
            var debit = new
            {
                id = debitId,
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 10m
            };
            await _client.PostAsJsonAsync("/api/v1/debit", debit);

            var revertResponse = await _client.PostAsync($"/api/v1/revert?id={debitId}", null);
            revertResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var revertResult = await revertResponse.Content.ReadFromJsonAsync<RevertResponseDto>();
            revertResult!.ClientBalance.Should().Be(30m);
        }

        [Fact]
        public async Task Revert_ShouldBeIdempotent()
        {
            var clientId = Guid.NewGuid();
            var creditId = Guid.NewGuid();
            var credit = new
            {
                id = creditId,
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 20m
            };
            await _client.PostAsJsonAsync("/api/v1/credit", credit);

            var debitId = Guid.NewGuid();
            var debit = new
            {
                id = debitId,
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 10m
            };
            await _client.PostAsJsonAsync("/api/v1/debit", debit);

            var response1 = await _client.PostAsync($"/api/v1/revert?id={debitId}", null);
            var response2 = await _client.PostAsync($"/api/v1/revert?id={debitId}", null);

            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);

            var result1 = await response1.Content.ReadFromJsonAsync<RevertResponseDto>();
            var result2 = await response2.Content.ReadFromJsonAsync<RevertResponseDto>();

            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result1!.ClientBalance.Should().Be(result2!.ClientBalance);
            result1.ClientBalance.Should().Be(20m);
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

        [Fact]
        public async Task Revert_NonExistentTransaction_ShouldReturnError()
        {
            var response = await _client.PostAsync($"/api/v1/revert?id={Guid.NewGuid()}", null);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problem.Should().NotBeNull();
            problem!.Detail.Should().Contain("Transaction not found");
        }
    }

}