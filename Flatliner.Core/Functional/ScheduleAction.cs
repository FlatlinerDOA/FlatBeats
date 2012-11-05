namespace Flatliner.Functional
{
    using System;

    public delegate IDisposable ScheduleAction(Action invoke);
}