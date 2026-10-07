using CMS.DataEngine;
using CMS.Tests;

using Kentico.Xperience.Zapier.Actions;
using Kentico.Xperience.Zapier.Common;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Zapier.Tests.Actions;

/// <summary>
/// Contract of the endpoints behind the form dropdown and the "Insert Form Record" action.
/// </summary>
[TestFixture]
public class DataAndActionControllersTests : UnitTests
{
    private const string ALLOWED_FORM = "BizForm.DancingGoatContactUs";
    private const string OTHER_FORM = "BizForm.NotAllowed";
    private const string FORM_DEFINITION = "<form><field column=\"UserEmail\" columntype=\"text\" /></form>";


    [SetUp]
    public void SetUp() =>
        Fake<DataClassInfo, DataClassInfoProvider>().WithData(
            NewDataClass(1, ALLOWED_FORM, "Contact us", ClassType.FORM, FORM_DEFINITION),
            NewDataClass(2, OTHER_FORM, "Not allowed", ClassType.FORM, FORM_DEFINITION),
            NewDataClass(3, "BizForm.EmptyDefinition", "Empty", ClassType.FORM, string.Empty),
            NewDataClass(4, "DancingGoat.Cafe", "Cafe", ClassType.CONTENT_TYPE, FORM_DEFINITION));


    [Test]
    public void GetFormTypes_ReturnsOnlyAllowedFormClasses()
    {
        var controller = new ZapierDataController(Options([ALLOWED_FORM, "BizForm.EmptyDefinition", "DancingGoat.Cafe"]));

        var result = controller.GetFormTypes().Value!.ToList();

        // Content types and forms outside AllowedObjects are excluded.
        Assert.That(result, Is.EquivalentTo(new[]
        {
            new SelectOptionItem(ALLOWED_FORM, "Contact us"),
            new SelectOptionItem("BizForm.EmptyDefinition", "Empty"),
        }));
    }


    [Test]
    public void GetFormFields_ReturnsFormDefinitionXmlForAllowedForm()
    {
        var controller = NewActionController([ALLOWED_FORM]);

        var result = controller.GetFormFields(ALLOWED_FORM);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo(FORM_DEFINITION));
        });
    }


    [Test]
    public void GetFormFields_MatchesAllowedObjectsCaseInsensitively()
    {
        var controller = NewActionController([ALLOWED_FORM.ToUpperInvariant()]);

        Assert.That(controller.GetFormFields(ALLOWED_FORM), Is.TypeOf<OkObjectResult>());
    }


    [Test]
    public void GetFormFields_RejectsFormOutsideAllowedObjects()
    {
        var controller = NewActionController([ALLOWED_FORM]);

        Assert.That(controller.GetFormFields(OTHER_FORM), Is.TypeOf<BadRequestResult>());
    }


    [Test]
    public void GetFormFields_RejectsAllowedClassThatIsNotAForm()
    {
        var controller = NewActionController(["DancingGoat.Cafe"]);

        Assert.That(controller.GetFormFields("DancingGoat.Cafe"), Is.TypeOf<BadRequestResult>());
    }


    [Test]
    public void GetFormFields_RejectsFormWithoutDefinition()
    {
        var controller = NewActionController(["BizForm.EmptyDefinition"]);

        Assert.That(controller.GetFormFields("BizForm.EmptyDefinition"), Is.TypeOf<BadRequestResult>());
    }


    private static ActionFormInsertController NewActionController(IEnumerable<string> allowedObjects) =>
        new(Substitute.For<ILogger<ActionFormInsertController>>(), Options(allowedObjects));


    private static IOptionsMonitor<ZapierConfiguration> Options(IEnumerable<string> allowedObjects)
    {
        var monitor = Substitute.For<IOptionsMonitor<ZapierConfiguration>>();
        monitor.CurrentValue.Returns(new ZapierConfiguration { AllowedObjects = [.. allowedObjects] });
        return monitor;
    }


    private static DataClassInfo NewDataClass(int id, string className, string displayName, string classType, string formDefinition)
    {
        var dataClass = DataClassInfo.New();
        dataClass.ClassID = id;
        dataClass.ClassName = className;
        dataClass.ClassDisplayName = displayName;
        dataClass.ClassType = classType;
        dataClass.ClassFormDefinition = formDefinition;
        return dataClass;
    }
}
