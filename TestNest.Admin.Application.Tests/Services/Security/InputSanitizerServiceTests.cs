using TestNest.Admin.Application.Services;

namespace TestNest.Admin.Application.Tests.Services.Security;

public class InputSanitizerServiceTests
{
    private readonly InputSanitizerService _sanitizer;

    public InputSanitizerServiceTests()
    {
        _sanitizer = new InputSanitizerService();
    }

    #region Sanitize Tests

    [Fact]
    public void Sanitize_NullInput_ReturnsEmptyString()
    {
        // Act
        var result = _sanitizer.Sanitize(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Sanitize_EmptyInput_ReturnsEmptyString()
    {
        // Act
        var result = _sanitizer.Sanitize(string.Empty);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Sanitize_NormalText_ReturnsSameText()
    {
        // Arrange
        var input = "Hello World";

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Sanitize_TextWithWhitespace_ReturnsTrimmedText()
    {
        // Arrange
        var input = "  Hello World  ";

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Sanitize_HtmlTags_ReturnsEncodedText()
    {
        // Arrange
        var input = "<script>alert('xss')</script>";

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
    }

    [Fact]
    public void Sanitize_NullBytes_RemovesNullBytes()
    {
        // Arrange
        var input = "Hello\0World";

        // Act
        var result = _sanitizer.Sanitize(input);

        // Assert
        Assert.Equal("HelloWorld", result);
    }

    #endregion

    #region SanitizeHtml Tests

    [Fact]
    public void SanitizeHtml_ScriptTag_RemovesScriptContent()
    {
        // Arrange
        var input = "Hello <script>alert('xss')</script> World";

        // Act
        var result = _sanitizer.SanitizeHtml(input);

        // Assert
        Assert.DoesNotContain("script", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", result);
    }

    [Fact]
    public void SanitizeHtml_JavaScriptProtocol_RemovesJavaScript()
    {
        // Arrange
        var input = "javascript:alert('xss')";

        // Act
        var result = _sanitizer.SanitizeHtml(input);

        // Assert
        Assert.DoesNotContain("javascript:", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SanitizeHtml_EventHandler_RemovesEventHandler()
    {
        // Arrange
        var input = "<img src='x' onerror='alert(1)'>";

        // Act
        var result = _sanitizer.SanitizeHtml(input);

        // Assert
        Assert.DoesNotContain("onerror", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SanitizeHtml_AllHtmlTags_RemovesTags()
    {
        // Arrange
        var input = "<div><p>Hello</p><span>World</span></div>";

        // Act
        var result = _sanitizer.SanitizeHtml(input);

        // Assert
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
    }

    #endregion

    #region SanitizeSql Tests

    [Fact]
    public void SanitizeSql_SingleQuotes_EscapesSingleQuotes()
    {
        // Arrange
        var input = "O'Brien";

        // Act
        var result = _sanitizer.SanitizeSql(input);

        // Assert
        Assert.Equal("O''Brien", result);
    }

    [Fact]
    public void SanitizeSql_SqlKeywords_RemovesSqlKeywords()
    {
        // Arrange
        var input = "SELECT * FROM users";

        // Act
        var result = _sanitizer.SanitizeSql(input);

        // Assert - SELECT is a SQL keyword that gets removed
        Assert.DoesNotContain("SELECT", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SanitizeSql_Semicolons_RemovesSemicolons()
    {
        // Arrange
        var input = "value; DROP TABLE users;";

        // Act
        var result = _sanitizer.SanitizeSql(input);

        // Assert
        Assert.DoesNotContain(";", result);
    }

    [Fact]
    public void SanitizeSql_SqlComments_RemovesComments()
    {
        // Arrange
        var input = "value -- comment";

        // Act
        var result = _sanitizer.SanitizeSql(input);

        // Assert
        Assert.DoesNotContain("--", result);
    }

    [Fact]
    public void SanitizeSql_BlockComments_RemovesBlockComments()
    {
        // Arrange
        var input = "value /* comment */";

        // Act
        var result = _sanitizer.SanitizeSql(input);

        // Assert
        Assert.DoesNotContain("/*", result);
        Assert.DoesNotContain("*/", result);
    }

    #endregion

    #region ContainsPotentialXss Tests

    [Fact]
    public void ContainsPotentialXss_NullInput_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.ContainsPotentialXss(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ContainsPotentialXss_NormalText_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.ContainsPotentialXss("Hello World");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ContainsPotentialXss_ScriptTag_ReturnsTrue()
    {
        // Act
        var result = _sanitizer.ContainsPotentialXss("<script>alert('xss')</script>");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsPotentialXss_JavaScriptProtocol_ReturnsTrue()
    {
        // Act
        var result = _sanitizer.ContainsPotentialXss("javascript:alert('xss')");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsPotentialXss_EventHandler_ReturnsTrue()
    {
        // Act
        var result = _sanitizer.ContainsPotentialXss("<img onerror='alert(1)'>");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsPotentialXss_DataProtocol_ReturnsTrue()
    {
        // Act
        var result = _sanitizer.ContainsPotentialXss("data:text/html,<script>alert('xss')</script>");

        // Assert
        Assert.True(result);
    }

    #endregion

    #region ContainsPotentialSqlInjection Tests

    [Fact]
    public void ContainsPotentialSqlInjection_NullInput_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.ContainsPotentialSqlInjection(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ContainsPotentialSqlInjection_NormalText_ReturnsFalse()
    {
        // Act
        var result = _sanitizer.ContainsPotentialSqlInjection("Hello World");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("SELECT * FROM users")]
    [InlineData("INSERT INTO users")]
    [InlineData("UPDATE users SET")]
    [InlineData("DELETE FROM users")]
    [InlineData("DROP TABLE users")]
    [InlineData("UNION SELECT")]
    [InlineData("EXEC sp_executesql")]
    public void ContainsPotentialSqlInjection_SqlKeywords_ReturnsTrue(string input)
    {
        // Act
        var result = _sanitizer.ContainsPotentialSqlInjection(input);

        // Assert
        Assert.True(result);
    }

    #endregion
}
