SELECT M."Id",M."Status",AL."Id",AL."MeetupEventId", AL."Status",A."MemberId", A."Waiting"
FROM "MeetupEvent" M
         LEFT JOIN "AttendantList" AL ON M."Id" = AL."MeetupEventId"
         LEFT JOIN "Attendant" A on AL."Id" = A."AttendantListAggregateId"
ORDER BY A."AddedAt";
         
SELECT * FROM "Outbox";

SELECT M."Id", M."GroupId", M."Title", M."Description", M."Status", AL."Id" AS AttendantListId, AL."Capacity", AL."Status" AS AttendantListStatus, A."Id", A."MemberId", A."AddedAt", A."Waiting" 
FROM "MeetupEvent" M 
LEFT JOIN "AttendantList" AL on M."Id" = AL."MeetupEventId" 
LEFT JOIN "Attendant" A on AL."Id" = A."AttendantListAggregateId" ORDER BY A."AddedAt";

DELETE FROM "Attendant";
DELETE FROM "AttendantList";
DELETE FROM "MeetupEvent";
DELETE FROM "Outbox";
