# EmailAuthenticator

This authenticator provides a few interfaces, an AuthService, and an IdentityMiddleware. You need to 
implement an EmailService that actually sends the email. 

Recommended API layout in `Program.cs`: 

```csharp
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<IIDMiddlewareConfig, IDMiddlewareConfig>();
builder.Services.AddSingleton<IEmailService, EmailService>();

var app = builder.Build();

app.UseMiddleware<IdentityMiddleware>();
```

Important headers: 
```
Email-Auth-Email: {{email}}
Email-Auth-ApiKey: {{key}}
```

## SQL

This SQL is a requirement for the Service to work properly. Since I'm using Dapper, you need 
a Postgres database with the following SQL tables: 

```sql
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
```

This can be added to as much as you like to encode more information (for example in the User table), 
but this is what's required to make my auth work. 

## Upcoming Features

A few more features are coming before I consider the library done. 

- Method that gets rid of all lines in the Key table that are over time (opt-in, for your use)
- Integration tests with the Docker Compose to run full auth flows

## Changelog

0.9 (4/30/25)

- BREAKING CHANGE: Authentication Headers have had their (_) replaced with (-), inline with the [official documentation](https://www.rfc-editor.org/rfc/rfc9110.html#section-16.3.1-6.2) for HTML. This was messing with one of our Kubernetes assignments, after which I looked at the documentation and found this. I'm sorry, I won't make this mistake again. 
- Actually gave the expected SQL layout for the middleware/service to work properly. Sorry!

0.8.6 (4/10/25)

- Forgot to add nullability check. ExpirationDate now works correctly.

0.8.5 (4/10/25)

- BREAKING CHANGE: IIDMiddlewareConfig has an additional value, ReValidationDate (nullable), to reset the valid timer for that key. 
- Expiration Date in IIDMiddlewareConfig is now nullable

0.8 (3/31/25)

- Init