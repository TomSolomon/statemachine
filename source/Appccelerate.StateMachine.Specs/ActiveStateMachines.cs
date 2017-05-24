﻿//-------------------------------------------------------------------------------
// <copyright file="ActiveStateMachines.cs" company="Appccelerate">
//   Copyright (c) 2008-2017 Appccelerate
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace Appccelerate.StateMachine
{
    using System.Threading;

    using Appccelerate.StateMachine.Machine;

    using FakeItEasy;

    using FluentAssertions;

    using Xbehave;

    public class ActiveStateMachines
    {
        [Scenario]
        public void DefaultStateMachineName(
            ActiveStateMachine<string, int> machine,
            StateMachineNameReporter reporter)
        {
            "establish an instantiated active state machine"._(() =>
            {
                machine = new ActiveStateMachine<string, int>();
            });

            "establish a state machine reporter"._(() =>
            {
                reporter = new StateMachineNameReporter();
            });

            "when the state machine report is generated"._(() =>
                machine.Report(reporter));

            "it should use the type of the state machine as name for state machine"._(() =>
                reporter.StateMachineName
                    .Should().Be("Appccelerate.StateMachine.ActiveStateMachine<System.String,System.Int32>"));
        }

        [Scenario]
        public void CustomStateMachineName(
            ActiveStateMachine<string, int> machine,
            StateMachineNameReporter reporter)
        {
            const string Name = "custom name";

            "establish an instantiated active state machine with custom name"._(() =>
            {
                machine = new ActiveStateMachine<string, int>(Name);
            });

            "establish a state machine reporter"._(() =>
            {
                reporter = new StateMachineNameReporter();
            });

            "when the state machine report is generated"._(() =>
                machine.Report(reporter));

            "it should use custom name for state machine"._(() =>
                reporter.StateMachineName
                    .Should().Be(Name));
        }

        [Scenario]
        public void CustomFactory(
            ActiveStateMachine<string, int> machine,
            StandardFactory<string, int> factory)
        {
            "establish a custom factory"._(() =>
            {
                factory = A.Fake<StandardFactory<string, int>>();
            });

            "when creating an active state machine"._(() =>
            {
                machine = new ActiveStateMachine<string, int>("_", factory);

                machine.In("initial").On(42).Goto("answer");
            });

            "it should use custom factory to create internal instances"._(() =>
                A.CallTo(factory).MustHaveHappened());
        }

        [Scenario]
        public void EventsQueueing(
            IStateMachine<string, int> machine,
            AutoResetEvent signal)
        {
            const int FirstEvent = 0;
            const int SecondEvent = 1;

            "establish an active state machine with transitions"._(() =>
            {
                signal = new AutoResetEvent(false);

                machine = new ActiveStateMachine<string, int>();

                machine.In("A").On(FirstEvent).Goto("B");
                machine.In("B").On(SecondEvent).Goto("C");
                machine.In("C").ExecuteOnEntry(() => signal.Set());

                machine.Initialize("A");
            });

            "when firing an event onto the state machine"._(() =>
            {
                machine.Fire(FirstEvent);
                machine.Fire(SecondEvent);
                machine.Start();
            });

            "it should queue event at the end"._(() =>
                signal.WaitOne(1000).Should().BeTrue("state machine should arrive at destination state"));
        }

        [Scenario]
        public void PriorityEventsQueueing(
            IStateMachine<string, int> machine,
            AutoResetEvent signal)
        {
            const int FirstEvent = 0;
            const int SecondEvent = 1;

            "establish an active state machine with transitions"._(() =>
            {
                signal = new AutoResetEvent(false);

                machine = new ActiveStateMachine<string, int>();

                machine.In("A").On(SecondEvent).Goto("B");
                machine.In("B").On(FirstEvent).Goto("C");
                machine.In("C").ExecuteOnEntry(() => signal.Set());

                machine.Initialize("A");
            });

            "when firing a priority event onto the state machine"._(() =>
            {
                machine.Fire(FirstEvent);
                machine.FirePriority(SecondEvent);
                machine.Start();
            });

            "it should queue event at the front"._(() =>
                signal.WaitOne(1000).Should().BeTrue("state machine should arrive at destination state"));
        }
    }
}