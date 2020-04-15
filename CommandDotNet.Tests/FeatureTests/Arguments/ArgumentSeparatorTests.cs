using CommandDotNet.Rendering;
using CommandDotNet.TestTools.Scenarios;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CommandDotNet.Tests.FeatureTests.Arguments
{
    public class ArgumentSeparatorTests
    {
        private readonly ITestOutputHelper _output;

        private readonly AppSettings _appSettingsEndOfOptions = new AppSettings
        {
            DefaultArgumentSeparatorStrategy = ArgumentSeparatorStrategy.EndOfOptions
        };

        private readonly AppSettings _appSettingsPassThru = new AppSettings
        {
            DefaultArgumentSeparatorStrategy = ArgumentSeparatorStrategy.PassThru
        };

        public ArgumentSeparatorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Given_PassThru_When_OperandValueWithDash_FailsWithUnrecognizedOption()
        {
            new AppRunner<Math>(_appSettingsPassThru)
                .VerifyScenario(_output, new Scenario
                {
                    WhenArgs = "Add -1 -3",
                    Then =
                    {
                        ExitCode = 1,
                        ResultsContainsTexts = { "Unrecognized option '-1'" }
                    }
                });
        }

        [Fact]
        public void Given_EndOfOptions_ByAppSetting_When_OperandValueWithDash_FailsWithUnrecognizedOption()
        {
            new AppRunner<Math>(_appSettingsEndOfOptions)
                .VerifyScenario(_output, new Scenario
                {
                    WhenArgs = "Add -1 -3",
                    Then =
                    {
                        ExitCode = 1,
                        ResultsContainsTexts = { "Unrecognized option '-1'" }
                    }
                });
        }

        [Fact]
        public void Given_EndOfOptions_ByCommand_When_OperandValueWithDash_FailsWithUnrecognizedOption()
        {
            new AppRunner<Math>(_appSettingsPassThru)
                .VerifyScenario(_output, new Scenario
                {
                    WhenArgs = "Add_EndOfOptions -1 -3",
                    Then =
                    {
                        ExitCode = 1,
                        ResultsContainsTexts = { "Unrecognized option '-1'" }
                    }
                });
        }

        [Fact]
        public void Given_PassThru_ByAppSetting_When_Separator_OperandValueWithDash_OperandsAreIgnored()
        {
            var result = new AppRunner<Math>(_appSettingsPassThru)
                .VerifyScenario(_output, new Scenario
                {
                    WhenArgs = "Add -- -1 -3",
                    Then = { Result = "0" }
                });

            result.CommandContext.ParseResult.SeparatedArguments
                .Should().BeEquivalentTo(new[] { "-1", "-3" });
        }

        [Fact]
        public void Given_PassThru_ByCommand_When_Separator_OperandValueWithDash_OperandsAreIgnored()
        {
            var result = new AppRunner<Math>(_appSettingsEndOfOptions)
                .VerifyScenario(_output, new Scenario
                {
                    WhenArgs = "Add_PassThru -- -1 -3",
                    Then = { Result = "0" }
                });

            result.CommandContext.ParseResult.SeparatedArguments
                .Should().BeEquivalentTo(new[] { "-1", "-3" });
        }

        [Fact]
        public void Given_EndOfOptions_ByAppSetting_When_Separator_OperandValueWithDash_OperandsAreParsed()
        {
            var result = new AppRunner<Math>(_appSettingsEndOfOptions)
                .VerifyScenario(_output, new Scenario
                {
                    WhenArgs = "Add -- -1 -3",
                    Then = { Result = "-4" }
                });

            result.CommandContext.ParseResult.SeparatedArguments
                .Should().BeEquivalentTo(new[] { "-1", "-3" });
        }

        [Fact]
        public void Given_EndOfOptions_ByCommand_When_Separator_OperandValueWithDash_OperandsAreParsed()
        {
            var result = new AppRunner<Math>(_appSettingsPassThru)
                .VerifyScenario(_output, new Scenario
                {
                    WhenArgs = "Add_EndOfOptions -- -1 -3",
                    Then = { Result = "-4" }
                });

            result.CommandContext.ParseResult.SeparatedArguments
                .Should().BeEquivalentTo(new[] { "-1", "-3" });
        }

        [Fact]
        public void Given_EndOfOptions_ByAppSetting_And_IgnoreUnexpected_Enabled_When_Separator_OperandValueWithDash_OperandsAreParsed_And_ExtraArgsAreIgnoredAndCaptured()
        {
            var appSettings = _appSettingsEndOfOptions.Clone(s => s.IgnoreUnexpectedOperands = true);

            var result = new AppRunner<Math>(appSettings)
                .VerifyScenario(_output, new Scenario
                {
                    WhenArgs = "Add -- -1 -3 -5 -7",
                    Then = { Result = "-4" }
                });

            result.CommandContext.ParseResult.RemainingOperands
                .Should().BeEquivalentTo(new[] { "-5", "-7" });

            result.CommandContext.ParseResult.SeparatedArguments
                .Should().BeEquivalentTo(new[] { "-1", "-3", "-5", "-7" });
        }

        [Fact]
        public void Given_EndOfOptions_ByCommand_And_IgnoreUnexpected_Enabled_When_Separator_OperandValueWithDash_OperandsAreParsed_And_ExtraArgsAreIgnoredAndCaptured()
        {
            var appSettings = _appSettingsPassThru.Clone(s => s.IgnoreUnexpectedOperands = true);

            var result = new AppRunner<Math>(appSettings)
                .VerifyScenario(_output, new Scenario
                {
                    WhenArgs = "Add_EndOfOptions -- -1 -3 -5 -7",
                    Then = { Result = "-4" }
                });

            result.CommandContext.ParseResult.RemainingOperands
                .Should().BeEquivalentTo(new[] { "-5", "-7" });

            result.CommandContext.ParseResult.SeparatedArguments
                .Should().BeEquivalentTo(new[] { "-1", "-3", "-5", "-7" });
        }

        [Fact]
        public void Help_Alternative2_IsValid()
        {
            // test examples in the help documentation

            var appSettings = _appSettingsEndOfOptions.Clone(s => s.IgnoreUnexpectedOperands = true);

            var result = new AppRunner<Math>(appSettings)
                .VerifyScenario(_output, new Scenario
                {
                    WhenArgs = "Add -- -1 -3 -- -5 -7",
                    Then = { Result = "-4" }
                });

            result.CommandContext.ParseResult.RemainingOperands
                .Should().BeEquivalentTo(new[] { "--", "-5", "-7" });

            result.CommandContext.ParseResult.SeparatedArguments
                .Should().BeEquivalentTo(new[] { "-1", "-3", "--", "-5", "-7" });
        }

        [Fact]
        public void Help_Alternative3_IsValid()
        {
            // test examples in the help documentation

            var appSettings = _appSettingsEndOfOptions.Clone(s => s.IgnoreUnexpectedOperands = true);

            var result = new AppRunner<Math>(appSettings)
                .VerifyScenario(_output, new Scenario
                {
                    WhenArgs = "Add -- -1 -3 __ -5 -7",
                    Then = { Result = "-4" }
                });

            result.CommandContext.ParseResult.RemainingOperands
                .Should().BeEquivalentTo(new[] { "__", "-5", "-7" });

            result.CommandContext.ParseResult.SeparatedArguments
                .Should().BeEquivalentTo(new[] { "-1", "-3", "__", "-5", "-7" });
        }

        public class Math
        {
            public void Add(IConsole console, int x, int y)
            {
                console.WriteLine(x + y);
            }

            [Command(ArgumentSeparatorStrategy = ArgumentSeparatorStrategy.EndOfOptions)]
            public void Add_EndOfOptions(IConsole console, int x, int y)
            {
                console.WriteLine(x + y);
            }

            [Command(ArgumentSeparatorStrategy = ArgumentSeparatorStrategy.PassThru)]
            public void Add_PassThru(IConsole console, int x, int y)
            {
                console.WriteLine(x + y);
            }
        }
    }
}