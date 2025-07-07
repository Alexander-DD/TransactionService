using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TransactionService.Application.DTOs;

namespace TransactionService.Tests.Integration
{
    public class BalanceTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public BalanceTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetBalance_ShouldReturnCorrectBalance()
        {
            var clientId = Guid.NewGuid();
            var credit = new
            {
                id = Guid.NewGuid(),
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 70m
            };
            await _client.PostAsJsonAsync("/api/v1/credit", credit);

            var debit = new
            {
                id = Guid.NewGuid(),
                clientId,
                dateTime = DateTime.UtcNow,
                amount = 20m
            };
            await _client.PostAsJsonAsync("/api/v1/debit", debit);

            var response = await _client.GetAsync($"/api/v1/balance?id={clientId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<BalanceResponseDto>();
            result.Should().NotBeNull();
            result!.ClientBalance.Should().Be(50m);
        }

        [Fact]
        public async Task GetBalance_ForUnknownClient_ShouldReturnZero()
        {
            var clientId = Guid.NewGuid();
            var response = await _client.GetAsync($"/api/v1/balance?id={clientId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<BalanceResponseDto>();
            result.Should().NotBeNull();
            result!.ClientBalance.Should().Be(0m);
        }
    }
}