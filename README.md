# Xperience by Kentico Zapier

[![Kentico Labs](https://img.shields.io/badge/Kentico_Labs-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik05NTYuMjg4IDgwNC40OEw2NDAgMjc3LjQ0VjY0aDMyYzE3LjYgMCAzMi0xNC40IDMyLTMycy0xNC40LTMyLTMyLTMyaC0zMjBjLTE3LjYgMC0zMiAxNC40LTMyIDMyczE0LjQgMzIgMzIgMzJIMzg0djIxMy40NEw2Ny43MTIgODA0LjQ4Qy00LjczNiA5MjUuMTg0IDUxLjIgMTAyNCAxOTIgMTAyNGg2NDBjMTQwLjggMCAxOTYuNzM2LTk4Ljc1MiAxMjQuMjg4LTIxOS41MnpNMjQxLjAyNCA2NDBMNDQ4IDI5NS4wNFY2NGgxMjh2MjMxLjA0TDc4Mi45NzYgNjQwSDI0MS4wMjR6IiAgLz48L3N2Zz4=)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#labs-limited-support) [![CI: Build and Test](https://github.com/Kentico/repo-template/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Kentico/repo-template/actions/workflows/ci.yml)

> [!WARNING]  
> This integration is currently preview only with limited Kentico Labs support. The Zapier application is in public beta, you can get access through this link: https://zapier.com/developer/public-invite/197164/a81b6e163e6b8128b380c7a72e10552d/
>
> Fully featured and supported Zapier integration is coming in Xperience by Kentico March Refresh


## Description

Zapier integration enables seamless connectivity between a wide range of web applications, allowing users to create automated workflows, or "Zaps," that trigger actions across different platforms based on specified conditions and triggers. This integration empowers users to streamline repetitive tasks, synchronize data, and improve productivity without requiring any coding knowledge, making it an invaluable tool for individuals and businesses alike.

## Screenshots

![List of zapier triggers](/images/screenshots/listofzapiertriggers.png "List of zapier triggers")
![Zap creation in zapier UI](/images/screenshots/zapieruitrigger.png "Zap creation in zapier UI")


## Library Version Matrix

This version supports content workflows added in version 28.3.0. Please ensure that your versions are up to date.

| Xperience Version | Library Version |
| ----------------- | --------------- |
| >= 28.3.*         | PREVIEW         |

### Dependencies

- [ASP.NET Core 8.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.xperience.io/xp/changelog)

## Package Installation

> [!WARNING]  
> The nuget is not published yet, include this respository into your project directly

Add the package to your application using the .NET CLI

```powershell
dotnet add package Kentico.Xperience.Zapier
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
    * Specify the domain of your application and the allowed objects that can interact with your Zapier triggers and handlers.

   ```csharp
       // appsettings.json
      "ZapierConfiguration": {
        "WebAdminDomain": "myzapierapp.com",
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

3. In the administration go to UI application 'Zapier'.
4. On the API Key menu generate new API key for your Zapier application.
    * Save the API key as it will appear only once.
5. Open your Zapier UI and create new Zap.
    * As a trigger pick Xperience by Kentico application
    * Select Event "Catch Xperience by Kentico Webhook"
    * Then you have to connect an Account using URL of your website (without trailing slash) and your generated API Key from previous step
    * After your connection, you can set configuration for your trigger. 
        * Name your trigger
        * Pick object in Xperience by Kentico which you want to set for an automation process
        * Select type Event for your object
    * Test your trigger
6. Now you can create your own workflow eg. connect with Gmail, Microsoft Teams, ...
    * You can find more information on https://zapier.com/workflows 
7. After you finish your automation workflow... Publication of your Zap will create an object in your Xperience by Kentico application.
    * You can find it in administration -> Zapier UI application -> List of Zapier triggers


## Full Instructions

### Configure your application
Prerequisites for using this integration are a public application running on Xperience by Kentico and a Zapier account.

In the beginning, it is necessary to **register services into the DI container** in your application and to **configure Zapier in the appropriate appsettings** file in your application.
These steps are displayed in the first two points of our Quick Start section.
 * In the appsettings configuration, you need to specify the domain of your application in the correct format, e.g., 'myzapierapp.com', and list the allowed objects.
 * Setting the allowed objects list in appsettings.json affects the object selection in the Zapier UI. Make sure you set up the appropriate objects for your application.

You can view sample of such configuration [here](./docs/Configuration-example.md).

### Zapier admin application
After configuring your application, a new application named 'Zapier' should appear in your application's administration.
This application provides a list of created Zapier triggers from the Zapier UI and a submodule for the API Key.
 * On the 'List Zapier Triggers' tab, you can view the list of active zapier triggers. Items cannot be manually changed in the Admin application. Therefore, you must (and should) manage your triggers directly from the Zapier UI.
 * On the 'API Key' tab, you can generate API Key for communication between your application and Zapier itself.
    * **Note that there is only one API Key per application**. It is crucial to securely store it and share it only with your zapier editors. Once the API key is generated, you can only view it once.


### Authentication in Zapier
To connect zapier with your application, you must authenticate your app first. 
 * In the Zapier UI, you will be asked for the **domain of your application**.
 * Also, you must provide the **API Key** for your application. 

The API Key is used consistently throughout the integration process. Only regenerate your API Key if you are aware of the potential consequences, such as invalidating existing Zapier triggers and actions connected to your application.


### Supported scenarios

In the Zapier user interface, two primary features are available: triggers and actions.
 * Zapier trigger is an event or an action that initiates a workflow or a sequence of actions in Zapier. 
   * Event in your Xperince by Kentico is the starting point of automation workflow.
 * Zapier action is a task or operation that Zapier performs as part of an automated workflow, triggered by an event in one application.
   * Action results in creating, updating or otherwise managing data in your Xperince by Kentico application.

Several scenarios that this integration allows are described below. The 'Catch Xperience by Kentico Webhook' trigger allows for more complex scenarios, offering numerous possibilities due to its generic design.

**BizForm** - triggers and actions interacting with Xperience by Kentico forms.
 * Trigger Create - Listening for form submission
   * 'Catch Xperience by Kentico Webhook' connected to specific form class and listening to 'Create' event.
 * Action 'Insert form record' - Enables to create a form submission based on third party apps eg. Google Forms or based on another source of data.


**Event Log** - interaction based on event logs insertion
 * Trigger Create - Listening for the creation of a record in the event log
    * 'Catch Xperience by Kentico Webhook' connected to CMS.EventLog type and listening to 'Create' event.

**Activity**
 * Trigger Create - Listening for some activity
   * Set up similarly to the previous examples using a trigger 'Catch Xperience by Kentico Webhook'

**Contact**
 * Trigger Create - Listening for the creation of a contact eg. to send it to CRM
   * Set up similarly to the previous examples using a trigger 'Catch Xperience by Kentico Webhook'

**Page, headless item, reusable item**
* Trigger Create - Listening for creation page or reusable item
  * Set up similarly to the previous examples using a trigger 'Catch Xperience by Kentico Webhook'
* Trigger 'Move to step' - Listening for a workflow change of a specific object (custom content workflow)
  * Set up similarly to the previous examples using a 'Catch Xperience by Kentico Webhook' trigger, but with a workflow step event.
* Action 'Move to step' - Pages, headless items, or reusable items can be moved to a specific step within a custom content workflow for the respective object.
* Action 'Publish' -  Pages, headless items, or reusable items can be published using this action


### Trigger Catch Xperience by Kentico Webhook
The 'Catch Xperience by Kentico Webhook' trigger, being a complex feature, facilitates listening on multiple events such as Create, Update, and Delete (Enabled for Info objects, BizForm objects, Pages, Headless, Reusable).

Additionally, Pages, headless items, and reusable items can also monitor the publish event.
Furthermore, if any of these types are under custom workflows, you can also listen for specific custom step events.
For instance, detecting when a Page is moved to the prepublished step in a custom content workflow step created manually.

With this general settings it is possible to cover a large number of use cases.

## Contributing

To see the guidelines for Contributing to Kentico open source software, please see [Kentico's `CONTRIBUTING.md`](https://github.com/Kentico/.github/blob/main/CONTRIBUTING.md) for more information and follow the [Kentico's `CODE_OF_CONDUCT`](https://github.com/Kentico/.github/blob/main/CODE_OF_CONDUCT.md).

Instructions and technical details for contributing to **this** project can be found in [Contributing Setup](./docs/Contributing-Setup.md).

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more information.

## Support

[![7-day bug-fix policy](https://img.shields.io/badge/-7--days_bug--fixing_policy-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik04ODguNDkgMjIyLjY4NnYtMzEuNTRsLTY1LjY3Mi0wLjk1NWgtMC4yMDVhNDY1LjcxNSA0NjUuNzE1IDAgMCAxLTE0NC4zMTUtMzEuMzM0Yy03Ny4wMDUtMzEuMTk4LTEyNi4yOTQtNjYuNzY1LTEyNi43MDMtNjcuMTA3bC0zOS44LTI4LjY3Mi0zOS4xODUgMjguNDY4Yy0yLjA0OCAxLjUwMS00OS45MDMgMzYuMDQ0LTEyNi45MDggNjcuMzFhNDQ3LjQyIDQ0Ny40MiAwIDAgMS0xNDQuNTIgMzEuMzM1bC02NS44NzcgMC45NTZ2Mzc4Ljg4YzAgODcuMDQgNDkuODM0IDE4NC42NjEgMTM3LjAxIDI2Ny44MSAzNy41NDcgMzUuODQgNzkuMjU4IDY2LjM1NSAxMjAuODMzIDg4LjIgNDMuMjggMjIuNzMzIDg0LjI0IDM0LjYxMiAxMTguODUyIDM0LjYxMiAzNC40MDYgMCA3NS43NzYtMTIuMTUyIDExOS42MDMtMzUuMTU4YTU0Ny45NzcgNTQ3Ljk3NyAwIDAgMCAxMjAuMDEzLTg3LjY1NCA1MTUuMjA5IDUxNS4yMDkgMCAwIDAgOTYuMTg4LTEyMi44OGMyNy4xMDItNDkuNTYyIDQwLjgyMy05OC4zMDQgNDAuODIzLTE0NC45OTlsLTAuMTM2LTM0Ny4yMDR6TTUxMC4wOSAxNDMuNDI4bDEuNzA2LTEuMzY1IDEuNzc1IDEuMzY1YzUuODAzIDQuMTY1IDU5LjUyOSA0MS44NDggMTQwLjM1NiA3NC43NTIgNzkuMTkgMzIuMDg2IDE1My42IDM1LjYzNSAxNjcuNjYzIDM2LjA0NWwyLjU5NCAwLjA2OCAwLjIwNSAzMTUuNzM0YzAuMTM3IDY5LjQ5NS00Mi41OTggMTUwLjE4Ni0xMTcuMDc3IDIyMS40NTdDNjQxLjU3IDg1NC4yODkgNTYzLjEzIDg5Ni40NzggNTEyIDg5Ni40NzhjLTIzLjY4OSAwLTU1LjU3LTkuODk5LTg5LjcwMi0yNy43ODVhNDc4LjgyMiA0NzguODIyIDAgMCAxLTEwNS42MDktNzcuMjc4QzI0Mi4yMSA3MjAuMjEzIDE5OS40NzUgNjM5LjUyMiAxOTkuNDc1IDU2OS44OVYyNTQuMjI1bDIuNzMtMC4xMzZjMy4yNzggMCA4Mi42MDQtMS41MDIgMTY3LjY2NC0zNS45NzdhNzM5Ljk0MiA3MzkuOTQyIDAgMCAwIDE0MC4yMi03NC42MTV2LTAuMDY5eiIgIC8+PHBhdGggZD0iTTcxMy4zMTggMzY4LjY0YTMyLjIyMiAzMi4yMjIgMCAwIDAtNDUuMzI5IDBMNDQ5LjE5NSA1ODcuNDM1bC05My4xODQtOTMuMTE2YTMyLjIyMiAzMi4yMjIgMCAwIDAtNDUuMzMgMCAzMi4yMjIgMzIuMjIyIDAgMCAwIDAgNDUuMjZsMTE1Ljg1IDExNS44NWEzMi4yOSAzMi4yOSAwIDAgMCA0NS4zMjggMEw3MTMuMzIgNDEzLjlhMzIuMjIyIDMyLjIyMiAwIDAgMCAwLTQ1LjMzeiIgIC8+PC9zdmc+)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#full-support)

This project has **Full support by 7-day bug-fix policy**.

See [`SUPPORT.md`](https://github.com/Kentico/.github/blob/main/SUPPORT.md#full-support) for more information.

For any security issues see [`SECURITY.md`](https://github.com/Kentico/.github/blob/main/SECURITY.md).
