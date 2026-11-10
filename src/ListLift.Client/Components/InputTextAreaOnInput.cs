using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace ListLift.Client.Components;

public sealed class InputTextAreaOnInput : InputTextArea
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "textarea");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "name", NameAttributeValue);
        builder.AddAttribute(3, "class", CssClass);
        builder.AddAttribute(4, "value", CurrentValueAsString);
        builder.AddAttribute(5, "oninput", EventCallback.Factory.CreateBinder<string?>(
            this,
            value => CurrentValueAsString = value,
            CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");
        builder.CloseElement();
    }
}
