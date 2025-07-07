using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using TransactionService.Application.DTOs;

namespace TransactionService.Tests.Integration
{
    public class DebitTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public DebitTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Debit_ShouldDecreaseBalance()
        {
            var clientId = Guid.NewGuid();
            var credit = new
            {
                id = Guid.NewGuid(),
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 100m
            };
            await _client.PostAsJsonAsync("/api/v1/credit", credit);

            var debit = new
            {
                id = Guid.NewGuid(),
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 40m
            };
            var response = await _client.PostAsJsonAsync("/api/v1/debit", debit);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<TransactionResponseDto>();
            result.Should().NotBeNull();
            result!.ClientBalance.Should().Be(60m);
        }

        [Fact]
        public async Task Debit_ShouldBeIdempotent()
        {
            var clientId = Guid.NewGuid();
            var credit = new
            {
                id = Guid.NewGuid(),
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 100m
            };
            await _client.PostAsJsonAsync("/api/v1/credit", credit);

            var id = Guid.NewGuid();
            var debit = new
            {
                id,
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 30m
            };

            var response1 = await _client.PostAsJsonAsync("/api/v1/debit", debit);
            var response2 = await _client.PostAsJsonAsync("/api/v1/debit", debit);

            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);

            var result1 = await response1.Content.ReadFromJsonAsync<TransactionResponseDto>();
            var result2 = await response2.Content.ReadFromJsonAsync<TransactionResponseDto>();

            result1.Should().BeEquivalentTo(result2, options =>
                options.Using<DateTime>(ctx =>
                {
                    ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(1));
                }).WhenTypeIs<DateTime>()
            );
        }

        [Fact]
        public async Task Debit_MoreThanBalance_ShouldReturnDomainError()
        {
            var clientId = Guid.NewGuid();
            var credit = new
            {
                id = Guid.NewGuid(),
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 10m
            };
            await _client.PostAsJsonAsync("/api/v1/credit", credit);

            var debit = new
            {
                id = Guid.NewGuid(),
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 20m
            };
            var response = await _client.PostAsJsonAsync("/api/v1/debit", debit);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problem!.Detail.Should().Contain("Insufficient funds");
        }
    }
}