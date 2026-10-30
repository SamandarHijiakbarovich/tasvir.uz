namespace Tasvir.SharedKernel;

public enum ErrorKind
{
    Validation,
    Unauthorized,
    NotFound,
    Conflict,
    TooManyRequests,
}

/// <summary>Kutilgan biznes xatosi. Xabar foydalanuvchiga ko‘rsatiladi, Fields — maydonlar bo‘yicha xatolar.</summary>
public sealed record Error(string Code, string Message, ErrorKind Kind, IReadOnlyDictionary<string, string>? Fields = null);

public readonly record struct Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;

    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(value, null);
    public static Result<T> Fail(Error error) => new(default, error);

    public static implicit operator Result<T>(T value) => Ok(value);
    public static implicit operator Result<T>(Error error) => Fail(error);
}

/// <summary>Qiymat qaytarmaydigan amallar uchun.</summary>
public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
