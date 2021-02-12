using System;
using MeetupEvents.Framework;

namespace MeetupEvents.Domain
{
    public class MeetupEventAggregate : Aggregate
    {
        public Guid              GroupId            { get; private set; }
        public Details           Details            { get; private set; } = null!;
        public MeetupEventStatus Status             { get; private set; } = MeetupEventStatus.None;
        public ScheduleDateTime? ScheduleDateTime   { get; private set; }
        public Location?         Location           { get; private set; }
        public string?           CancellationReason { get; private set; }

        public void Create(Guid id, Guid groupId, Details details)
        {
            EnforceNotCreated();

            Id      = id;
            GroupId = groupId;
            Details = details;
            Status  = MeetupEventStatus.Draft;
        }

        public void UpdateDetails(Details details)
        {
            EnforceActive();
            Details = details;
        }

        public void Schedule(ScheduleDateTime scheduleDateTime)
        {
            EnforceActive();
            ScheduleDateTime = scheduleDateTime;
        }

        public void MakeOnline(Uri url)
        {
            EnforceActive();
            Location = Location.OnLine(url);
        }

        public void MakeOnsite(Address address)
        {
            EnforceActive();
            Location = Location.OnSite(address);
        }

        public void Publish()
        {
            EnforceScheduled();
            EnforceLocation();
            EnforceDraft();

            Status = MeetupEventStatus.Published;
        }

        public void Cancel(string? reason = null)
        {
            EnforcePublished();

            Status             = MeetupEventStatus.Cancelled;
            CancellationReason = reason;
        }

        public void Start()
        {
            EnforcePublished();
            Status = MeetupEventStatus.Started;
        }

        public void Finish()
        {
            EnforceStarted();
            Status = MeetupEventStatus.Finished;

            void EnforceStarted() => EnforceStatusMustBe(MeetupEventStatus.Started);
        }

        void EnforcePublished() =>
            EnforceStatusMustBe(MeetupEventStatus.Published);

        void EnforceDraft() =>
            EnforceStatusMustBe(MeetupEventStatus.Draft);

        void EnforceNotCreated() =>
            EnforceStatusMustBe(MeetupEventStatus.None);

        void EnforceActive()
        {
            if (Status != MeetupEventStatus.Draft && Status != MeetupEventStatus.Published)
                throw new InvalidOperationException("Not active meetup");
        }

        void EnforceStatusMustBe(MeetupEventStatus status)
        {
            if (Status != status)
                throw new InvalidOperationException($"Invalid status {status}");
        }

        void EnforceScheduled()
        {
            if (ScheduleDateTime is null)
                throw new ArgumentException("Not scheduled");
        }

        void EnforceLocation()
        {
            if (Location is null)
                throw new ArgumentException("Location not specified");
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