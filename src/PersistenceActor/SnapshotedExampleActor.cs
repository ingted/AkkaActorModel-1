﻿using System;
using Akka.Persistence;
using PersistenceActor;

namespace PersistenceExample
{
    public class SnapshotedExampleActor : PersistentActor
    {
        public SnapshotedExampleActor()
        {
            State = new ExampleState();
        }

        public override string PersistenceId { get { return "sample-id-3"; } }

        public ExampleState State { get; set; }

        protected override bool ReceiveRecover(object message)
        {
            if (message is SnapshotOffer)
            {
                var s = ((SnapshotOffer) message).Snapshot as ExampleState;
                Console.WriteLine("Offered state (from snapshot): " + s);
                State = s;
            }
            else if (message is string)
                State = State.Update(new Event(message.ToString()));
            else return false;
            return true;
        }

        protected override bool ReceiveCommand(object message)
        {
            if (message == "print")
                Console.WriteLine("Current actor's state: " + State);
            else if (message == "snap")
                SaveSnapshot(State);
            else if (message is SaveSnapshotFailure || message is SaveSnapshotSuccess) { }
            else if (message is string)
                Persist(message.ToString(), evt => State = State.Update(new Event(evt)));
            else return false;
            return true;
        }
    }
}
