namespace EmailAuthenticator;

/// <summary>
/// Base class for the Email Auth system. 
/// </summary>
public class EmailAccount {
    /// <summary>
    /// Email of the user (PRIMARY KEY).
    /// </summary>
    public string Email { get; set; } = "";
    /// <summary>
    /// Display name for the user. Defaults to whatever is before the @ in 
    /// their email.
    /// </summary>
    public string DisplayName { get; set; } = "";
    /// <summary>
    /// Role of the user. This can be used as a level system (leveraging numbers and &lt;&gt;=) 
    /// signs or run through an Enum to assign specific types. 
    /// </summary>
    public int Role { get; set; }
}
