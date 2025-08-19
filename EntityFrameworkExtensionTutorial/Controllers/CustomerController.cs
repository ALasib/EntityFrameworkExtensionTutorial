using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using EntityFrameworkExtensionTutorial.Application.DTOs;
using EntityFrameworkExtensionTutorial.Application.Interfaces;
using EntityFrameworkExtensionTutorial.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkExtensionTutorial.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService customerService, IMapper mapper, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _mapper = mapper;
        _logger = logger;
    }

    #region Standard CRUD Operations - Use EF Core

    /// <summary>
    /// Get customer by ID - Use EF Core for simple entity retrieval
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var result = await _customerService.GetCustomerByIdAsync(id);
        
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(_mapper.Map<CustomerDto>(result.Value));
    }

    /// <summary>
    /// Get customer by code - Use EF Core for simple lookups
    /// </summary>
    [HttpGet("code/{code}")]
    public async Task<ActionResult<CustomerDto>> GetCustomerByCode(string code)
    {
        var result = await _customerService.GetCustomerByCodeAsync(code);
        
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(_mapper.Map<CustomerDto>(result.Value));
    }

    /// <summary>
    /// Get all customers - Use EF Core for small datasets
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
    {
        var result = await _customerService.GetAllCustomersAsync();
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<IEnumerable<CustomerDto>>(result.Value));
    }

    /// <summary>
    /// Create customer - Use EF Core for single entity creation
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto createDto)
    {
        var customer = _mapper.Map<Customer>(createDto);
        var result = await _customerService.CreateCustomerAsync(customer);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        
        return CreatedAtAction(nameof(GetCustomer), new { id = result.Value.Id }, 
            _mapper.Map<CustomerDto>(result.Value));
    }

    /// <summary>
    /// Update customer - Use EF Core for single entity updates
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto updateDto)
    {
        var existingCustomerResult = await _customerService.GetCustomerByIdAsync(id);
        if (existingCustomerResult.IsFailure)
            return NotFound(new { error = existingCustomerResult.Error });

        var existingCustomer = existingCustomerResult.Value;
        _mapper.Map(updateDto, existingCustomer);
        
        var updateResult = await _customerService.UpdateCustomerAsync(existingCustomer);
        if (updateResult.IsFailure)
            return BadRequest(new { error = updateResult.Error });
        
        return NoContent();
    }

    /// <summary>
    /// Delete customer - Use EF Core for single entity deletion
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var result = await _customerService.DeleteCustomerAsync(id);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        
        return NoContent();
    }

    #endregion

    #region Business Operations - Use EF Core

    /// <summary>
    /// Get active customers with orders - Use EF Core for complex queries with navigation properties
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetActiveCustomers()
    {
        var result = await _customerService.GetActiveCustomersAsync();
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<IEnumerable<CustomerDto>>(result.Value));
    }

    /// <summary>
    /// Update customer statistics - Use EF Core for complex business logic
    /// </summary>
    [HttpPost("{id}/update-stats")]
    public async Task<IActionResult> UpdateCustomerStats(int id)
    {
        var result = await _customerService.UpdateCustomerStatsAsync(id);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        
        return NoContent();
    }

    #endregion

    #region Bulk Operations - Use Entity Framework Extensions

    /// <summary>
    /// Bulk create customers - Use Entity Framework Extensions for high-performance bulk operations
    /// </summary>
    [HttpPost("bulk-create")]
    public async Task<ActionResult<BulkOperationResult>> BulkCreateCustomers([FromBody] IEnumerable<CreateCustomerDto> createDtos)
    {
        var customers = _mapper.Map<IEnumerable<Customer>>(createDtos);
        var result = await _customerService.BulkCreateCustomersAsync(customers);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        
        return Ok(result.Value);
    }

    /// <summary>
    /// Bulk update customers - Use Entity Framework Extensions for bulk updates
    /// </summary>
    [HttpPut("bulk-update")]
    public async Task<ActionResult<BulkOperationResult>> BulkUpdateCustomers([FromBody] IEnumerable<CustomerDto> customerDtos)
    {
        var customers = _mapper.Map<IEnumerable<Customer>>(customerDtos);
        var result = await _customerService.BulkUpdateCustomersAsync(customers);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        
        return Ok(result.Value);
    }

    /// <summary>
    /// Bulk deactivate inactive customers - Use Entity Framework Extensions for batch operations
    /// </summary>
    [HttpPost("bulk-deactivate-inactive")]
    public async Task<ActionResult<int>> BulkDeactivateInactiveCustomers([FromQuery] int daysThreshold = 365)
    {
        var lastLoginThreshold = DateTime.UtcNow.AddDays(-daysThreshold);
        var result = await _customerService.BulkDeactivateInactiveCustomersAsync(lastLoginThreshold);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        
        return Ok(new { affectedRows = result.Value, message = "Successfully deactivated inactive customers using batch update" });
    }

    /// <summary>
    /// Bulk sync customers (upsert) - Use Entity Framework Extensions for bulk merge
    /// </summary>
    [HttpPost("bulk-sync")]
    public async Task<ActionResult<BulkOperationResult>> BulkSyncCustomers([FromBody] IEnumerable<CustomerDto> customerDtos)
    {
        var customers = _mapper.Map<IEnumerable<Customer>>(customerDtos);
        var result = await _customerService.BulkSyncCustomersAsync(customers);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        
        return Ok(result.Value);
    }

    /// <summary>
    /// Bulk synchronize customers - Use Entity Framework Extensions for complete data synchronization
    /// </summary>
    [HttpPost("bulk-synchronize")]
    public async Task<ActionResult<BulkOperationResult>> BulkSynchronizeCustomers([FromBody] IEnumerable<CustomerDto> customerDtos)
    {
        var customers = _mapper.Map<IEnumerable<Customer>>(customerDtos);
        var result = await _customerService.BulkSynchronizeCustomersAsync(customers);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        
        return Ok(result.Value);
    }

    #endregion

    #region Performance Comparison - Demonstrate EF Core vs Entity Framework Extensions

    /// <summary>
    /// Compare EF Core vs Entity Framework Extensions performance for insert operations
    /// </summary>
    [HttpPost("performance/insert-comparison")]
    public async Task<ActionResult<PerformanceComparisonDto>> CompareInsertPerformance([FromQuery] int count = 1000)
    {
        var efCoreResult = await _customerService.MeasureEfCoreInsertAsync(count);
        var bulkResult = await _customerService.MeasureBulkInsertAsync(count);
        
        if (efCoreResult.IsFailure || bulkResult.IsFailure)
        {
            var error = efCoreResult.IsFailure ? efCoreResult.Error : bulkResult.Error;
            return BadRequest(new { error });
        }

        var efCoreTime = efCoreResult.Value;
        var bulkTime = bulkResult.Value;
        var improvement = (efCoreTime.TotalMilliseconds - bulkTime.TotalMilliseconds) / efCoreTime.TotalMilliseconds * 100;
        
        var result = new PerformanceComparisonDto(
            count,
            efCoreTime,
            bulkTime,
            Math.Round(improvement, 2),
            "Insert"
        );
        
        return Ok(result);
    }

    /// <summary>
    /// Compare EF Core vs Entity Framework Extensions performance for update operations
    /// </summary>
    [HttpPost("performance/update-comparison")]
    public async Task<ActionResult<PerformanceComparisonDto>> CompareUpdatePerformance([FromQuery] int count = 1000)
    {
        var efCoreResult = await _customerService.MeasureEfCoreUpdateAsync(count);
        var bulkResult = await _customerService.MeasureBulkUpdateAsync(count);
        
        if (efCoreResult.IsFailure || bulkResult.IsFailure)
        {
            var error = efCoreResult.IsFailure ? efCoreResult.Error : bulkResult.Error;
            return BadRequest(new { error });
        }

        var efCoreTime = efCoreResult.Value;
        var bulkTime = bulkResult.Value;
        var improvement = (efCoreTime.TotalMilliseconds - bulkTime.TotalMilliseconds) / efCoreTime.TotalMilliseconds * 100;
        
        var result = new PerformanceComparisonDto(
            count,
            efCoreTime,
            bulkTime,
            Math.Round(improvement, 2),
            "Update"
        );
        
        return Ok(result);
    }

    #endregion
}
