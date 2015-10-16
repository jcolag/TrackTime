# TrackTime

Window-based time-tracking for Microsoft Windows (.NET)

I recently stumbled across this on a hard drive I hadn't checked in a couple of years.  It _appears_ to be a first run at creating something like _[uManage](https://github.com/jcolag/uManage)_ that I don't remember working on.  It uses Win32 calls to find the active window, snag the title, and even guesstimate user idle time, showing all updates in the main window.

It's not a very sophisticated application and probably could use a lot of work, and I don't think I'll be updating it seriously, but it does basically do its job of keeping track of where time is spent.

There isn't much to the interface.  Updates (window title and duration) are appended to the text in the window.  The time between checks can be configured.  The monitoring can be paused.  From the menu, the log can be saved or cleared.  That's about it.

Perhaps the most interesting thing about the code is that, while the code is completely in C#, the critical features aren't exposed to the .NET framework and are the same Windows API calls I needed to make back in the 1990s.
