# MarkedTextBox

A text box control with an associated symbol (mark) that indicates the type of input expected from the user. Inherits from `TextBox` and adds a `Mark` property for displaying an icon or indicator.

## Basic usage

```xml
<MarkedTextBox Watermark="Enter email"
              Mark="{DynamicResource EmailIcon}" />
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Mark` | `object?` | — | Symbol or indicator showing the expected input type (e.g., icon, image) |

## Common marks

Use icons to indicate input types:

```xml
<!-- Email input -->
<MarkedTextBox Watermark="Email address"
              Mark="{DynamicResource MailIcon}" />

<!-- Password input -->
<MarkedTextBox Watermark="Password"
              Mark="{DynamicResource LockIcon}"
              PasswordChar="*" />

<!-- Phone input -->
<MarkedTextBox Watermark="Phone number"
              Mark="{DynamicResource PhoneIcon}" />

<!-- Search input -->
<MarkedTextBox Watermark="Search"
              Mark="{DynamicResource SearchIcon}" />
```

## Custom marks

The `Mark` property accepts any object, allowing for custom content:

```xml
<!-- Text mark -->
<MarkedTextBox Mark="@" />

<!-- PathIcon mark -->
<MarkedTextBox Mark="{DynamicResource UserIcon}" />

<!-- Image mark -->
<MarkedTextBox Mark="{Binding UserAvatar}" />
```

## Styling

The mark is typically displayed in the text box's template. Refer to the control's theme for styling options.

## Example

```xml
<StackPanel Spacing="8" Width="300">
    <TextBlock Text="Login" FontWeight="Bold" />
    <MarkedTextBox Watermark="Username or email"
                  Mark="{DynamicResource UserIcon}" />
    <MarkedTextBox Watermark="Password"
                  Mark="{DynamicResource LockIcon}"
                  PasswordChar="*" />
</StackPanel>
```
