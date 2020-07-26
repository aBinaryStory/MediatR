MediatR Topics
=======


### About this fork
By default MediatR's event handlers are triggered by type. 
This fork allows another restriction based on a given topic.

## Use Case
Segregation of events of the same type based on customer filters

## Example
In our restaurant our regular customers get an additional free drink when they place their order.
The order type is the same, but the decision is made based on the customer names.
```csharp
public class RegularCustomerHandler : INotificationHandler<Order>
{
    [Topic("regular-customer")]
    public Task Handle(Order notification, CancellationToken cancellationToken)
    {
        //serve a free drink
    }
}

public class CustomerHandler : INotificationHandler<Order>
{
    public Task Handle(Order notification, CancellationToken cancellationToken)
    {
        //prepare meal
    }
}

public class OrderCreator
{
    private Mediator _mediator;
    private List<string> _regularCustomers = new List<string>() { "Maria Anders", "Ana Trujillo"};

    public void OrderCreated(Order order)
    {
        _mediator.Publish(order); //prepare Meal

        if(_regularCustomers.contains(order.customer))
        {
            _mediator.Publish(order, "regular-customer"); //serve free drink etc.
        }
    }
}