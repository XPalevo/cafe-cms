namespace TestA;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/controller-a")]
public class ControllerA : ControllerBase
{
    private readonly ServiceA _serviceA;
    
    public ControllerA(ServiceA serviceA)
    {
        _serviceA = serviceA;
    }
    
    [HttpPost("send-order")]
    public async Task<IActionResult> SendOrder([FromBody] SendOrderRequest request)
    {
        await _serviceA.SendOrder(request.Id, request.Description);

        return Ok(new { message = "Order sent" });
    }
}

public class SendOrderRequest
{
    public int Id { get; set; }
    public string? Description { get; set; }
}