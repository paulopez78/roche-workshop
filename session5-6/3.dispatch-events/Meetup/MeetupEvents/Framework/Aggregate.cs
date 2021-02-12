using System;
using System.Collections.Generic;

namespace MeetupEvents.Framework
{
    public class Aggregate
    {
        public Guid Id      { get; protected set; }
        public int  Version { get; private set; }

        protected List<object>        _changes = new();
        public    IEnumerable<object> Changes => _changes;

        public void ClearChanges() => _changes.Clear();
        public void IncreaseVersion() => Version += 1;
    }
}