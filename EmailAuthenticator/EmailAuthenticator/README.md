# EmailAuthenticator

This authenticator provides a few interfaces, an AuthService, and an IdentityMiddleware. You need to 
implement an EmailService that actually sends the email. 

Recommended API layout in `Program.cs`: 

```
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<IIDMiddlewareConfig, IDMiddlewareConfig>();
builder.Services.AddSingleton<IEmailService, EmailService>();

var app = builder.Build();

app.UseMiddleware<IdentityMiddleware>();
```

## Changelog

0.8.5 (4/10/25)

- BREAKING CHANGE: IIDMiddlewareConfig has an additional value, ReValidationDate (nullable), to reset the valid timer for that key. 
- Expiration Date in IIDMiddlewareConfig is now nullable

0.8 (3/31/25)

- Init