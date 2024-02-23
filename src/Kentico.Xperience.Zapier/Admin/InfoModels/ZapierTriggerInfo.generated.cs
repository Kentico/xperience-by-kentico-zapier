using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Integration.Zapier;

[assembly: RegisterObjectType(typeof(ZapierTriggerInfo), ZapierTriggerInfo.OBJECT_TYPE)]

namespace Kentico.Integration.Zapier
{
    /// <summary>
    /// Data container class for <see cref="ZapierTriggerInfo"/>.
    /// </summary>
    [Serializable]
    public partial class ZapierTriggerInfo : AbstractInfo<ZapierTriggerInfo, IZapierTriggerInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "kenticozapier.zapiertrigger";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ZapierTriggerInfoProvider), OBJECT_TYPE, "KenticoZapier.ZapierTrigger", "ZapierTriggerID", null, null, "ZapierTriggerCodeName", "ZapierTriggerDisplayName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
            },
        };


        /// <summary>
        /// Zapier trigger ID.
        /// </summary>
        [DatabaseField]
        public virtual int ZapierTriggerID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ZapierTriggerID)), 0);
            set => SetValue(nameof(ZapierTriggerID), value);
        }


        /// <summary>
        /// Zapier trigger display name.
        /// </summary>
        [DatabaseField]
        public virtual string ZapierTriggerDisplayName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ZapierTriggerDisplayName)), String.Empty);
            set => SetValue(nameof(ZapierTriggerDisplayName), value);
        }


        /// <summary>
        /// Zapier trigger code name.
        /// </summary>
        [DatabaseField]
        public virtual string ZapierTriggerCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ZapierTriggerCodeName)), String.Empty);
            set => SetValue(nameof(ZapierTriggerCodeName), value);
        }


        /// <summary>
        /// Zapier trigger enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool ZapierTriggerEnabled
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(ZapierTriggerEnabled)), false);
            set => SetValue(nameof(ZapierTriggerEnabled), value);
        }


        /// <summary>
        /// Zapier trigger object type.
        /// </summary>
        [DatabaseField]
        public virtual string mZapierTriggerObjectType
        {
            get => ValidationHelper.GetString(GetValue(nameof(ZapierTriggerObjectType)), String.Empty);
            set => SetValue(nameof(ZapierTriggerObjectType), value);
        }


        /// <summary>
        /// Zapier trigger object type.
        /// </summary>
        public IEnumerable<string> ZapierTriggerObjectType
        {
            get => global::CMS.DataEngine.Internal.JsonDataTypeConverter.ConvertToModels<string>(mZapierTriggerObjectType);
        }


        /// <summary>
        /// Zapier trigger object class type.
        /// </summary>
        [DatabaseField]
        public virtual string ZapierTriggerObjectClassType
        {
            get => ValidationHelper.GetString(GetValue(nameof(ZapierTriggerObjectClassType)), String.Empty);
            set => SetValue(nameof(ZapierTriggerObjectClassType), value);
        }


        /// <summary>
        /// Zapier trigger event type.
        /// </summary>
        [DatabaseField]
        public virtual string ZapierTriggerEventType
        {
            get => ValidationHelper.GetString(GetValue(nameof(ZapierTriggerEventType)), String.Empty);
            set => SetValue(nameof(ZapierTriggerEventType), value);
        }


        /// <summary>
        /// Zapier trigger zapier URL.
        /// </summary>
        [DatabaseField]
        public virtual string ZapierTriggerZapierURL
        {
            get => ValidationHelper.GetString(GetValue(nameof(ZapierTriggerZapierURL)), String.Empty);
            set => SetValue(nameof(ZapierTriggerZapierURL), value, String.Empty);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected ZapierTriggerInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="ZapierTriggerInfo"/> class.
        /// </summary>
        public ZapierTriggerInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ZapierTriggerInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ZapierTriggerInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
