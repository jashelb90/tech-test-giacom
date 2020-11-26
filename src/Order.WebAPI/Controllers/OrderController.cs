using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Service;
using System;
using System.Net;
using System.Threading.Tasks;
using Order.Model.Enums;
using Order.Model.Requests;
using Order.Utility.Exceptions;

namespace OrderService.WebAPI.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                return Ok(order);
            }
            else
            {
                return NotFound();
            }
        }
        
        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            var isValidStatus = Enum.TryParse(status, true, out OrderStatusEnum orderStatus);

            if (!isValidStatus)
            {
                return BadRequest("Please provide a valid order status.");
            }
            
            var order = await _orderService.GetOrdersByStatusAsync(orderStatus);
            
            if (order != null)
            {
                return Ok(order);
            }

            return NotFound();
        }
        
        [HttpPatch("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] Guid orderId, [FromQuery(Name="status")] string status)
        {
            var isValidStatus = Enum.TryParse(status, true, out OrderStatusEnum orderStatus);

            if (!isValidStatus)
            {
                return BadRequest("Please provide a valid order status.");
            }

            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(orderId, orderStatus);

                return Ok(order);

            }
            catch (EntityNotFoundException ex)
            {
                return StatusCode((int) HttpStatusCode.NotFound, ex.Message);
            }
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrder createOrder)
        {
            try
            {
                var orderId = await _orderService.CreateOrderAsync(createOrder);
            
                return Created(Request.Path, orderId);
            
            }
            catch (EntityNotFoundException ex)
            {
                return StatusCode((int) HttpStatusCode.NotFound, ex.Message);
            }
        }
        
        [HttpGet("profit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> OrderProfit()
        {
            var profitDetails = await _orderService.GetOrderProfitAsync();
            return Ok(profitDetails);
        }
    }
}
