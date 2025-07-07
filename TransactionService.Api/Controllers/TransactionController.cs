using MediatR;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.DTOs;
using TransactionService.Application.UseCases.Credit;
using TransactionService.Application.UseCases.Debit;
using TransactionService.Application.UseCases.GetBalance;
using TransactionService.Application.UseCases.Revert;

namespace TransactionService.Api.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class TransactionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TransactionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("credit")]
        public async Task<IActionResult> Credit([FromBody] CreditCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("debit")]
        public async Task<IActionResult> Debit([FromBody] DebitCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("revert")]
        public async Task<IActionResult> Revert([FromQuery] Guid id)
        {
            var result = await _mediator.Send(new RevertCommand
            {
                TransactionId = id
            });
            return Ok(result);
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance([FromQuery] Guid id)
        {
            var result = await _mediator.Send(new BalanceQuery
            {
                ClientId = id
            });
            return Ok(result);
        }
    }
}
