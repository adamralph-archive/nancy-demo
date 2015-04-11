namespace NancyDemo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Nancy;
    using Nancy.Hosting.Self;
    using Nancy.Responses;
    using Nancy.Routing.Constraints;

    public class Program
    {
        private static void Main()
        {
            var uri = new Uri("http://localhost:8888");
            new NancyHost(uri).Start();
            Console.WriteLine("Nancy demo running at {0}", uri);
            Thread.Sleep(Timeout.Infinite);
        }

        public class SpeakToModule : NancyModule
        {
            public SpeakToModule(ISpeaker speaker)
            {
                Get["/speak-to/{listener:person}"] =
                    _ => speaker.SpeakTo(_.listener.Value);
            }
        }

        public interface ISpeaker
        {
            Message SpeakTo(Person person);
        }

        public class Message
        {
            public Message()
            {
                Timestamp = DateTime.UtcNow;
            }

            public string Text { get; set; }

            public DateTime Timestamp { get; private set; }
        }

        public class NancySpeaker : ISpeaker
        {
            public Message SpeakTo(Person person)
            {
                return new Message
                {
                    Text = string.Format("Hello {0}, Nancy is great!", person),
                };
            }
        }

        public class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(LastName)
                    ? FirstName
                    : string.Concat(FirstName, " ", LastName);
            }
        }

        public class PersonConstraint : RouteSegmentConstraintBase<Person>
        {
            protected override bool TryMatch(
                string constraint, string segment, out Person matchedValue)
            {
                var names = segment.Split(new[] { '-' }, 2);
                matchedValue = new Person
                {
                    FirstName = names[0],
                    LastName = names.Length == 1 ? null : names[1],
                };

                return true;
            }

            public override string Name
            {
                get { return "person"; }
            }
        }

        public class ArrayAllTheThingsJsonSerializer : ISerializer
        {
            private readonly DefaultJsonSerializer serializer;

            public ArrayAllTheThingsJsonSerializer(DefaultJsonSerializer serializer)
            {
                this.serializer = serializer;
            }

            public IEnumerable<string> Extensions
            {
                get { return serializer.Extensions; }
            }

            public bool CanSerialize(string contentType)
            {
                return serializer.CanSerialize(contentType);
            }

            public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
            {
                outputStream.Write(new[] { (byte)'[' }, 0, 1);
                serializer.Serialize(contentType, model, outputStream);
                outputStream.Write(new[] { (byte)']' }, 0, 1);
            }
        }
    }
}
