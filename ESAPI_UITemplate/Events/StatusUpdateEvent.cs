using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESAPI_UITemplate.Events
{
    //Events can be applied to a class in 2 ways: Publisher or Subscriber.
    //The publisher notifes the app that an event has taken place.
    //The subscriber can run some method when it hears the event.
    public class StatusUpdateEvent:PubSubEvent<string>
    {
    }
}
