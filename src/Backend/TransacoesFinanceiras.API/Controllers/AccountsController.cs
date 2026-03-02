using MediatR;
using Microsoft.AspNetCore.Mvc;
using TransacoesFinanceiras.Application.Commands;
using TransacoesFinanceiras.Application.DTOs;
using TransacoesFinanceiras.Application.Querys;

namespace TransacoesFinanceiras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(IMediator mediator, ILogger<AccountsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Cria uma nova conta
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto dto)
        {
            var command = new CreateAccountCommand(dto);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAccount), new { id = result.AccountId }, result);
        }

        /// <summary>
        /// Obtém uma conta por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDto>> GetAccount(string id)
        {
            var query = new GetAccountQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Obtém todas as transações de uma conta
        /// </summary>
        [HttpGet("{id}/transactions")]
        [ProducesResponseType(typeof(List<TransactionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TransactionDto>>> GetAccountTransactions(string id)
        {
            var query = new GetAccountTransactionsQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
