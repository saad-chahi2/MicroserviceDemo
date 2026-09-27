using CustomerService.Application;
using CustomerService.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public sealed class CustomersController(ICustomerAppService customers) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CustomerResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await customers.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await customers.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Email déjà utilisé",
                Detail = $"Un client avec l'email '{request.Email}' existe déjà.",
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Customer!.Id }, result.Customer);
    }
}
