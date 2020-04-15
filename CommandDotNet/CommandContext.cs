﻿using CommandDotNet.Builders;
using CommandDotNet.Execution;
using CommandDotNet.Parsing;
using CommandDotNet.Rendering;
using CommandDotNet.Tokens;

namespace CommandDotNet
{
    public class CommandContext
    {
        /// <summary>The original string array and first parse of tokens</summary>
        public OriginalInput Original { get; }

        /// <summary>
        /// The current working tokens. This collection is update with
        /// <see cref="TokenTransformation"/>s during <see cref="MiddlewareStages.Tokenize"/>.
        /// Modifications should be performed via <see cref="TokenTransformation"/>s so they
        /// can be logged with the Parse directive
        /// </summary>
        public TokenCollection Tokens { get; set; }

        /// <summary>
        /// The root command for the type specified in <see cref="AppRunner{TRootCommandType}"/>.  
        /// This is populated in the <see cref="MiddlewareStages.Tokenize"/> stage.
        /// </summary>
        public Command RootCommand { get; set; }

        /// <summary>
        /// The results of the <see cref="MiddlewareStages.ParseInput"/> stage.
        /// </summary>
        public ParseResult ParseResult { get; set; }

        /// <summary>
        /// The <see cref="InvocationStep"/>s within the pipeline are mostly populated in
        /// the <see cref="MiddlewareStages.ParseInput"/> stage.
        /// <see cref="InvocationStep.Instance"/> and <see cref="IInvocation.ParameterValues"/>
        /// are populated in the <see cref="MiddlewareStages.BindValues"/> stage.
        /// </summary>
        public InvocationPipeline InvocationPipeline { get; } = new InvocationPipeline();

        public AppConfig AppConfig { get; }

        /// <summary>The <see cref="IConsole"/>, defaulted from <see cref="Execution.AppConfig.Console"/>.</summary>
        public IConsole Console { get; set; }

        /// <summary>When true, help will be displayed as the app exits</summary>
        public bool ShowHelpOnExit { get; set; }

        /// <summary>
        /// Services registered for the lifetime of the <see cref="CommandContext"/>.<br/>
        /// Can be used to store state for coordination between middleware and across middleware stages.
        /// </summary>
        public IServices Services { get; } = new Services();

        /// <summary>
        /// The application <see cref="IDependencyResolver"/>.<br/>
        /// Set in <see cref="AppRunner.Configure"/>
        /// </summary>
        /// <remarks>
        /// Delegate from AppConfig. Included here for easier discovery
        /// and reduce confusion with <see cref="Services"/>
        /// </remarks>
        public IDependencyResolver DependencyResolver => AppConfig.DependencyResolver;

        public CommandContext(
            string[] originalArgs, 
            TokenCollection originalTokens,
            AppConfig appConfig)
        {
            Original = new OriginalInput(originalArgs, originalTokens);
            Tokens = originalTokens;
            AppConfig = appConfig;
            Console = appConfig.Console;
        }
    }
}