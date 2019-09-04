//-------------------------------------------------------------------------------
// <copyright file="HierarchicalTransitions.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Specs.Sync
{
    using System;
    using System.Globalization;
    using System.Linq;
    using FluentAssertions;
    using Machine;
    using Xbehave;

    public class HierarchicalTransitions
    {
        [Scenario]
        public void NoCommonAncestor(
            PassiveStateMachine<string, int> machine)
        {
            const string SourceState = "SourceState";
            const string ParentOfSourceState = "ParentOfSourceState";
            const string SiblingOfSourceState = "SiblingOfSourceState";
            const string DestinationState = "DestinationState";
            const string ParentOfDestinationState = "ParentOfDestinationState";
            const string SiblingOfDestinationState = "SiblingOfDestinationState";
            const string GrandParentOfSourceState = "GrandParentOfSourceState";
            const string GrandParentOfDestinationState = "GrandParentOfDestinationState";
            const int Event = 0;

            var log = string.Empty;

            "establish a hierarchical state machine".x(() =>
            {
                machine = new StateMachineDefinitionBuilder<string, int>()
                    .WithConfiguration(x =>
                        x.DefineHierarchyOn(ParentOfSourceState)
                            .WithHistoryType(HistoryType.None)
                            .WithInitialSubState(SourceState)
                            .WithSubState(SiblingOfSourceState))
                    .WithConfiguration(x =>
                        x.DefineHierarchyOn(ParentOfDestinationState)
                            .WithHistoryType(HistoryType.None)
                            .WithInitialSubState(DestinationState)
                            .WithSubState(SiblingOfDestinationState))
                    .WithConfiguration(x =>
                        x.DefineHierarchyOn(GrandParentOfSourceState)
                            .WithHistoryType(HistoryType.None)
                            .WithInitialSubState(ParentOfSourceState))
                    .WithConfiguration(x =>
                        x.DefineHierarchyOn(GrandParentOfDestinationState)
                            .WithHistoryType(HistoryType.None)
                            .WithInitialSubState(ParentOfDestinationState))
                    .WithConfiguration(x =>
                        x.In(SourceState)
                            .ExecuteOnExit(() => log += "exit" + SourceState)
                            .On(Event).Goto(DestinationState))
                    .WithConfiguration(x =>
                        x.In(ParentOfSourceState)
                            .ExecuteOnExit(() => log += "exit" + ParentOfSourceState))
                    .WithConfiguration(x =>
                        x.In(DestinationState)
                            .ExecuteOnEntry(() => log += "enter" + DestinationState))
                    .WithConfiguration(x =>
                        x.In(ParentOfDestinationState)
                            .ExecuteOnEntry(() => log += "enter" + ParentOfDestinationState))
                    .WithConfiguration(x =>
                        x.In(GrandParentOfSourceState)
                            .ExecuteOnExit(() => log += "exit" + GrandParentOfSourceState))
                    .WithConfiguration(x =>
                        x.In(GrandParentOfDestinationState)
                            .ExecuteOnEntry(() => log += "enter" + GrandParentOfDestinationState))
                    .Build()
                    .CreatePassiveStateMachine();

                machine.Initialize(SourceState);
                machine.Start();
            });

            "when firing an event resulting in a transition without a common ancestor".x(() =>
                machine.Fire(Event));

            "it should execute exit action of source state".x(() =>
                log.Should().Contain("exit" + SourceState));

            "it should execute exit action of parents of source state (recursively)".x(() =>
                log
                    .Should().Contain("exit" + ParentOfSourceState)
                    .And.Contain("exit" + GrandParentOfSourceState));

            "it should execute entry action of parents of destination state (recursively)".x(() =>
                log
                    .Should().Contain("enter" + ParentOfDestinationState)
                    .And.Contain("enter" + GrandParentOfDestinationState));

            "it should execute entry action of destination state".x(() =>
                log.Should().Contain("enter" + DestinationState));

            "it should execute actions from source upwards and then downwards to destination state".x(() =>
            {
                string[] states =
                {
                    SourceState,
                    ParentOfSourceState,
                    GrandParentOfSourceState,
                    GrandParentOfDestinationState,
                    ParentOfDestinationState,
                    DestinationState
                };

                var statesInOrderOfAppearanceInLog = states
                    .OrderBy(s => log.IndexOf(s.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
                statesInOrderOfAppearanceInLog
                    .Should().Equal(states);
            });
        }

        [Scenario]
        public void CommonAncestor(
            PassiveStateMachine<int, int> machine)
        {
            const int CommonAncestorState = 0;
            const int SourceState = 1;
            const int ParentOfSourceState = 2;
            const int SiblingOfSourceState = 3;
            const int DestinationState = 4;
            const int ParentOfDestinationState = 5;
            const int SiblingOfDestinationState = 6;
            const int Event = 0;

            var commonAncestorStateLeft = false;

            "establish a hierarchical state machine".x(() =>
            {
                machine = new StateMachineDefinitionBuilder<int, int>()
                    .WithConfiguration(x =>
                        x.DefineHierarchyOn(CommonAncestorState)
                            .WithHistoryType(HistoryType.None)
                            .WithInitialSubState(ParentOfSourceState)
                            .WithSubState(ParentOfDestinationState))
                    .WithConfiguration(x =>
                        x.DefineHierarchyOn(ParentOfSourceState)
                            .WithHistoryType(HistoryType.None)
                            .WithInitialSubState(SourceState)
                            .WithSubState(SiblingOfSourceState))
                    .WithConfiguration(x =>
                        x.DefineHierarchyOn(ParentOfDestinationState)
                            .WithHistoryType(HistoryType.None)
                            .WithInitialSubState(DestinationState)
                            .WithSubState(SiblingOfDestinationState))
                    .WithConfiguration(x =>
                        x.In(SourceState)
                            .On(Event).Goto(DestinationState))
                    .WithConfiguration(x =>
                        x.In(CommonAncestorState)
                            .ExecuteOnExit(() => commonAncestorStateLeft = true))
                    .Build()
                    .CreatePassiveStateMachine();

                machine.Initialize(SourceState);
                machine.Start();
            });

            "when firing an event resulting in a transition with a common ancestor".x(() =>
                machine.Fire(Event));

            "the state machine should remain inside common ancestor state".x(() =>
                commonAncestorStateLeft
                    .Should().BeFalse());
        }
    }
}