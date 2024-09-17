- Start RabbitMQ in Docker:

  `docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.13-management`

- Start each service in seperate shells.

- Go to http://localhost:5162

- Fill out form and submit

- Verify that checked "Booking" only get send to EmailService and that "Cancel" and "Booking" are sent to BackOffice. 

