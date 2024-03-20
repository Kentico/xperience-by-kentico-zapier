# Usage Guide

## Configure your application
Prerequisites for using this integration are a public application running on Xperience by Kentico and a Zapier account.

In the beginning, it is necessary to **register services into the DI container** in your application and to **configure Zapier in the appropriate appsettings** file in your application.
These steps are displayed in the first two points of our Quick Start section.
 * In the appsettings configuration, you need to specify the domain of your application in the correct format, e.g., 'myzapierapp.com', and list the allowed objects.
 * Setting the allowed objects list in appsettings.json affects the object selection in the Zapier UI. Make sure you set up the appropriate objects for your application.

You can view [sample of such configuration](Configuration-example.md).

## Zapier admin application
After configuring your application, a new application named 'Zapier' should appear in your application's administration.
This application provides a list of created Zapier triggers from the Zapier UI and a submodule for the API Key.
 * On the 'List Zapier Triggers' tab, you can view the list of active zapier triggers. Items cannot be manually changed in the Admin application. Therefore, you must (and should) manage your triggers directly from the Zapier UI.
 * On the 'API Key' tab, you can generate API Key for communication between your application and Zapier itself.
    * **Note that there is only one API Key per application**. It is crucial to securely store it and share it only with your zapier editors. Once the API key is generated, you can only view it once.


## Authentication in Zapier
To connect zapier with your application, you must authenticate your app first. 
 * In the Zapier UI, you will be asked for the **domain of your application**.
 * Also, you must provide the **API Key** for your application. 

The API Key is used consistently throughout the integration process. Only regenerate your API Key if you are aware of the potential consequences, such as invalidating existing Zapier triggers and actions connected to your application.


## Supported scenarios

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


<!--
## Trigger Catch Xperience by Kentico Webhook
The 'Catch Xperience by Kentico Webhook' trigger, being a complex feature, facilitates listening on multiple events such as Create, Update, and Delete (Enabled for Info objects, BizForm objects, Pages, Headless, Reusable).

Additionally, Pages, headless items, and reusable items can also monitor the publish event.
Furthermore, if any of these types are under custom workflows, you can also listen for specific custom step events.
For instance, detecting when a Page is moved to the prepublished step in a custom content workflow step created manually.

With this general settings it is possible to cover a large number of use cases.
-->