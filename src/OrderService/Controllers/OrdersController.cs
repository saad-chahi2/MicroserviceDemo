using Microsoft.AspNetCore.Mvc;
using OrderService.Application;
using OrderService.Contracts;

namespace OrderService.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public sealed class OrdersController(IOrderAppService orders) : ControllerBase
{
    /// <param name="customerId">Filtre optionnel : commandes d'un client.</param>
    /// <param name="cancellationToken">Annulé si le client HTTP coupe la requête.</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrderResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetAll([FromQuery] int? customerId, CancellationToken cancellationToken) =>
        Ok(await orders.GetAllAsync(customerId, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await orders.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }
}
