using System;

namespace Meetup.GroupManagement.Data
{
    public class MeetupGroup
    {
        public Guid           Id          { get; set; }
        public Guid           OrganizerId { get; set; }
        public string         Slug        { get; set; }
        public string         Title       { get; set; }
        public string         Location    { get; set; }
        public string         Description { get; set; }
        public DateTimeOffset FoundedAt   { get; set; }
        public GroupStatus    Status      { get; set; } = GroupStatus.Active;
    }

    public enum GroupStatus
    {
        Active,
        Archived,
    }

    public class GroupMember
    {
        public int            Id       { get; set; }
        public Guid           UserId   { get; set; }
        public Guid           GroupId  { get; set; }
        public MemberStatus   Status   { get; set; } = MemberStatus.Active;
        public DateTimeOffset JoinedAt { get; set; }
        public Role           Role     { get; set; } = Role.Member;
    }

    public enum MemberStatus
    {
        Active,
        WaitingForApproval
    }

    public enum Role
    {
        Organizer,
        Member
    }

    public record CommandResult(Guid GroupId, string GroupSlug);
}