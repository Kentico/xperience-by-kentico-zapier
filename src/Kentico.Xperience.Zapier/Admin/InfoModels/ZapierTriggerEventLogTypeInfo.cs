using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Integration.Zapier;

[assembly: RegisterObjectType(typeof(ZapierTriggerEventLogTypeInfo), ZapierTriggerEventLogTypeInfo.OBJECT_TYPE)]

namespace Kentico.Integration.Zapier;

/// <summary>
/// Data container class for <see cref="ZapierTriggerEventLogTypeInfo"/>.
/// </summary>
[Serializable]
public partial class ZapierTriggerEventLogTypeInfo : AbstractInfo<ZapierTriggerEventLogTypeInfo, IInfoProvider<ZapierTriggerEventLogTypeInfo>>, IInfoWithId
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticozapier.zapiertriggereventlogtype";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<ZapierTriggerEventLogTypeInfo>), OBJECT_TYPE, "KenticoZapier.ZapierTriggerEventLogType", nameof(ZapierTriggerEventLogTypeID), null, null, null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new("ZapierTriggerEventLogTypeZapierTriggerID", "kenticozapier.zapiertrigger", ObjectDependencyEnum.Required),
        },
    };


    /// <summary>
    /// Zapier trigger event log type ID.
    /// </summary>
    [DatabaseField]
    public virtual int ZapierTriggerEventLogTypeID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(ZapierTriggerEventLogTypeID)), 0);
        set => SetValue(nameof(ZapierTriggerEventLogTypeID), value);
    }


    /// <summary>
    /// Zapier trigger event log type zapier trigger ID.
    /// </summary>
    [DatabaseField]
    public virtual int ZapierTriggerEventLogTypeZapierTriggerID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(ZapierTriggerEventLogTypeZapierTriggerID)), 0);
        set => SetValue(nameof(ZapierTriggerEventLogTypeZapierTriggerID), value);
    }


    /// <summary>
    /// Zapier trigger event log type type.
    /// </summary>
    [DatabaseField]
    public virtual string ZapierTriggerEventLogTypeType
    {
        get => ValidationHelper.GetString(GetValue(nameof(ZapierTriggerEventLogTypeType)), string.Empty);
        set => SetValue(nameof(ZapierTriggerEventLogTypeType), value);
    }


    /// <summary>
    /// Deletes the object using appropriate provider.
    /// </summary>
    protected override void DeleteObject() => Provider.Delete(this);


    /// <summary>
    /// Updates the object using appropriate provider.
    /// </summary>
    protected override void SetObject() => Provider.Set(this);

    /// <summary>
    /// Creates an empty instance of the <see cref="ZapierTriggerEventLogTypeInfo"/> class.
    /// </summary>
    public ZapierTriggerEventLogTypeInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="ZapierTriggerEventLogTypeInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public ZapierTriggerEventLogTypeInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
