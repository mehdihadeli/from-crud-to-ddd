---
description: "This file provides guidelines for writing effective, maintainable tests using xUnit and Shouldly"
applyTo: "tests/**/*.cs"
---

## Role Definition:
- Test Engineer
- Quality Assurance Specialist

## General:

**Description:**
Tests should be reliable, maintainable, and provide meaningful coverage. Use xUnit as the primary testing framework with Shouldly for assertions, with proper isolation and clear patterns for test organization and execution.

**Requirements:**
- Use xUnit as the testing framework
- Use Shouldly library for all assertions
- Use xUnit's ITestOutputHelper for logging
- Use xUnit's IClassFixture and IAsyncLifetime for a shared state
- Use xUnit's Theory and MemberData for test data
- Use Bogus for generate mock data
- Use NSubstitute for mocking dependencies
- Ensure test isolation
- Follow consistent patterns
- Maintain high code coverage

## Test Class Structure:

- Use ITestOutputHelper for logging:
    ```csharp
    public class OrderProcessingTests(ITestOutputHelper output)
    {
        [Fact]
        public async Task ProcessOrder_ValidOrder_Succeeds()
        {
            output.WriteLine("Starting test with valid order");
            // Test implementation
        }
    }
    ```
- Use fixtures for shared state:
    ```csharp
    public class DatabaseFixture : IAsyncLifetime
    {
        public DbConnection Connection { get; private set; }
        
        public async Task InitializeAsync()
        {
            Connection = new SqlConnection("connection-string");
            await Connection.OpenAsync();
        }
        
        public async Task DisposeAsync()
        {
            await Connection.DisposeAsync();
        }
    }
    
    public class OrderTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly ITestOutputHelper _output;
        
        public OrderTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }
    }
    ```

## Test Methods:

- Prefer Theory over multiple Facts:
    ```csharp
    public class DiscountCalculatorTests
    {
        public static TheoryData<decimal, int, decimal> DiscountTestData => 
            new()
            {
                { 100m, 1, 0m },      // No discount for single item
                { 100m, 5, 5m },      // 5% for 5 items
                { 100m, 10, 10m },    // 10% for 10 items
            };
        
        [Theory]
        [MemberData(nameof(DiscountTestData))]
        public void CalculateDiscount_ReturnsCorrectAmount(
            decimal price,
            int quantity,
            decimal expectedDiscount)
        {
            // Arrange
            var calculator = new DiscountCalculator();
            
            // Act
            var discount = calculator.Calculate(price, quantity);
            
            // Assert
            discount.ShouldBe(expectedDiscount);
        }
    }
    ```
- Follow Arrange-Act-Assert pattern:
    ```csharp
    [Fact]
    public async Task ProcessOrder_ValidOrder_UpdatesInventory()
    {
        // Arrange
        var order = new Order(
            OrderId.New(),
            new[] { new OrderLine("SKU123", 5) });
        var processor = new OrderProcessor(_mockRepository.Object);
        
        // Act
        var result = await processor.ProcessAsync(order);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        _mockRepository.Verify(
            r => r.UpdateInventoryAsync(
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }
    ```

## Test Isolation:

- Use fresh data for each test:
    ```csharp
    public class OrderTests
    {
        private static Order CreateTestOrder() =>
            new(OrderId.New(), TestData.CreateOrderLines());
            
        [Fact]
        public async Task ProcessOrder_Success()
        {
            var order = CreateTestOrder();
            // Test implementation
        }
    }
    ```
- Clean up resources:
    ```csharp
    public class IntegrationTests : IAsyncDisposable
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        
        public IntegrationTests()
        {
            _server = new TestServer(CreateHostBuilder());
            _client = _server.CreateClient();
        }
        
        public async ValueTask DisposeAsync()
        {
            _client.Dispose();
            await _server.DisposeAsync();
        }
    }
    ```

## Best Practices:

- Name tests clearly:
    ```csharp
    // Good: Clear test names
    [Fact]
    public async Task ProcessOrder_WhenInventoryAvailable_UpdatesStockAndReturnsSuccess()
    
    // Avoid: Unclear names
    [Fact]
    public async Task TestProcessOrder()
    ```
- Use meaningful assertions:
    ```csharp
    // Good: Clear assertions with Shouldly
    actual.ShouldBe(expected);
    collection.ShouldContain(expectedItem);
    Should.Throw<OrderException>(() => processor.Process(invalidOrder));
    
    // Avoid: Multiple assertions without context
    result.ShouldNotBeNull();
    result.Success.ShouldBeTrue();
    result.Errors.Count.ShouldBe(0);
    ```
- Handle async operations properly:
    ```csharp
    // Good: Async test method
    [Fact]
    public async Task ProcessOrder_ValidOrder_Succeeds()
    {
        await using var processor = new OrderProcessor();
        var result = await processor.ProcessAsync(order);
        result.IsSuccess.ShouldBeTrue();
    }
    
    // Avoid: Sync over async
    [Fact]
    public void ProcessOrder_ValidOrder_Succeeds()
    {
        using var processor = new OrderProcessor();
        var result = processor.ProcessAsync(order).Result;  // Can deadlock
        result.IsSuccess.ShouldBeTrue();
    }
    ```
- Use `TestContext.Current.CancellationToken` for cancellation:
    ```csharp
    // Good:
    [Fact]
    public async Task ProcessOrder_CancellationRequested()
    {
        await using var processor = new OrderProcessor();
        var result = await processor.ProcessAsync(order, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }
    // Avoid:
    [Fact]
    public async Task ProcessOrder_CancellationRequested()
    {
        await using var processor = new OrderProcessor();
        var result = await processor.ProcessAsync(order, CancellationToken.None);
        result.IsSuccess.ShouldBeFalse();
    }
    ```

