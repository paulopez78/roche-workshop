using System;

namespace MeetupEvents.Domain
{
    public class MeetupEventEntity
    {
        public Guid              Id       { get; private set; }
        public string            Title    { get; private set; } = string.Empty;
        public int               Capacity { get; private set; }
        public MeetupEventStatus Status   { get; private set; } = MeetupEventStatus.None;

        public void Create(Guid id, string title, int capacity)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException(nameof(title));

            if (capacity <= 0)
                throw new ArgumentException("Capacity must be positive");

            EnforceStatusMustBe(MeetupEventStatus.None);


            Id       = id;
            Title    = title;
            Capacity = capacity;
            Status   = MeetupEventStatus.Draft;
        }

        public void Publish()
        {
            EnforceStatusMustBe(MeetupEventStatus.Draft);
            Status = MeetupEventStatus.Published;
        }

        public void Cancel()
        {
            EnforceStatusMustBe(MeetupEventStatus.Published);
            Status = MeetupEventStatus.Cancelled;
        }

        void EnforceStatusMustBe(MeetupEventStatus status)
        {
            if (Status != status)
                throw new InvalidOperationException("Invalid status");
        }
    }

    public enum MeetupEventStatus
    {
        None,
        Draft,
        Published,
        Cancelled,
        Started,
        Finished
    }
}