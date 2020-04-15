﻿using CommandDotNet.Extensions;
using CommandDotNet.Rendering;
using CommandDotNet.TestTools.Scenarios;
using CommandDotNet.Tokens;
using Xunit;
using Xunit.Abstractions;

namespace CommandDotNet.Tests.FeatureTests
{
    public class CustomTokenTransformationTests
    {
        private readonly ITestOutputHelper _output;

        public CustomTokenTransformationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanRegisterCustomTokenTransformation()
        {
            new AppRunner<App>()
                .Configure(c =>
                    c.UseTokenTransformation("test", 1,
                        (ctx, tokens) => tokens.Transform(
                            skipDirectives: true,
                            skipSeparated: true,
                            transformation: t =>
                                t.TokenType == TokenType.Value && t.Value == "like"
                                    ? Tokenizer.TokenizeValue("roses").ToEnumerable()
                                    : t.ToEnumerable())))
                .VerifyScenario(_output, new Scenario
                {
                    WhenArgs = "Do --opt1 smells like",
                    Then =
                    {
                        Result = "smells roses"
                    }
                });
        }

        public class App
        {
            public void Do(IConsole console, [Option] string opt1, string arg1 = "wet dog")
            {
                console.Out.Write($"{opt1} {arg1}");
            }
        }
    }
}