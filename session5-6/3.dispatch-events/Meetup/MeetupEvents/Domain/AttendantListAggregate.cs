using System;
using System.Collections.Generic;
using System.Linq;
using MeetupEvents.Framework;
using static MeetupEvents.Contracts.AttendantListEvents.V1;

namespace MeetupEvents.Domain
{
    public class AttendantListAggregate : Aggregate
    {
        readonly List<Attendant>        _attendants = new();
        public   IEnumerable<Attendant> Attendants    => _attendants;
        public   Guid                   MeetupEventId { get; private set; }
        public   PositiveNumber         Capacity      { get; private set; } = 0;
        public   AttendantListStatus    Status        { get; private set; } = AttendantListStatus.None;

        public void Create(Guid id, Guid meetupEventId, PositiveNumber capacity)
        {
            EnforceNotCreated();

            Id            = id;
            Capacity      = capacity == 0 ? 10 : capacity;
            Status        = AttendantListStatus.Closed;
            MeetupEventId = meetupEventId;

            _changes.Add(new AttendantListCreated(Id, MeetupEventId, Capacity));
        }

        public void Open(DateTimeOffset at)
        {
            EnforceActive();
            Status = AttendantListStatus.Opened;

            _changes.Add(new Opened(Id, at));
        }

        public void Close(DateTimeOffset at)
        {
            EnforceOpened();
            Status = AttendantListStatus.Closed;

            _changes.Add(new Closed(Id, at));
        }

        public void Archive(DateTimeOffset at)
        {
            Status = AttendantListStatus.Archived;

            _changes.Add(new Archived(Id, at));
        }

        public void Attend(Guid memberId, DateTimeOffset at)
        {
            EnforceNotAttending();
            EnforceOpened();

            var attendant = new Attendant(memberId, at);

            if (HasFreeSpots())
            {
                attendant.Attend();
                _changes.Add(new AttendantAdded(Id, memberId, at));
            }
            else
            {
                attendant.Wait();
                _changes.Add(new AttendantMovedToWaiting(Id, memberId, at));
            }

            _attendants.Add(attendant);

            bool HasFreeSpots() => Capacity - _attendants.Count > 0;

            void EnforceNotAttending()
            {
                if (Attendant(memberId) is not null)
                    throw new InvalidOperationException($"Member {memberId} already attending");
            }
        }

        public void CancelAttendance(Guid memberId)
        {
            EnforceOpened();
            EnforceAttending();

            _attendants.Remove(Attendant(memberId)!);
            _changes.Add(new AttendantRemoved(Id, memberId));

            UpdateWaitingList();

            void EnforceAttending()
            {
                if (Attendant(memberId) is null)
                    throw new InvalidOperationException($"Member {memberId} is not attending");
            }

            void UpdateWaitingList()
            {
                var firstWaiting = Attendants.FirstOrDefault(x => x.Waiting);
                if (firstWaiting is null) return;

                firstWaiting.Attend();
                _changes.Add(new AttendantAdded(Id, firstWaiting.MemberId, firstWaiting.AddedAt));
            }
        }

        public void ReduceCapacity(PositiveNumber byNumber)
        {
            EnforceActive();

            Capacity -= byNumber;
            UpdateWaitingList();

            void UpdateWaitingList()
            {
                var shouldWait = Attendants
                    .Where(x => !x.Waiting)
                    .TakeLast(byNumber)
                    .ToList();

                shouldWait.ForEach(x =>
                {
                    x.Wait();
                    _changes.Add(new AttendantMovedToWaiting(Id, x.MemberId, x.AddedAt));
                });
            }
        }

        public void IncreaseCapacity(PositiveNumber byNumber)
        {
            EnforceActive();

            Capacity += byNumber;
            UpdateWaitingList();

            void UpdateWaitingList()
            {
                var shouldAttend = Attendants
                    .Where(x => x.Waiting)
                    .Take(byNumber)
                    .ToList();

                shouldAttend.ForEach(x =>
                {
                    x.Attend();
                    _changes.Add(new AttendantAdded(Id, x.MemberId, x.AddedAt));
                });
            }
        }

        void EnforceNotCreated() =>
            EnforceStatusMustBe(AttendantListStatus.None);

        void EnforceOpened() =>
            EnforceStatusMustBe(AttendantListStatus.Opened);

        void EnforceActive()
        {
            if (Status == AttendantListStatus.Archived)
                throw new InvalidOperationException("Not active attendant list");
        }

        void EnforceStatusMustBe(AttendantListStatus status)
        {
            if (Status != status)
                throw new InvalidOperationException($"Invalid status {status}");
        }

        Attendant? Attendant(Guid attendantId) =>
            Attendants.FirstOrDefault(x => x.MemberId == attendantId);
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

    public enum AttendantListStatus
    {
        None,
        Opened,
        Closed,
        Archived
    }
}