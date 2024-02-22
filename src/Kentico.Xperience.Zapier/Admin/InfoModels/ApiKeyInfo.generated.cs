using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Integration.Zapier;

[assembly: RegisterObjectType(typeof(ApiKeyInfo), ApiKeyInfo.OBJECT_TYPE)]

namespace Kentico.Integration.Zapier
{
    /// <summary>
    /// Data container class for <see cref="ApiKeyInfo"/>.
    /// </summary>
    [Serializable]
    public partial class ApiKeyInfo : AbstractInfo<ApiKeyInfo, IApiKeyInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "kenticozapier.apikey";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ApiKeyInfoProvider), OBJECT_TYPE, "KenticoZapier.ApiKey", "ApiKeyID", "ApiKeyCreated", null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Api key ID.
        /// </summary>
        [DatabaseField]
        public virtual int ApiKeyID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ApiKeyID)), 0);
            set => SetValue(nameof(ApiKeyID), value);
        }


        /// <summary>
        /// Api key key.
        /// </summary>
        [DatabaseField]
        public virtual string ApiKeyToken
        {
            get => ValidationHelper.GetString(GetValue(nameof(ApiKeyToken)), String.Empty);
            set => SetValue(nameof(ApiKeyToken), value, String.Empty);
        }


        /// <summary>
        /// Api key created.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ApiKeyCreated
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(ApiKeyCreated)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(ApiKeyCreated), value);
        }

        /// <summary>
        /// Api key created by.
        /// </summary>
        [DatabaseField]
        public virtual int ApiKeyCreatedBy
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ApiKeyCreatedBy)), 0);
            set => SetValue(nameof(ApiKeyCreatedBy), value);
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
        protected ApiKeyInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="ApiKeyInfo"/> class.
        /// </summary>
        public ApiKeyInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ApiKeyInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ApiKeyInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
