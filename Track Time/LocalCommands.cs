//-----------------------------------------------------------------------
// <copyright file="LocalCommands.cs" company="Colagioia Industries">
//     Copyright (c) John Colagioia, available under the GPLv3.
// </copyright>
//-----------------------------------------------------------------------
namespace Track_Time
{
    class LocalCommands
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

    {
        public static readonly RoutedUICommand Pause
            = new RoutedUICommand(
                "Pause",
                "Pause",
                typeof(LocalCommands)
                );

        public static readonly RoutedUICommand Clear
            = new RoutedUICommand(
                "Clear",
                "Clear",
                typeof(LocalCommands)
                );

        public static readonly RoutedUICommand Save
            = new RoutedUICommand(
                "Save...",
                "Save",
                typeof(LocalCommands)
                );

        public static readonly RoutedUICommand Exit
            = new RoutedUICommand(
                "Exit",
                "Exit",
                typeof(LocalCommands)
                );
    }
}