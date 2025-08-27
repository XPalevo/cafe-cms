namespace TestB;

using Common.Services.EventBroker.Core.Ports;

public class ServiceB
{
    private readonly IEventBroker _eventBroker;
    private readonly IEventHandlerRouter _router;
    
    public ServiceB(IEventBroker eventBroker, IEventHandlerRouter router)
    {
        _eventBroker = eventBroker;
        _router = router;
    }

    public void GetOrder()
    {
        _eventBroker.Subscribe("orders", _router.RouteAsync);
    }
}