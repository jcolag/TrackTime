﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Track_Time
{
    class LocalCommands
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
