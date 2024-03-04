
using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Admin;
using Microsoft.Extensions.DependencyInjection;

[assembly: RegisterModule(type: typeof(ZapierModule))]

namespace Kentico.Xperience.Zapier.Admin;

internal class ZapierModule : Module
{
    private IZapierModuleInstaller? installer;

    public ZapierModule() : base(nameof(ZapierModule)) { }


    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        installer = services.GetRequiredService<IZapierModuleInstaller>();

        ApplicationEvents.Initialized.Execute += InitializeModule;
        ApplicationEvents.Initialized.Execute += InitZapierRegistrations;
        ZapierTriggerInfo.TYPEINFO.Events.Insert.After += (s, e) => AddNewZapWebhook(e);
        ZapierTriggerInfo.TYPEINFO.Events.Update.Before += (s, e) => UpdateIfEnabled(e);
        ZapierTriggerInfo.TYPEINFO.Events.Delete.After += (s, e) => RemoveZapWebhook(e);
    }

    private void InitZapierRegistrations(object? sender, EventArgs e)
    {
        var zapsToRegister = ZapierTriggerInfoProvider.ProviderObject.Get().WhereTrue(nameof(ZapierTriggerInfo.ZapierTriggerEnabled));

        foreach (var zapInfo in zapsToRegister)
        {
            ZapierHelper.RegisterWebhook(zapInfo);
        }
    }

    private void UpdateIfEnabled(ObjectEventArgs e)
    {
        if (e.Object is ZapierTriggerInfo webhook && webhook.ChangedColumns().Contains(nameof(webhook.ZapierTriggerEnabled)))
        {
            if (webhook.ZapierTriggerEnabled)
            {
                ZapierHelper.RegisterWebhook(webhook);
            }
            else
            {
                ZapierHelper.UnregisterWebhook(webhook);
            }
        }
    }


    private void RemoveZapWebhook(ObjectEventArgs e)
    {
        if (e.Object is ZapierTriggerInfo webhook)
        {
            ZapierHelper.UnregisterWebhook(webhook);
        }
    }

    private void AddNewZapWebhook(ObjectEventArgs e)
    {
        if (e.Object is ZapierTriggerInfo webhook)
        {
            ZapierHelper.RegisterWebhook(webhook);
        }
    }


    private void InitializeModule(object? sender, EventArgs e) => installer?.Install();

}
