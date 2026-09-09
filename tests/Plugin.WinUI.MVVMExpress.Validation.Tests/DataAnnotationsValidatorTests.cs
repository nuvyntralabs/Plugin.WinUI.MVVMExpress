using System.ComponentModel.DataAnnotations;
using Plugin.WinUI.MVVMExpress.Validation;

namespace Plugin.WinUI.MVVMExpress.Validation.Tests;

public sealed class DataAnnotationsValidatorTests
{
    [Fact]
    public void Validation_ExposesPackageIdentity()
    {
        Assert.Equal("Plugin.WinUI.MVVMExpress.Validation", ValidationMarker.PackageId);
    }

    [Fact]
    public void Validate_Required_FailsWhenEmpty()
    {
        var summary = DataAnnotationsValidator.Instance.Validate(new Named { Name = "" });
        Assert.False(summary.IsValid);
        Assert.Contains(summary.Messages, item => item.PropertyName == nameof(Named.Name));
        Assert.Contains("Name", summary.ToString());
    }

    [Fact]
    public async Task ValidateAsync_SucceedsWhenValid()
    {
        var summary = await DataAnnotationsValidator.Instance.ValidateAsync(new Named { Name = "Ada" });
        Assert.True(summary.IsValid);
        Assert.Equal(string.Empty, summary.ToString());
    }

    [Fact]
    public void TrimDescriptor_RootsSupportedAttributes()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "ILLink.Descriptors.xml");
        Assert.True(File.Exists(path), "ILLink.Descriptors.xml was not copied to the test output.");
        var xml = File.ReadAllText(path);
        foreach (var name in new[]
                 {
                     "RequiredAttribute",
                     "StringLengthAttribute",
                     "MinLengthAttribute",
                     "MaxLengthAttribute",
                     "RangeAttribute",
                     "RegularExpressionAttribute",
                     "EmailAddressAttribute",
                     "CompareAttribute",
                     "MustMatchAttribute",
                     "DataAnnotationsValidator"
                 })
        {
            Assert.Contains(name, xml, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void MustMatch_FailsWhenDifferent()
    {
        var summary = DataAnnotationsValidator.Instance.Validate(new Passwords { Password = "a", Confirm = "b" });
        Assert.False(summary.IsValid);
        Assert.Contains(summary.Messages, item => item.PropertyName == nameof(Passwords.Confirm));
    }

    private sealed class Named
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; init; } = "";
    }

    private sealed class Passwords
    {
        public string Password { get; init; } = "";

        [MustMatch(nameof(Password))]
        public string Confirm { get; init; } = "";
    }
}
