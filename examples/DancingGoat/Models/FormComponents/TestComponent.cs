using CMS.DataEngine;
using DancingGoat.Models.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

[assembly: RegisterFormComponent(MyRichTestFormComponent.IDENTIFIER, typeof(MyRichTestFormComponent), "RichTextHTML component", Description = "This is a custom form component.", IconClass = "icon-newspaper")]


namespace DancingGoat.Models.FormComponents;



public class MyRichTestFormComponent : FormComponent<MyRichTestComponentProperties, string>
{
    public const string IDENTIFIER = "MyRichTestFormComponent";

    [BindableProperty]
    public string Value { get; set; }

    // Gets the value of the form field instance passed from a view where the instance is rendered
    public override string GetValue() => Value;

    // Sets the default value of the form field instance
    public override void SetValue(string value) => Value = value;
}


public class MyRichTestComponentProperties : FormComponentProperties<string>
{
    [RichTextEditorComponent(Label = "Default value", Order = EditingComponentOrder.DEFAULT_VALUE)]
    public override string DefaultValue
    {
        get;
        set;
    }


    // Initializes a new instance of the properties class and configures the underlying database field
    public MyRichTestComponentProperties()
        : base(FieldDataType.RichTextHTML, 500)
    {
    }
}
