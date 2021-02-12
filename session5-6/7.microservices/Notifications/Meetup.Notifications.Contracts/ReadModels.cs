namespace Meetup.Notifications.Contracts
{
    public static class ReadModels
    {
        public static class V1
        {
            public record Notification
            {
                public string           Id               { get; set; }
                public string           UserId           { get; set; }
                public string           Message          { get; set; }
                public string           GroupId          { get; set; }
                public string           MeetupId         { get; set; }
                public string           MemberId         { get; set; }
                public NotificationType NotificationType { get; set; }
            }

            public enum NotificationType
            {
                Message,
                NewGroupCreated,
                MeetupPublished,
                MeetupCancelled,
                MemberJoined,
                MemberLeft,
                Waiting,
                Attending
            }
        }
    }
}