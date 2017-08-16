//-----------------------------------------------------------------------
// <copyright file="LocalCommands.cs" company="Colagioia Industries">
//     Copyright (c) John Colagioia, available under the GPLv3.
// </copyright>
//-----------------------------------------------------------------------
namespace Track_Time
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// Local commands for the application.
    /// </summary>
    public class LocalCommands
    {
        /// <summary>
        /// The pause button's command.
        /// </summary>
        public static readonly RoutedUICommand Pause
            = new RoutedUICommand(
                "Pause",
                "Pause",
                typeof(LocalCommands));

        /// <summary>
        /// The clear-log command.
        /// </summary>
        public static readonly RoutedUICommand Clear
            = new RoutedUICommand(
                "Clear",
                "Clear",
                typeof(LocalCommands));

        /// <summary>
        /// The save file command.
        /// </summary>
        public static readonly RoutedUICommand Save
            = new RoutedUICommand(
                "Save...",
                "Save",
                typeof(LocalCommands));

        /// <summary>
        /// The exit-program command.
        /// </summary>
        public static readonly RoutedUICommand Exit
            = new RoutedUICommand(
                "Exit",
                "Exit",
                typeof(LocalCommands));
    }
}