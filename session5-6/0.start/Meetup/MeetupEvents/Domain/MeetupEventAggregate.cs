using System;
using System.Collections.Generic;
using System.Linq;

namespace MeetupEvents.Domain
{
    public class MeetupEventAggregate
    {
        public Guid              Id                 { get; private set; }
        public string            Title              { get; private set; } = string.Empty;
        public string            Description        { get; private set; } = string.Empty;
        public int               Capacity           { get; private set; }
        public MeetupEventStatus Status             { get; private set; } = MeetupEventStatus.None;
        public DateTimeOffset?   StartTime          { get; private set; }
        public DateTimeOffset?   EndTime            { get; private set; }
        public string?           Address            { get; private set; }
        public Uri?              Url                { get; private set; }
        public string?           CancellationReason { get; private set; }

        readonly List<Attendant>        _attendants = new();
        public   IEnumerable<Attendant> Attendants => _attendants.OrderBy(x => x.AddedAt);
        IEnumerable<Attendant>          Going      => Attendants.Where(x => !x.Waiting);
        IEnumerable<Attendant>          Waiting    => Attendants.Where(x => x.Waiting);
        Attendant? GetAttendant(Guid attendantId) => Attendants.FirstOrDefault(x => x.MemberId == attendantId);

        public void Create(Guid id, string title, string description, int capacity)
        {
            EnforceNotEmptyDetails(title, description);
            EnforcePositiveNumber(capacity);
            EnforceNoneCreated();

            Id          = id;
            Title       = title;
            Description = description;
            Capacity    = capacity;
            Status      = MeetupEventStatus.Draft;
        }


        public void UpdateDetails(string title, string description)
        {
            EnforceNotEmptyDetails(title, description);
            EnforceActive();

            Title       = title;
            Description = description;
        }

        public void Schedule(DateTimeOffset start, DateTimeOffset end, DateTimeOffset now)
        {
            EnforceValidScheduleTimeRange();
            EnforceActive();

            StartTime = start;
            EndTime   = end;

            void EnforceValidScheduleTimeRange()
            {
                if (start <= now)
                    throw new ArgumentException($"Schedule start time {start} can not be before now {now}");

                if (start >= end)
                    throw new ArgumentException($"Schedule start time {start} can not be after end {end}");
            }
        }

        public void MakeOnline(Uri url)
        {
            EnforceActive();
            Url = url;
        }

        public void MakeOnsite(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException(nameof(address));

            EnforceActive();

            Address = address;
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

            var attendant = GetAttendant(memberId)!;
            _attendants.Remove(attendant);

            UpdateWaitingList();

            void EnforceAttending()
            {
                if (GetAttendant(memberId) is null)
                    throw new InvalidOperationException($"Member {memberId} is not attending");
            }

            void UpdateWaitingList()
            {
                if (attendant.Waiting) return;
                Attendants.FirstOrDefault(x => x.Waiting)?.Attend();
            }
        }

        public void ReduceCapacity(int byNumber)
        {
            EnforcePositiveNumber(byNumber);
            EnforceActive();

            Capacity -= byNumber;
            UpdateWaitingList();

            void UpdateWaitingList() =>
                Going
                    .TakeLast(LostSpots()).ToList()
                    .ForEach(x => x.Wait());

            int LostSpots() =>
                Going.Count() - Capacity;
        }

        public void IncreaseCapacity(int byNumber)
        {
            EnforcePositiveNumber(byNumber);
            EnforceActive();

            Capacity += byNumber;
            UpdateWaitingList();

            void UpdateWaitingList() =>
                Waiting
                    .Take(byNumber).ToList()
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

        void EnforcePositiveNumber(int number)
        {
            if (number <= 0)
                throw new ArgumentException("Capacity must be positive");
        }

        void EnforceNotEmptyDetails(string title, string description)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException(nameof(title));

            if (string.IsNullOrEmpty(description))
                throw new ArgumentNullException(nameof(description));
        }

        void EnforceScheduled()
        {
            if (StartTime is null || EndTime is null)
                throw new ArgumentException("Not scheduled");
        }

        void EnforceLocation()
        {
            if (string.IsNullOrEmpty(Address) && Url is null)
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

        public void Attend() => Waiting = false;

        public void Wait() => Waiting = true;
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