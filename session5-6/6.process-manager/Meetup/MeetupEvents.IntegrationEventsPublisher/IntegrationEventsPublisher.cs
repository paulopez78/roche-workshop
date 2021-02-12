using System;
using System.Threading.Tasks;
using MassTransit;
using MeetupEvents.Contracts;
using static MeetupEvents.Contracts.AttendantListEvents.V1;
using static MeetupEvents.Contracts.MeetupEvents.V1;

namespace MeetupEvents.IntegrationEventsPublisher
{
    public class IntegrationEventsPublisher :
        IConsumer<Published>,
        IConsumer<Canceled>,
        IConsumer<AttendantAdded>,
        IConsumer<AttendantMovedToWaiting>
    {
        private readonly GetMeetupDetails _getMeetupDetails;
        private readonly GetMeetupEventId _getMeetupId;

        public IntegrationEventsPublisher(GetMeetupDetails getMeetupDetails, GetMeetupEventId getMeetupEventId)
        {
            _getMeetupDetails = getMeetupDetails;
            _getMeetupId      = getMeetupEventId;
        }

        public async Task Consume(ConsumeContext<Published> context)
        {
            await context.Publish(
                new IntegrationEvents.V1.MeetupPublished(
                    context.Message.Id,
                    context.Message.At
                )
            );

            var meetup = await GetMeetupDetails(context.Message.Id);

            await context.Publish(
                new IntegrationEvents.V2.MeetupPublished(
                    context.Message.Id,
                    meetup.Title,
                    meetup.Description
                )
            );
        }

        public Task Consume(ConsumeContext<Canceled> context) =>
            context.Publish(
                new IntegrationEvents.V1.MeetupCanceled(
                    context.Message.Id,
                    context.Message.Reason,
                    context.Message.At
                )
            );

        public async Task Consume(ConsumeContext<AttendantAdded> context)
        {
            var meetupId = await GetMeetupEventId(context.Message.Id);
            await context.Publish(
                new IntegrationEvents.V1.MeetupAttendantAdded(
                    meetupId,
                    context.Message.MemberId,
                    context.Message.At
                )
            );
        }

        public async Task Consume(ConsumeContext<AttendantMovedToWaiting> context)
        {
            var meetupId = await GetMeetupEventId(context.Message.Id);
            await context.Publish(
                new IntegrationEvents.V1.MeetupAttendantMovedToWaiting(
                    meetupId,
                    context.Message.MemberId,
                    context.Message.At
                )
            );
        }

        async Task<Guid> GetMeetupEventId(Guid attendantListId)
        {
            var meetupId = await _getMeetupId(attendantListId);
            if (meetupId is null)
                throw new ArgumentException($"MeetupId for AttendantList {attendantListId} not found.");

            return meetupId.Value;
        }

        async Task<MeetupDetails> GetMeetupDetails(Guid meetupId)
        {
            var meetup = await _getMeetupDetails(meetupId);
            if (meetup is null)
                throw new ArgumentException($"Meetup details {meetupId} not found.");
            return meetup;
        }
    }
}