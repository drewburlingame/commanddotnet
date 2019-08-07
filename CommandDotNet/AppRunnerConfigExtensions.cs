﻿using System;
using CommandDotNet.Builders;
using CommandDotNet.Directives;
using CommandDotNet.Parsing;

namespace CommandDotNet
{
    /// <summary>Extensions to enable and configure features</summary>
    public static class AppRunnerConfigExtensions
    {
        [Obsolete("replace with UseVersionMiddleware, UseFluentValidation ")]
        public static AppRunner UseBackwardsCompatibilityMode(this AppRunner appRunner)
        {
            if (appRunner.AppSettings.EnableVersionOption)
            {
                appRunner.UseVersionMiddleware();
            }

            return appRunner;
        }

        public static AppRunner UseVersionMiddleware(this AppRunner appRunner)
        {
            return VersionMiddleware.UseVersionMiddleware(appRunner);
        }

        public static AppRunner UseDebugDirective(this AppRunner appRunner)
        {
            AssertDirectivesAreEnabled(appRunner);
            return DebugDirective.UseDebugDirective(appRunner);
        }

        public static AppRunner UseParseDirective(this AppRunner appRunner)
        {
            AssertDirectivesAreEnabled(appRunner);
            return ParseDirective.UseParseDirective(appRunner);
        }

        /// <summary>Enables piped input to be appended to any commands operand list</summary>
        public static AppRunner AppendPipedInputToOperandList(this AppRunner appRunner)
        {
            return PipedInputMiddleware.EnablePipedInput(appRunner);
        }

        private static void AssertDirectivesAreEnabled(AppRunner appRunner)
        {
            if (!appRunner.AppSettings.EnableDirectives)
            {
                throw new AppRunnerException($"Directives are not enabled.  " +
                                             $"{nameof(AppRunner)}.{nameof(AppRunner.AppSettings)}.{nameof(AppSettings.EnableDirectives)} " +
                                             "must be set to true");
            }
        }
    }
}