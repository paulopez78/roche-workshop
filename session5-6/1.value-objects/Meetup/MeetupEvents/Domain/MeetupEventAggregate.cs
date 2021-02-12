using System;
using System.Collections.Generic;
using System.Linq;

namespace MeetupEvents.Domain
{
    public class MeetupEventAggregate
    {
        public Guid              Id                 { get; private set; }
        public Details           Details            { get; private set; } = null!;
        public PositiveNumber    Capacity           { get; private set; } = 0;
        public MeetupEventStatus Status             { get; private set; } = MeetupEventStatus.None;
        public ScheduleDateTime? ScheduleDateTime   { get; private set; }
        public Location?         Location           { get; private set; }
        public string?           CancellationReason { get; private set; }

        readonly List<Attendant>        _attendants = new();
        public   IEnumerable<Attendant> Attendants => _attendants.OrderBy(x => x.AddedAt);

        public void Create(Guid id, Details details, PositiveNumber capacity)
        {
            EnforceNoneCreated();

            Id       = id;
            Details  = details;
            Capacity = capacity;
            Status   = MeetupEventStatus.Draft;
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

        public void Attend(Guid memberId, DateTimeOffset at)
        {
            EnforceNotAttending();
            EnforcePublished();

            var attendant = new Attendant(memberId, at);

            if (HasFreeSpots())
                attendant.Attend();
            else
                attendant.Wait();

            _attendants.Add(attendant);

            bool HasFreeSpots() => Capacity - _attendants.Count > 0;

            void EnforceNotAttending()
            {
                if (Attendants.Any(x => x.MemberId == memberId))
                    throw new InvalidOperationException($"Member {memberId} already attending");
            }
        }

        public void CancelAttendance(Guid memberId)
        {
            EnforceAttending();
            EnforcePublished();

            _attendants.RemoveAll(x => x.MemberId == memberId);

            UpdateWaitingList();

            void EnforceAttending()
            {
                if (Attendants.All(x => x.MemberId != memberId))
                    throw new InvalidOperationException($"Member {memberId} not attending");
            }

            void UpdateWaitingList() =>
                Attendants.FirstOrDefault(x => x.Waiting)?.Attend();
        }

        public void ReduceCapacity(PositiveNumber byNumber)
        {
            EnforceActive();

            Capacity -= byNumber;
            UpdateWaitingList();

            void UpdateWaitingList() =>
                Attendants
                    .Where(x => !x.Waiting)
                    .TakeLast(byNumber)
                    .ToList()
                    .ForEach(x => x.Wait());
        }

        public void IncreaseCapacity(PositiveNumber byNumber)
        {
            EnforceActive();

            Capacity += byNumber;
            UpdateWaitingList();

            void UpdateWaitingList() =>
                Attendants
                    .Where(x => x.Waiting)
                    .Take(byNumber)
                    .ToList()
                    .ForEach(x => x.Attend());
        }

        void EnforcePublished() =>
            EnforceStatusMustBe(MeetupEventStatus.Published);

        void EnforceDraft() =>
            EnforceStatusMustBe(MeetupEventStatus.Draft);

        void EnforceNoneCreated() =>
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

    public class Attendant
    {
        public Guid MemberId { get; private set; }

        public DateTimeOffset AddedAt { get; private set; }

        public bool Waiting { get; private set; }

        public Attendant(Guid memberId, DateTimeOffset addedAt)
        {
            MemberId = memberId;
            AddedAt  = addedAt;
        }

        public void Attend() =>
            Waiting = false;

        public void Wait() =>
            Waiting = true;
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