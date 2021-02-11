SELECT M."Id",M."Status",AL."Id",AL."MeetupEventId", AL."Status",A."MemberId", A."Waiting"
FROM "MeetupEvent" M
         LEFT JOIN "AttendantList" AL ON M."Id" = AL."MeetupEventId"
         LEFT JOIN "Attendant" A on AL."Id" = A."AttendantListAggregateId"
