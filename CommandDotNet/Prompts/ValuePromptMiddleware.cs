﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CommandDotNet.Execution;
using CommandDotNet.Extensions;
using CommandDotNet.Logging;

namespace CommandDotNet.Prompts
{
    internal static class ValuePromptMiddleware
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        internal static AppRunner UsePrompting(
            AppRunner appRunner,
            Func<CommandContext, IPrompter> prompterOverride = null,
            bool promptForMissingArguments = true,
            Func<CommandContext, IArgument, string> argumentPromptTextOverride = null,
            Predicate<IArgument> argumentFilter = null)
        {
            return appRunner.Configure(c =>
            {

                prompterOverride = prompterOverride
                                   ?? c.Services.Get<Func<CommandContext, IPrompter>>()
                                   ?? (ctx =>
                                   {
                                       // create only one prompter per CommandContext
                                       // in theory, an implementation could track prompts and values
                                       var prompter = ctx.Services.Get<IPrompter>();
                                       if(prompter == null)
                                       {
                                           prompter = new Prompter(ctx.Console);
                                           ctx.Services.AddOrUpdate(prompter);
                                       }
                                       return prompter;
                                   });

                c.UseParameterResolver(ctx => prompterOverride(ctx));

                if (promptForMissingArguments)
                {
                    argumentFilter = argumentFilter ?? (a => a.Arity.RequiresAtLeastOne()); 
                    c.UseMiddleware(
                        (ctx, next) => PromptForMissingArguments(ctx, next,
                            new ArgumentPrompter(prompterOverride(ctx), argumentPromptTextOverride), argumentFilter), 
                        MiddlewareSteps.ValuePromptMissingArguments.Stage, MiddlewareSteps.ValuePromptMissingArguments.Order);
                }
            });
        }

        private static Task<int> PromptForMissingArguments(
            CommandContext commandContext, 
            ExecutionDelegate next, 
            IArgumentPrompter argumentPrompter,
            Predicate<IArgument> argumentFilter)
        {
            var parseResult = commandContext.ParseResult;

            if (commandContext.Console.IsInputRedirected)
            {
                Log.Debug("Skipping prompts. Console does not support Console.ReadKey when Console.IsInputRedirected.");
                // avoid: System.InvalidOperationException: Cannot read keys when either application does not have a console or when console input has been redirected. Try Console.Read.
                return next(commandContext);
            }

            if (parseResult.HelpWasRequested())
            {
                Log.Debug("Skipping prompts. Help was requested.");
                return next(commandContext);
            }

            bool isCancellationRequested = false;

            parseResult.TargetCommand
                .AllArguments(includeInterceptorOptions: true)
                .Where(a => a.SwitchFunc(
                    operand => true,
                    option => !option.Arity.AllowsNone() // exclude flag options: help, version, ...
                ))
                .Where(a => argumentFilter == null || argumentFilter(a))
                .Where(a => a.InputValues.IsEmpty() && (a.Default?.Value.IsNullValue() ?? true))
                .TakeWhile(a => !commandContext.AppConfig.CancellationToken.IsCancellationRequested && !isCancellationRequested)
                .ForEach(a =>
                {
                    Log.Debug($"Prompting for {a.Name}");
                    var values = argumentPrompter.PromptForArgumentValues(commandContext, a, out isCancellationRequested);
                    a.InputValues.Add(new InputValue(Constants.InputValueSources.Prompt, false, values));
                });

            if (isCancellationRequested)
            {
                return Task.FromResult(0);
            }

            return next(commandContext);
        }
    }
}