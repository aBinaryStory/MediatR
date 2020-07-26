using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediatR
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TopicAttribute : Attribute
    {
        public string Topic { get; }
        public TopicAttribute(string topic)
        {
            Topic = topic;
        }
    }
}
