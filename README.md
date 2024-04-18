# Xperience by Kentico Zapier

[![Kentico Labs](https://img.shields.io/badge/Kentico_Labs-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik05NTYuMjg4IDgwNC40OEw2NDAgMjc3LjQ0VjY0aDMyYzE3LjYgMCAzMi0xNC40IDMyLTMycy0xNC40LTMyLTMyLTMyaC0zMjBjLTE3LjYgMC0zMiAxNC40LTMyIDMyczE0LjQgMzIgMzIgMzJIMzg0djIxMy40NEw2Ny43MTIgODA0LjQ4Qy00LjczNiA5MjUuMTg0IDUxLjIgMTAyNCAxOTIgMTAyNGg2NDBjMTQwLjggMCAxOTYuNzM2LTk4Ljc1MiAxMjQuMjg4LTIxOS41MnpNMjQxLjAyNCA2NDBMNDQ4IDI5NS4wNFY2NGgxMjh2MjMxLjA0TDc4Mi45NzYgNjQwSDI0MS4wMjR6IiAgLz48L3N2Zz4=)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#labs-limited-support)

> [!WARNING]  
> This integration is currently preview only with limited Kentico Labs support. The Zapier application is in public beta here: https://zapier.com/apps/xperience-by-kentico/integrations
>
> Updated and fully supported Zapier integration is coming in Xperience by Kentico April Refresh

## Description

Zapier integration enables seamless connectivity between a wide range of web applications, allowing users to create automated workflows, or "Zaps," that trigger actions across different platforms based on specified conditions and triggers. This integration empowers users to streamline repetitive tasks, synchronize data, and improve productivity without requiring any coding knowledge, making it an invaluable tool for individuals and businesses alike.

## Screenshots

![List of zapier triggers](/images/screenshots/listofzapiertriggers.png "List of zapier triggers")
![Trigger events](/images/screenshots/zapieruitriggerevents.png "Available trigger events")
![Action events](/images/screenshots/zapieruiactionevents.png "Available action events")
![API Key page](/images/screenshots/zapierapikey.png "API Key page")
![API Key generate page](/images/screenshots/zapierapikeygenerate.png "API Key generate page")

## Library Version Matrix

This version supports content workflows added in version 28.3.0. Please ensure that your versions are up to date.

| Xperience Version | Library Version |
| ----------------- | --------------- |
| >= 28.3.*         | 0.1.0 PREVIEW   |

### Dependencies

- [ASP.NET Core 8.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.xperience.io/xp/changelog)

## Package Installation

Add the package to your application using the .NET CLI

```powershell
dotnet add package Kentico.Xperience.Zapier --prerelease
```

## Quick Start (with trigger example)

1. Register required services into DI container.

   ```csharp
   // Program.cs
   var builder = WebApplication.CreateBuilder(args);

   // ...

   builder.Services.AddKenticoZapier();

   // ...

   var app = builder.Build();

   // ...

   app.UseAuthorization() //place under app.UseKentico()
   ```

2. Configure Zapier using ZapierConfiguration in appsettings.json of your application.

   - Specify allowed objects that can interact with your Zapier triggers and handlers.

   ```csharp
       // appsettings.json
      "ZapierConfiguration": {
        "AllowedObjects": [
          "cms.user",
          "CMS.EventLog",
           // ...
          "DancingGoat.Banner",
          "DancingGoat.ConfirmationPage",
           // ...
          "BizForm.DancingGoatContactUs",
        ]
      }
   ```

3. In the administration go to UI application 'Zapier' located at 'Configuration' section.
4. On the API Key menu generate new API key for your Zapier application.
   - Save the API key as it will appear only once.
5. Open your Zapier UI and create new Zap.
   - As a trigger pick _Xperience by Kentico_ application
   - In the first step select event of your choice
   - Then you have to connect an Account using URL of your website (without trailing slash) and your generated API Key from previous step
   - In the third step you need to set configuration for your trigger
   - Test your trigger
6. Now you can create your own workflow eg. connect with Gmail, Microsoft Teams, ...
   - You can find more information on https://zapier.com/workflows
7. Publication of your Zap will create an object in your Xperience by Kentico application.
   - You can find the list of created triggers at Zapier application at page _List of Zapier triggers_

## Full Instructions

View the [Usage Guide](./docs/Usage-Guide.md) for more detailed instructions.

## Contributing

To see the guidelines for Contributing to Kentico open source software, please see [Kentico's `CONTRIBUTING.md`](https://github.com/Kentico/.github/blob/main/CONTRIBUTING.md) for more information and follow the [Kentico's `CODE_OF_CONDUCT`](https://github.com/Kentico/.github/blob/main/CODE_OF_CONDUCT.md).

Instructions and technical details for contributing to **this** project can be found in [Contributing Setup](./docs/Contributing-Setup.md).

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more information.

## Support

[![Kentico Labs](https://img.shields.io/badge/Kentico_Labs-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik05NTYuMjg4IDgwNC40OEw2NDAgMjc3LjQ0VjY0aDMyYzE3LjYgMCAzMi0xNC40IDMyLTMycy0xNC40LTMyLTMyLTMyaC0zMjBjLTE3LjYgMC0zMiAxNC40LTMyIDMyczE0LjQgMzIgMzIgMzJIMzg0djIxMy40NEw2Ny43MTIgODA0LjQ4Qy00LjczNiA5MjUuMTg0IDUxLjIgMTAyNCAxOTIgMTAyNGg2NDBjMTQwLjggMCAxOTYuNzM2LTk4Ljc1MiAxMjQuMjg4LTIxOS41MnpNMjQxLjAyNCA2NDBMNDQ4IDI5NS4wNFY2NGgxMjh2MjMxLjA0TDc4Mi45NzYgNjQwSDI0MS4wMjR6IiAgLz48L3N2Zz4=)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#labs-limited-support)

This project has **Kentico Labs limited support** while in PREVIEW version. You can expect our 7-day bug-fix policy support when full version is released.

See [`SUPPORT.md`](https://github.com/Kentico/.github/blob/main/SUPPORT.md#full-support) for more information.

For any security issues see [`SECURITY.md`](https://github.com/Kentico/.github/blob/main/SECURITY.md).
