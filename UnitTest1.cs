using System.ComponentModel.DataAnnotations;

[assembly: CaptureConsole]
[assembly: CaptureTrace]

namespace validation_bug_repro;

public class UnitTest1
{
    [Fact]
    public void GivenTwoUrisButOneRelativeUri_AfterValidating_TheValidationFailsWithValidationResult()
    {
        // Arrange
        var relativeUri = "/NotAbsoluteUri";
        var input = new CorsConfiguration
        {
            AllowedOrigins = [ "https://www.example.com", relativeUri ]
        };
        var validationContext = new ValidationContext(input);
        var validationResults = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(input, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        /*
        isValid.ShouldBeFalse();
        validationResults.ShouldHaveSingleItem();
        ValidationResult failure = validationResults.First();
        failure.ErrorMessage.ShouldNotBeNull();
        failure.ErrorMessage.ShouldContain(relativeUri);
        failure.ErrorMessage.ShouldContain("cannot be parsed as an absolute Uri");
        failure.MemberNames.ShouldHaveSingleItem();
        failure.MemberNames.First().ShouldBe(nameof(input.AllowedOrigins));
        */
    }
    
    public class CorsConfiguration : IValidatableObject
    {
        public const string SectionName = "Cors";
        public const string CorsAllowedOrigins = $"{SectionName}:AllowedOrigins";

        public const string NoOriginsErrorMessage = $"The '{nameof(AllowedOrigins)}' list must contain at least one entry.";

        public IList<string> AllowedOrigins { get; init; } = new List<string>();

        public bool AllowedOriginsContainAny => AllowedOrigins.Contains("*");

        // This class uses System.ComponentModel.DataAnnotations.IValidatableObject for custom validation
        // The alternative is Microsoft.Extensions.Options.IValidateOptions<T>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AllowedOrigins.Count == 0)
            {
                Console.WriteLine("No origins were provided.");
                yield return new ValidationResult(NoOriginsErrorMessage, [ nameof(AllowedOrigins) ]);
                Console.WriteLine("No origins were provided. Just before break");
                yield break;
            }

            if (AllowedOriginsContainAny)
            {
                Console.WriteLine("Allowed origins contain any '*'.");
                yield break;
            }

            foreach (var origin in AllowedOrigins)
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out _))
                {
                    Console.WriteLine($"Entry could not be parsed as a valid absolute URI: '{origin}'");
                    yield return new ValidationResult(errorMessage: $"Origin '{origin}' in configuration path '{CorsAllowedOrigins}' cannot be parsed as an absolute Uri",
                        memberNames:[ nameof(AllowedOrigins) ]);
                }
            }
            Console.WriteLine("End of Validate method");
        }
    }
}
