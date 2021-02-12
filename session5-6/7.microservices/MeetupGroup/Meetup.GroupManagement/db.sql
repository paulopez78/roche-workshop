SELECT * FROM meetup.group_management."MeetupGroups";
SELECT * FROM meetup.group_management."Members";
SELECT * FROM meetup.group_management."Outbox";

DELETE FROM meetup.group_management."MeetupGroups";
DELETE FROM meetup.group_management."Members";
DELETE FROM meetup.group_management."Outbox";

SELECT G."Id", G."Title", G."Slug", G."Description", G."Location", M."Id", M."UserId",  M."JoinedAt"
FROM meetup.group_management."MeetupGroups" G  LEFT JOIN meetup.group_management."Members" M on M."GroupId" = G."Id"
WHERE G."Slug" = 'netcorebcn';

