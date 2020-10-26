namespace MediatR.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class NotificationHandlerWrapper
    {
        public abstract Task Handle(INotification notification, string topic, CancellationToken cancellationToken, ServiceFactory serviceFactory,
                                    Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, string, CancellationToken, Task> publish);
    }

    internal class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
        where TNotification : INotification
    {
        public override Task Handle(INotification notification, string topic, CancellationToken cancellationToken, ServiceFactory serviceFactory,
                                    Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, string, CancellationToken, Task> publish)
        {
            List<Func<INotification, CancellationToken, Task>>? handlers = new List<Func<INotification, CancellationToken, Task>>();

            var handlersOFType = serviceFactory
           .GetInstances<INotificationHandler<TNotification>>();
            //Limitation von covariant IoC Containern:
            //man kann nicht beides INotificationhandler<Base> & INotificationhandler<Spezific> implementieren
            //1. es wird immer INotificationhandler<Base> aufgerufen
            //2. Die TOpicMethode kann nicht mehr eindeutig zugeordnet werden
            foreach (var h in handlersOFType)
            {
                var methods = h.GetType().GetMethods();
                foreach (var m in methods)
                {
                    if (m.Name == nameof(INotificationHandler<TNotification>.Handle))
                    {
                        if(m.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(TNotification)))
                        {
                            if(topic == "")
                            {
                                if(m.GetCustomAttribute<TopicAttribute>() == null)
                                {
                                    handlers.Add(new Func<INotification, CancellationToken, Task>((theNotification, theToken) => h.Handle((TNotification) theNotification, theToken)));
                                }
                            }
                            else
                            {
                                if (m.GetCustomAttribute<TopicAttribute>()?.Topic == topic)
                                {
                                    handlers.Add(new Func<INotification, CancellationToken, Task>((theNotification, theToken) => h.Handle((TNotification) theNotification, theToken)));
                                }
                            }
                        }
                    }
                }
            }

            return publish(handlers.ToList(), notification, topic, cancellationToken);
        }
    }
}