using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MeetupEvents.Infrastructure
{
    public static class EventsDispatcherExtensions
    {
        public static void AddEventsDispatcher(this IServiceCollection serviceCollection, Type type)
        {
            var registry = new EventHandlerRegistry();
            RegisterHandlers(type.Assembly);

            serviceCollection.AddScoped(sp => new EventsDispatcher(sp, registry));

            void RegisterHandlers(params Assembly[] assemblies)
            {
                foreach (var (message, wrapper) in GetTypesFromAssembly(typeof(IEventHandler<>)))
                {
                    registry.Add(message, wrapper);
                    serviceCollection.AddScoped(wrapper);
                }

                IEnumerable<(Type message, Type wrapper)> GetTypesFromAssembly(Type interfaceType) =>
                    from ti in assemblies.SelectMany(a => a.DefinedTypes)
                    where ti.IsClass && !ti.IsAbstract && !ti.IsInterface
                    from i in ti.ImplementedInterfaces
                    where i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == interfaceType
                    select
                    (
                        messageType: i.GenericTypeArguments.First(),
                        wrapperType: ti.AsType()
                    );
            }
        }
    }

    public interface IEventHandler<in TMessage>
    {
        Task Handle(TMessage created);
    }

    public class EventsDispatcher
    {
        readonly IServiceProvider     _serviceProvider;
        readonly EventHandlerRegistry _registry;

        public EventsDispatcher(IServiceProvider sp, EventHandlerRegistry registry)
        {
            _serviceProvider = sp;
            _registry        = registry;
        }

        public async Task Publish(object domainEvent)
        {
            if (_registry.TryGetValue(domainEvent.GetType(), out var handlers))
            {
                foreach (var handler in
                    handlers.Select(handlerType => _serviceProvider.GetRequiredService(handlerType)))
                    await ((dynamic) handler).Handle((dynamic) domainEvent);
            }
        }
    }

    public class EventHandlerRegistry : Dictionary<Type, List<Type>>
    {
        public void Add(Type domainEvent, Type handlerType)
        {
            var observed = ContainsKey(domainEvent);
            if (!observed)
                Add(domainEvent, new List<Type> {handlerType});
            else
                this[domainEvent].Add(handlerType);
        }
    }
}