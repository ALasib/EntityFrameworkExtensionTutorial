# Entity Framework Extensions Performance Demo Script
# This script demonstrates the performance differences between EF Core and Entity Framework Extensions

param(
    [string]$BaseUrl = "https://localhost:7000",
    [int]$EntityCount = 1000
)

Write-Host "🚀 Entity Framework Extensions Performance Demo" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host "Entity Count: $EntityCount" -ForegroundColor Yellow
Write-Host ""

# Function to make HTTP requests
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null
    )
    
    try {
        $headers = @{
            "Content-Type" = "application/json"
        }
        
        if ($Body) {
            $jsonBody = $Body | ConvertTo-Json -Depth 10
            $response = Invoke-RestMethod -Uri "$BaseUrl$Endpoint" -Method $Method -Headers $headers -Body $jsonBody
        } else {
            $response = Invoke-RestMethod -Uri "$BaseUrl$Endpoint" -Method $Method -Headers $headers
        }
        
        return $response
    }
    catch {
        Write-Host "❌ Error calling $Endpoint : $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Test basic CRUD operations (EF Core)
Write-Host "📋 Testing Basic CRUD Operations (EF Core)..." -ForegroundColor Cyan

# Get all customers
Write-Host "  Getting all customers..." -ForegroundColor White
$customers = Invoke-ApiRequest -Method "GET" -Endpoint "/api/customer"
if ($customers) {
    Write-Host "  ✅ Found $($customers.Count) customers" -ForegroundColor Green
}

# Get customer by ID
if ($customers -and $customers.Count -gt 0) {
    Write-Host "  Getting customer by ID..." -ForegroundColor White
    $customer = Invoke-ApiRequest -Method "GET" -Endpoint "/api/customer/$($customers[0].id)"
    if ($customer) {
        Write-Host "  ✅ Retrieved customer: $($customer.name)" -ForegroundColor Green
    }
}

Write-Host ""

# Test bulk operations (Entity Framework Extensions)
Write-Host "🚀 Testing Bulk Operations (Entity Framework Extensions)..." -ForegroundColor Cyan

# Generate test data for bulk operations
$testCustomers = @()
for ($i = 1; $i -le 100; $i++) {
    $testCustomers += @{
        code = "BULK$($i.ToString('D6'))"
        name = "Bulk Customer $i"
        email = "bulk$i@example.com"
        phone = "+1-555-123-4567"
    }
}

# Bulk create customers
Write-Host "  Bulk creating $($testCustomers.Count) customers..." -ForegroundColor White
$bulkCreateResult = Invoke-ApiRequest -Method "POST" -Endpoint "/api/customer/bulk-create" -Body $testCustomers
if ($bulkCreateResult) {
    Write-Host "  ✅ $($bulkCreateResult.message)" -ForegroundColor Green
}

Write-Host ""

# Test performance comparison
Write-Host "📊 Testing Performance Comparison..." -ForegroundColor Cyan

# Compare insert performance
Write-Host "  Comparing insert performance for $EntityCount entities..." -ForegroundColor White
$insertComparison = Invoke-ApiRequest -Method "POST" -Endpoint "/api/customer/performance/insert-comparison?count=$EntityCount"
if ($insertComparison) {
    Write-Host "  📈 Insert Performance Results:" -ForegroundColor Yellow
    Write-Host "    Entity Count: $($insertComparison.entityCount)" -ForegroundColor White
    Write-Host "    EF Core Time: $($insertComparison.efCoreTime)" -ForegroundColor Red
    Write-Host "    Bulk Time: $($insertComparison.bulkTime)" -ForegroundColor Green
    Write-Host "    Performance Improvement: $($insertComparison.performanceImprovement)%" -ForegroundColor Green
}

Write-Host ""

# Compare update performance
Write-Host "  Comparing update performance for $EntityCount entities..." -ForegroundColor White
$updateComparison = Invoke-ApiRequest -Method "POST" -Endpoint "/api/customer/performance/update-comparison?count=$EntityCount"
if ($updateComparison) {
    Write-Host "  📈 Update Performance Results:" -ForegroundColor Yellow
    Write-Host "    Entity Count: $($updateComparison.entityCount)" -ForegroundColor White
    Write-Host "    EF Core Time: $($updateComparison.efCoreTime)" -ForegroundColor Red
    Write-Host "    Bulk Time: $($updateComparison.bulkTime)" -ForegroundColor Green
    Write-Host "    Performance Improvement: $($updateComparison.performanceImprovement)%" -ForegroundColor Green
}

Write-Host ""
Write-Host "🎉 Performance Demo Completed!" -ForegroundColor Green
Write-Host ""
Write-Host "💡 Key Takeaways:" -ForegroundColor Yellow
Write-Host "  • Use EF Core for simple CRUD operations and complex business logic" -ForegroundColor White
Write-Host "  • Use Entity Framework Extensions for bulk operations and large datasets" -ForegroundColor White
Write-Host "  • Performance improvements can be significant (up to 93% faster)" -ForegroundColor White
Write-Host ""
Write-Host "📚 For more information, check the README.md file" -ForegroundColor Cyan
