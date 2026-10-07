using CMS.DataEngine;
using CMS.Tests;

namespace Kentico.Xperience.Zapier.Tests.Support;

internal static class FakeExtensions
{
    /// <summary>
    /// Returns the faked generic provider typed as <see cref="IInfoProvider{TInfo}"/> so it can be injected into tested code.
    /// </summary>
    public static IInfoProvider<TInfo> Provider<TInfo>(this IInfoProviderFake<TInfo, IInfoProvider<TInfo>> fake)
        where TInfo : AbstractInfoBase<TInfo>, new() =>
        (IInfoProvider<TInfo>)fake.ProviderObject;
}
