﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommandDotNet.MicrosoftCommandLineUtils
{
    public interface ICommand : INameAndDescription
    {
        string Name { get; }
        string Description { get; }
        string ExtendedHelpText { get; }
        ICommand Parent { get; }
        IEnumerable<ICommand> Commands { get; }
        IEnumerable<IOperand> Operands { get; }
        IEnumerable<IOption> GetOptions(bool includeInherited = true);
        IOption OptionHelp { get; }

        ICustomAttributeProvider CustomAttributeProvider { get; }

        #region Obsolete members

        [Obsolete("do not use.  value is always true.")]
        bool ShowInHelpText { get; }
        [Obsolete("This was used solely for help.  The functionality has been moved to help providers.")]
        string GetFullCommandName();

        #endregion
    }
}