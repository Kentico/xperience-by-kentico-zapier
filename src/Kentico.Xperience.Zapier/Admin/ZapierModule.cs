using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Admin;
using Kentico.Xperience.Zapier.Triggers;

using Microsoft.Extensions.DependencyInjection;

[assembly: RegisterModule(type: typeof(ZapierModule))]

namespace Kentico.Xperience.Zapier.Admin;

internal class ZapierModule : Module
{
    private IZapierModuleInstaller? installer;
    private IZapierRegistrationService? zapierRegistrationService;


    public ZapierModule() : base(nameof(ZapierModule)) { }


    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        installer = services.GetRequiredService<IZapierModuleInstaller>();
        zapierRegistrationService = services.GetRequiredService<IZapierRegistrationService>();

        ApplicationEvents.Initialized.Execute += InitializeModule;
        ApplicationEvents.Initialized.Execute += InitZapierRegistrations;
        ZapierTriggerInfo.TYPEINFO.Events.Insert.After += (s, e) => AddNewZapWebhook(e);
        ZapierTriggerInfo.TYPEINFO.Events.Update.Before += (s, e) => Update(e);
        ZapierTriggerInfo.TYPEINFO.Events.Delete.After += (s, e) => RemoveZapWebhook(e);
    }


    private void InitZapierRegistrations(object? sender, EventArgs e)
    {
        var zapsToRegister = ZapierTriggerInfo.Provider.Get();

        foreach (var zapInfo in zapsToRegister)
        {
            zapierRegistrationService?.RegisterWebhook(zapInfo);
        }
    }


    private void Update(ObjectEventArgs e)
    {
        if (e.Object is ZapierTriggerInfo webhook)
        {
            zapierRegistrationService?.RegisterWebhook(webhook);
        }
    }


    private void RemoveZapWebhook(ObjectEventArgs e)
    {
        if (e.Object is ZapierTriggerInfo webhook)
        {
            zapierRegistrationService?.UnregisterWebhook(webhook);
        }
    }


    private void AddNewZapWebhook(ObjectEventArgs e)
    {
        if (e.Object is ZapierTriggerInfo webhook)
        {
            zapierRegistrationService?.RegisterWebhook(webhook);
        }
    }


    private void InitializeModule(object? sender, EventArgs e) => installer?.Install();
}
