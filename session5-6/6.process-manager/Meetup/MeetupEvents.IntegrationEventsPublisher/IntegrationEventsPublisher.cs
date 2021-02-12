using System;
using System.Threading.Tasks;
using MassTransit;
using MeetupEvents.Contracts;
using static MeetupEvents.Contracts.AttendantListEvents.V1;
using static MeetupEvents.Contracts.MeetupEvents.V1;

namespace MeetupEvents.IntegrationEventsPublisher
{
    public class IntegrationEventsPublisher :
        IConsumer<Created>,
        IConsumer<Scheduled>,
        IConsumer<Published>,
        IConsumer<Started>,
        IConsumer<Finished>,
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

        public Task Consume(ConsumeContext<Created> context) =>
            context.Publish(
                new IntegrationEvents.V1.MeetupCreated(context.Message.Id)
            );

        public Task Consume(ConsumeContext<Scheduled> context) =>
            context.Publish(
                new IntegrationEvents.V1.MeetupScheduled(context.Message.Id, context.Message.Start, context.Message.End)
            );

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
                new IntegrationEvents.V1.MeetupCancelled(
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

        public Task Consume(ConsumeContext<Started> context) =>
            context.Publish(
                new IntegrationEvents.V1.MeetupStarted(context.Message.Id)
            );

        public Task Consume(ConsumeContext<Finished> context) =>
            context.Publish(
                new IntegrationEvents.V1.MeetupFinished(context.Message.Id)
            );

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