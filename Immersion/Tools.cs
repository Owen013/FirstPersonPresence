using System;

namespace Immersion
{
    [Flags]
    public enum Tools
    {
        None = 0,
        ItemTool = 1,
        Signalscope = 2,
        ProbeLauncher = 4,
        Translator = 8,
        All = ItemTool | Signalscope | ProbeLauncher | Translator
    }
}