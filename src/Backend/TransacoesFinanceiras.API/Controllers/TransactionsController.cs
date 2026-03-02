using MediatR;
using Microsoft.AspNetCore.Mvc;
using TransacoesFinanceiras.Application.Commands;
using TransacoesFinanceiras.Application.DTOs;
using TransacoesFinanceiras.Application.DTOs.Responses;

namespace TransacoesFinanceiras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(IMediator mediator, ILogger<TransactionsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Cria uma nova transação financeira
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<TransactionResponseDto>> CreateTransaction([FromBody] CreateTransactionDto dto)
        {
            try
            {
                var command = new CreateTransactionCommand(dto);
                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(CreateTransaction), new { id = result.TransactionId }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro ao processar transação: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar transação");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Processa múltiplas transações em lote
        /// </summary>
        [HttpPost("batch")]
        [ProducesResponseType(typeof(List<TransactionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<List<TransactionResponseDto>>> ProcessBatch(
            [FromBody] List<CreateTransactionDto> requests)
        {
            try
            {
                var results = new List<TransactionResponseDto>();
                foreach (var req in requests)
                {
                    var result = await _mediator.Send(new CreateTransactionCommand(req));
                    results.Add(result);
                }
                return Ok(results);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro ao processar transação: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar transação");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
