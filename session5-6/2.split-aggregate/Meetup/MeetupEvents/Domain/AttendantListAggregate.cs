using System;
using System.Collections.Generic;
using System.Linq;
using MeetupEvents.Framework;

namespace MeetupEvents.Domain
{
    public class AttendantListAggregate : Aggregate
    {
        public Guid                MeetupEventId { get; private set; }
        public PositiveNumber      Capacity      { get; private set; } = 0;
        public AttendantListStatus Status        { get; private set; } = AttendantListStatus.None;

        readonly List<Attendant>        _attendants = new();
        public   IEnumerable<Attendant> Attendants => _attendants.OrderBy(x => x.AddedAt);
        IEnumerable<Attendant>          Going      => Attendants.Where(x => !x.Waiting);
        IEnumerable<Attendant>          Waiting    => Attendants.Where(x => x.Waiting);
        Attendant? GetAttendant(Guid attendantId) => Attendants.FirstOrDefault(x => x.MemberId == attendantId);

        public void Create(Guid id, Guid meetupEventId, PositiveNumber capacity)
        {
            EnforceNotCreated();

            Id            = id;
            Capacity      = capacity == 0 ? 10 : capacity;
            Status        = AttendantListStatus.Closed;
            MeetupEventId = meetupEventId;
        }

        public void Open(DateTimeOffset at)
        {
            EnforceActive();
            Status = AttendantListStatus.Opened;
        }

        public void Close(DateTimeOffset at)
        {
            EnforceOpened();
            Status = AttendantListStatus.Closed;
        }

        public void Archive(DateTimeOffset at)
        {
            Status = AttendantListStatus.Archived;
        }

        public void Attend(Guid memberId, DateTimeOffset at)
        {
            EnforceNotAttending();
            EnforceOpened();

            var attendant = new Attendant(memberId, at);

            if (HasFreeSpots())
                attendant.Attend();
            else
                attendant.Wait();

            _attendants.Add(attendant);

            bool HasFreeSpots() => Capacity - _attendants.Count > 0;

            void EnforceNotAttending()
            {
                if (GetAttendant(memberId) is not null)
                    throw new InvalidOperationException($"Member {memberId} already attending");
            }
        }

        public void CancelAttendance(Guid memberId)
        {
            EnforceOpened();
            EnforceAttending();

            var attendant = GetAttendant(memberId)!;

            _attendants.Remove(attendant!);

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

        public void ReduceCapacity(PositiveNumber byNumber)
        {
            EnforceActive();

            Capacity -= byNumber;

            Going.TakeLast(LostSpots())
                .ToList().ForEach(x => x.Wait());

            PositiveNumber LostSpots() =>
                Going.Count() - Capacity;
        }

        public void IncreaseCapacity(PositiveNumber byNumber)
        {
            EnforceActive();

            Capacity += byNumber;

            Waiting.Take(byNumber)
                .ToList().ForEach(x => x.Attend());
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