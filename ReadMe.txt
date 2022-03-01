
--------------------------------------DOCKER------------------------------------------------------------------------

Use below command for docker build

	docker build -f Dockerfile -t web:is-booking-opened-api-doc . 
	
Docker Run Local
	
	docker run --env-file ./env.list -d -p 7788:8081 --name is-booking-opened-cont web:is-booking-opened-api-doc


--------------------------------------PSQL------------------------------------------------------------------------
For logging into heroku postgresql

Use the below Command
psql -h {HOSTNAME} -p {PORTNUMBER} -U {USERNAME} -W  {DB}


Create table
 	CREATE TABLE IF NOT EXISTS trigger 
		(trigger_id serial, movie_name VARCHAR(255), theater_name VARCHAR(255), theater_id VARCHAR(50),
		trigger_url VARCHAR(255) , trigger_status smallint, trigger_date VARCHAR(255),trigger_time VARCHAR(255),
		created_on TIMESTAMP,updated_on TIMESTAMP);

Sample Insert
 	INSERT INTO trigger (movie_name,theater_name,theater_id,trigger_url,trigger_status,trigger_date,trigger_time,created_on,updated_on) 
	VALUES ('fir','PVR','VRCM',
	'fir-chennai/movie-chen-ET00321951-MT/20220216',0,'2022-02-11', 
	'12:00 AM','2022-02-11 04:30:02','2022-02-13 04:30:01');

Sample Update
 	UPDATE trigger SET theater_id ='JACM' WHERE trigger_id = 1;

	UPDATE trigger SET trigger_url='veerame-vaagai-soodum-chennai/movie-chen-ET00314800-MT/20220215',
	theater_id ='AGSM' , trigger_time = '8:00 PM' WHERE trigger_id = 2;

Sample Delete
 	DELETE FROM trigger;

Sample Alter Sequence
	ALTER SEQUENCE trigger_trigger_id_seq RESTART WITH 1;


--------------------------------------HEROKU------------------------------------------------------------------------
Heroku deploy type 1

	heroku Login

	heroku container:login
		
	Heroku push
		heroku container:push -a postgresql-app-dev web:postgresql-web-app

		heroku container:push web:is-booking-opened-api-doc -a booking-opened-api 
		
	Heroku release 

		heroku container:release web -a booking-opened-api 


Heroku deploy type 2
	heroku Login

	heroku container:login

	docker build -f Dockerfile -t web:is-booking-opened-api-doc .

	docker login --username=_ --password=${heroku auth:token} registry.heroku.com 
	
	docker build -t registry.heroku.com/booking-opened-api/web .

	docker push registry.heroku.com/booking-opened-api/web

	heroku container:release web --app booking-opened-api