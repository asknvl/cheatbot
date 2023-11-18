using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.ViewModels.events
{
    public class EventAggregator : IEventAggregator
    {

        public static EventAggregator instance;
        private EventAggregator()
        {

        }
        public static EventAggregator getInstance()
        {
            if (instance == null)
                instance = new EventAggregator();
            return instance;
        }


        private readonly Dictionary<Type, List<object>> _subscribers = new Dictionary<Type, List<object>>();

        public void Subscribe(object subscriber)
        {
            var subscriberTypes = subscriber.GetType().GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventSubscriber<>));

            foreach (var subscriberType in subscriberTypes)
            {
                var messageType = subscriberType.GetGenericArguments()[0];
                if (!_subscribers.ContainsKey(messageType))
                {
                    _subscribers[messageType] = new List<object>();
                }

                if (!_subscribers[messageType].Contains(subscriber))
                {
                    _subscribers[messageType].Add(subscriber);
                }
            }
        }

        public void Unsubscribe(object subscriber)
        {
            foreach (var messageType in _subscribers.Keys.ToList())
            {
                _subscribers[messageType].Remove(subscriber);
            }
        }

        public void Publish<TMessage>(TMessage message) where TMessage : class
        {
            var messageType = typeof(TMessage);
            if (!_subscribers.ContainsKey(messageType)) return;

            foreach (var subscriber in _subscribers[messageType].ToList())
            {
                if (subscriber is IEventSubscriber<TMessage> typedSubscriber)
                {
                    typedSubscriber.OnEvent(message);
                }
            }
        }
    }
}
