CREATE TABLE "HowlDev.User" (
  email varchar(200) UNIQUE PRIMARY KEY NOT NULL, 
  displayName varchar(80) NOT NULL,
  role int4 NOT NULL
);

CREATE TABLE "HowlDev.Key" (
  id int4 PRIMARY KEY GENERATED ALWAYS AS IDENTITY NOT NULL,
  email varchar(200) references "HowlDev.User" (email) NOT NULL, 
  apiKey varchar(20) NOT NULL,
  validatorToken varchar(40) NOT NULL,
  validatedOn timestamp NULL
);