## Assertions:

- Use Shouldly for all assertions:
    ```csharp
    public class OrderTests
    {
        [Fact]
        public void CalculateTotal_WithValidLines_ReturnsCorrectSum()
        {
            // Arrange
            var order = new Order(
                OrderId.New(),
                new[]
                {
                    new OrderLine("SKU1", 2, 10.0m),
                    new OrderLine("SKU2", 1, 20.0m)
                });
            
            // Act
            var total = order.CalculateTotal();
            
            // Assert
            total.ShouldBe(40.0m);
        }
        
        [Fact]
        public void Order_WithInvalidLines_ThrowsException()
        {
            // Arrange
            var invalidLines = new OrderLine[] { };
            
            // Act & Assert
            var ex = Should.Throw<ArgumentException>(() =>
                new Order(OrderId.New(), invalidLines));
            ex.Message.ShouldBe("Order must have at least one line");
        }
        
        [Fact]
        public void Order_WithValidData_HasExpectedProperties()
        {
            // Arrange
            var id = OrderId.New();
            var lines = new[] { new OrderLine("SKU1", 1, 10.0m) };
            
            // Act
            var order = new Order(id, lines);
            
            // Assert
            order.ShouldNotBeNull();
            order.Id.ShouldBe(id);
            order.Lines.ShouldHaveSingleItem();
            order.Lines[0].Sku.ShouldBe("SKU1");
            order.Lines[0].Quantity.ShouldBe(1);
            order.Lines[0].Price.ShouldBe(10.0m);
        }
    }
    ```
    
- Use Shouldly's fluent assertions:
    ```csharp
    public class CustomerTests
    {
        [Fact]
        public void Customer_WithValidEmail_IsCreated()
        {
            // Boolean assertions
            customer.IsActive.ShouldBeTrue();
            customer.IsDeleted.ShouldBeFalse();
            
            // Equality assertions
            customer.Email.ShouldBe("john@example.com");
            customer.Id.ShouldNotBe(Guid.Empty);
            
            // Collection assertions
            customer.Orders.ShouldBeEmpty();
            customer.Roles.ShouldContain("Admin");
            customer.Roles.ShouldNotContain("Guest");
            customer.Orders.ShouldAllBe(o => o.Id != null);
            
            // Type assertions
            customer.ShouldBeOfType<PremiumCustomer>();
            customer.ShouldBeAssignableTo<ICustomer>();
            
            // String assertions
            customer.Reference.ShouldStartWith("CUST");
            customer.Description.ShouldContain("Premium");
            customer.Reference.ShouldMatch("^CUST\\d{6}$");
            
            // Range assertions
            customer.Age.ShouldBeInRange(18, 100);
            
            // Reference assertions
            actualCustomer.ShouldBeSameAs(expectedCustomer);
            actualCustomer.ShouldNotBeSameAs(differentCustomer);
        }
    }
    ```
    
- Use Shouldly's collection assertions:
    ```csharp
    [Fact]
    public void ProcessOrder_CreatesExpectedEvents()
    {
        // Arrange
        var processor = new OrderProcessor();
        var order = CreateTestOrder();
        
        // Act
        var events = processor.Process(order);
        
        // Assert
        events.Count.ShouldBe(3);
        events[0].ShouldBeOfType<OrderReceivedEvent>()
            .OrderId.ShouldBe(order.Id);
        events[1].ShouldBeOfType<InventoryReservedEvent>()
            .OrderId.ShouldBe(order.Id);
        events[1].ShouldBeOfType<InventoryReservedEvent>()
            .ReservedItems.ShouldNotBeEmpty();
        events[2].ShouldBeOfType<OrderConfirmedEvent>()
            .OrderId.ShouldBe(order.Id);
        events[2].ShouldBeOfType<OrderConfirmedEvent>()
            .IsSuccess.ShouldBeTrue();
    }
    ```


## Create Fake objetcs

For creating fake objects use `Bogus` library and for mocking use `NSubstitute` library:

Here is a sample of using `Bogus`:
```csharp
public sealed class CreateBookCommandFaker : Faker<CreateBookCommand>
{
    public CreateBookCommandFaker()
    {
        CustomInstantiator(f =>
            new(
                f.Commerce.ProductName(),
                f.Lorem.Paragraph(),
                null,
                f.Finance.Amount(100),
                f.Finance.Amount(1, 99),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                [Guid.CreateVersion7()]
            )
        );
    }

    public CreateBookCommand Generate()
    {
        return Generate(1)[0];
    }
}
```

## 🧪 BDD-Style Test Naming Convention

### Format
Use the following structure for naming unit or integration tests in a behavior-driven style:


**Given[Precondition]_When[Action]_Then[ExpectedBehavior]**


This format emphasizes clarity and expresses the behavior being tested, not the implementation.


### ✅ Examples

- `GivenValidParameters_WhenCreatingBook_ThenShouldInitializeCorrectly`
- `GivenInvalidUserId_WhenFetchingProfile_ThenShouldReturnNotFound`
- `GivenEmptyCart_WhenCheckout_ThenShouldThrowValidationException`


### 💡 Naming Guidelines

- Use **PascalCase** for all words.
- Separate `Given`, `When`, and `Then` segments with **underscores** for readability.
- Keep the name descriptive but **not overly long**.
- Use **domain language**, not technical jargon (e.g., "CreatingBook", not "CallingConstructor").
- Use **"Should"** in the `Then` clause to clearly indicate expected behavior.
- Ensure consistency across all test names in the project.

