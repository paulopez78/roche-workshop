SELECT * FROM "MeetupGroups" INNER JOIN "Members" M on "MeetupGroups"."Status" = M."Status";


SELECT M."Id"
FROM "MeetupEvent" M
         LEFT JOIN "AttendantList" AL ON M."Id" = AL."MeetupEventId"
         LEFT JOIN "Attendant" A on AL."Id" = A."AttendantListAggregateId"
WHERE M."Status"!='Finished' AND M."GroupId" = '99053ddd-42b8-4826-a904-69a09e40f1df' AND A."MemberId" = '5279ed6a-582b-44df-947e-cd1ac678f454'

SELECT M."Id",M."Status",M."IsOnline",AL."Status",A."MemberId", A."Waiting"
FROM "MeetupEvent" M
         LEFT JOIN "AttendantList" AL ON M."Id" = AL."MeetupEventId"
         LEFT JOIN "Attendant" A on AL."Id" = A."AttendantListAggregateId"
ORDER BY A."AddedAt";

SELECT * FROM "MeetupGroups";

DELETE FROM "Members";
DELETE FROM "MeetupGroups";
DELETE FROM "Outbox";

DELETE FROM "Attendant";
DELETE FROM "AttendantList";
DELETE FROM "MeetupEvent";
DELETE FROM "Outbox";