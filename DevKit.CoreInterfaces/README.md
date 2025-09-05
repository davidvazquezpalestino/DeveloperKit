# Core Interfaces for DeveloperKit

[![NuGet](https://img.shields.io/nuget/v/DevKit.CoreInterfaces.svg?style=flat-square)](https://www.nuget.org/packages/DevKit.CoreInterfaces/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://dotnet.microsoft.com/)

CoreInterfaces is a foundational library that provides common abstractions and standard contracts for .NET applications, enabling consistent implementation patterns across the DeveloperKit ecosystem.

## ✨ Features

- **Standardized Response Objects**: Consistent patterns for API responses and process results
- **Form Controls**: Base interfaces for UI components and forms
- **State Management**: Interfaces for managing read-only and mutable states
- **Web API Support**: Standardized response formats for web APIs
- **Type Safety**: Strongly-typed interfaces with generic support
- **Dependency Injection**: First-class support for .NET Core DI

## 🚀 Getting Started

### Prerequisites

- .NET Standard 2.0+ or .NET 6.0+
- Visual Studio 2022 or VS Code with C# Dev Kit (recommended)

### Installation

```bash
dotnet add package DevKit.CoreInterfaces
```

## 🛠 Core Components

### Response Interfaces

#### `IProcessResponse<T>`

Standard response object for operation results with success/failure state and messages.

```csharp
public interface IProcessResponse<T>
{
    T Data { get; set; }
    ProcessResult ProcessResult { get; set; }
    string SuccessMessage { get; set; }
    string ErrorMessage { get; set; }
}
```

#### `IWebApiResponse<T>`

Standardized response format for Web API endpoints.

```csharp
public interface IWebApiResponse<T>
{
    bool Success { get; set; }
    string Message { get; set; }
    T Data { get; set; }
    List<string> Errors { get; set; }
}
```

### Form Controls

#### `IForm`
Base interface for form controls.

```csharp
public interface IForm
{
    void Clear();
    void LoadData(object data);
    bool Validate();
}
```

#### `IPrincipal`
Interface for principal UI components.

### State Management

#### `IReadOnly`
Interface for read-only state management.

```csharp
public interface IReadOnly
{
    bool IsReadOnly { get; set; }
    void SetReadOnly(bool readOnly);
}
```

## 💻 Usage Examples

### Creating a Standard API Response

```csharp
public IWebApiResponse<User> GetUser(int userId)
{
    try
    {
        var user = _userRepository.GetById(userId);
        if (user == null)
        {
            return new WebApiResponse<User>
            {
                Success = false,
                Message = "User not found",
                Errors = new List<string> { $"User with ID {userId} not found" }
            };
        }

        return new WebApiResponse<User>
        {
            Success = true,
            Data = user,
            Message = "User retrieved successfully"
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving user {UserId}", userId);
        return new WebApiResponse<User>
        {
            Success = false,
            Message = "An error occurred while retrieving the user",
            Errors = new List<string> { ex.Message }
        };
    }
}
```

### Implementing a Read-Only Form

```csharp
public class UserDetailsForm : IForm, IReadOnly
{
    private TextBox _nameTextBox;
    private TextBox _emailTextBox;
    private bool _isReadOnly;

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            _isReadOnly = value;
            UpdateReadOnlyState();
        }
    }

    public void Clear()
    {
        _nameTextBox.Text = string.Empty;
        _emailTextBox.Text = string.Empty;
    }

    public void LoadData(object data)
    {
        if (data is User user)
        {
            _nameTextBox.Text = user.Name;
            _emailTextBox.Text = user.Email;
        }
    }

    public bool Validate()
    {
        // Validation logic here
        return true;
    }

    private void UpdateReadOnlyState()
    {
        _nameTextBox.ReadOnly = _isReadOnly;
        _emailTextBox.ReadOnly = _isReadOnly;
    }
}
```

## 📚 API Reference

### Enums

#### `ProcessResult`
- `Success`: Operation completed successfully
- `Warning`: Operation completed with warnings
- `Error`: Operation failed with errors
- `ValidationError`: Operation failed due to validation errors

### Interfaces

| Interface | Description |
|-----------|-------------|
| `IForm` | Base interface for form controls |
| `IPrincipal` | Interface for principal UI components |
| `IReadOnly` | Manages read-only state |
| `IProcessResponse<T>` | Standard response for operations |
| `IWebApiResponse<T>` | Standard response for Web APIs |

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please read our [contributing guidelines](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## 📫 Support

For support, please open an issue in our [issue tracker](https://github.com/davidvazquezpalestino/DeveloperKit/issues).

---

<div align="center">
  Made with ❤️ by the DeveloperKit Team
</div>
// Implementación de IWebApiResponse
public class WebApiResponse<T> : IWebApiResponse<T>
{
    public T Data { get; set; }
    public bool IsSuccessful { get; set; }
    public string SuccessMessage { get; set; }
    public string ErrorMessage { get; set; }
}

// Uso de IForm
public class MyForm : IForm
{
    public Type Form { get; set; }
}

// Uso de IReadOnly
public class MyReadOnlyComponent : IReadOnly
{
    public bool ReadOnly { get; set; }
    public bool IgnoreReadOnly { get; set; }
}
```
