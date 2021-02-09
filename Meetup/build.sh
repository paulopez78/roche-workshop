image=paulopez/meetup-events:2.0

docker build -f Dockerfile -t $image .


#172.17.0.2
docker run -d \
-e POSTGRES_PASSWORD=mysecretpassword \
-v $HOME/postgres/data:/var/lib/postgresql/data postgres

#172.17.0.3
docker run --name=meetup \
-d \
-e ASPNETCORE_ENVIRONMENT=Development \
-e ConnectionString="Host=172.17.0.2;Port=5432;Database=meetupevents;Username=postgres;Password=mysecretpassword" \
-p 5000:80 $image

