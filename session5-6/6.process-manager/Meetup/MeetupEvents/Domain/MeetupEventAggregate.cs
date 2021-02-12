using System;
using MeetupEvents.Framework;
using static MeetupEvents.Contracts.MeetupEvents.V1;

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
            
            _changes.Add(new Created(id, groupId, details.Title, details.Description));
        }

        public void UpdateDetails(Details details)
        {
            EnforceActive();
            Details = details;
            
            _changes.Add(new DetailsUpdated(Id, details.Title, details.Description));
        }

        public void Schedule(ScheduleDateTime scheduleDateTime)
        {
            EnforceActive();
            ScheduleDateTime = scheduleDateTime;
            
            _changes.Add(new Scheduled(Id, ScheduleDateTime.Start, ScheduleDateTime.End));
        }

        public void MakeOnline(Uri url)
        {
            EnforceActive();
            Location = Location.OnLine(url);
            
            _changes.Add(new MadeOnline(Id, url));
        }

        public void MakeOnsite(Address address)
        {
            EnforceActive();
            Location = Location.OnSite(address);
            
            _changes.Add(new MadeOnsite(Id, address));
        }

        public void Publish(DateTimeOffset at)
        {
            EnforceScheduled();
            EnforceLocation();
            EnforceDraft();

            Status = MeetupEventStatus.Published;
            
            _changes.Add(new Published(Id, at));
        }

        public void Cancel(DateTimeOffset at, string? reason = null )
        {
            EnforcePublished();

            Status             = MeetupEventStatus.Cancelled;
            CancellationReason = reason;
            
            _changes.Add(new Canceled(Id, CancellationReason!, at));
        }

        public void Start()
        {
            EnforcePublished();
            Status = MeetupEventStatus.Started;
            
            _changes.Add(new Started(Id));
        }

        public void Finish()
        {
            EnforceStarted();
            Status = MeetupEventStatus.Finished;
            
            _changes.Add(new Finished(Id));

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