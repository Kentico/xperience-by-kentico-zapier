### Configure your application

Prerequisites for using this integration are a public Xperience by Kentico application and a Zapier account.

In the beginning, it is necessary to **register services into the DI container** in your application and to **configure Zapier in the appropriate appsettings** file in your application.

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// ...

builder.Services.AddKenticoZapier();

// ...

var app = builder.Build();

// ...

app.UseAuthorization(); //place under app.UseKentico()
```

In the appsettings configuration, you need to specify the list of the allowed objects which affects the object selection in the Zapier UI. Make sure you set up the appropriate objects for your application.

You can view a sample of such configuration [here](./docs/Configuration-example.md).

### Zapier administration application

After configuring your application, a new application named **Zapier** should appear in your application's administration.
This application consists of two submodules: a listing page of created Zapier triggers from the Zapier UI and a submodule for managing the API Key.

- On the _List Zapier Triggers_ tab, you can view the list of created Zapier triggers. Items cannot be manually changed or added in the Admin application. Therefore, you must (and should) manage your triggers directly from the Zapier UI. A trigger record appears in the list after Zapier's zap publication. After turning the zap off, the corresponding record is automatically deleted.
- On the 'API Key' tab, you can generate an API Key for communication between your application and Zapier itself.
  - **Note that there is only one API Key per application**. It is crucial to securely store it and share it only with your Zapier editors. Once the API key is generated, you can only view it once.
  - Generation of a new API Key can be done by all global administrators of the system and the users with the assigned permission **Generate** for _Zapier_ application.
    ![Generate permission](/images/screenshots/zapiergeneratepermission.png "Generate permission")

### Authentication in Zapier

To connect Zapier with your application, you must authenticate your app first.

- In the Zapier UI, you will be asked for the **domain of your application**.
- Also, you must provide the **API Key** for your application.

The API Key is used consistently throughout the integration process. Only regenerate your API Key if you are aware of the potential consequences, such as invalidating existing Zapier triggers and actions connected to your application.

### Supported scenarios

In the Zapier user interface, two primary features are available: triggers and actions.

- A Zapier trigger is an event that initiates a workflow or a sequence of actions in Zapier.
  - The event in your Xperience by Kentico is the starting point of the automation workflow.
- A Zapier action is a task or operation that Zapier performs as part of an automated workflow, triggered by an event in one application.
  - The action results in creating, updating, or otherwise managing data in your Xperience by Kentico application.

#### Supported trigger events

**Content moves to a workflow step**

- _Triggers when content item moves to a step._
- Only moves to a _custom_ workflow step (created in _Workflows_ application) are supported.
- To set up a trigger, you need to choose a **specific content type** (page, headless item, or reusable item). After that, the relevant workflow steps are loaded, and you need to select one **specific workflow step**. The event triggers when any content item of the chosen content type is moved **to** the selected workflow step.
- The trigger record consists of:
  - DisplayName - content item's display name
  - ContentTypeName - name of content type
  - StepName - new workflow step's display name
  - OriginalStepName - previous workflow step's display name
  - UserID
  - AdminLink - link to the moved content item in administration
  - UserName
  - DateTime

**New Event Log Entry**

- _Triggers when a new event log is inserted._
- In the trigger configuration step, you need to select at least one event log **severity**. It is allowed to select multiple severities.
- The trigger record contains all event log info detail fields.
  ![Fields in trigger record](/images/screenshots/zapiereventlogdetail.png "Fields in trigger record")

**New Form Submission**

- _Triggers when a new form submission is created._
- In the trigger configuration step, you need to select a specific form.
- The trigger record contains:
  - common field _FormInserted_ - time of submission
  - form fields specific for individual forms

#### Supported action events

**Insert Form Record**

- _Inserts a record into an XbyK form._
- In the action configuration step, a specific XbyK form needs to be selected. After that, the form fields are dynamically loaded. Usually, you want to map trigger record's fields to those form fields.
- Note that only key–value fields are supported. It's not possible to attach or upload a file to the form record.
