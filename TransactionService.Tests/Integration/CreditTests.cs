using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using TransactionService.Application.DTOs;

namespace TransactionService.Tests.Integration
{
    public class CreditTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CreditTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Credit_ShouldIncreaseBalance()
        {
            var clientId = Guid.NewGuid();
            var command = new
            {
                id = Guid.NewGuid(),
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 100m
            };

            var response = await _client.PostAsJsonAsync("/api/v1/credit", command);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<TransactionResponseDto>();
            result.Should().NotBeNull();
            result!.ClientBalance.Should().Be(100m);
        }

        [Fact]
        public async Task Credit_ShouldBeIdempotent()
        {
            var clientId = Guid.NewGuid();
            var id = Guid.NewGuid();
            var command = new
            {
                id,
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 50m
            };

            var response1 = await _client.PostAsJsonAsync("/api/v1/credit", command);
            var response2 = await _client.PostAsJsonAsync("/api/v1/credit", command);

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
        public async Task Credit_WithNegativeAmount_ShouldReturnValidationError()
        {
            var command = new
            {
                id = Guid.NewGuid(),
                clientId = Guid.NewGuid(),
                dateTime = DateTime.UtcNow,
                amount = -10m
            };

            var response = await _client.PostAsJsonAsync("/api/v1/credit", command);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problem.Should().NotBeNull();
            problem!.Title.Should().Contain("Validation");
        }
    }
}