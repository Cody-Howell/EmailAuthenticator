namespace EmailAuthenticator;

public interface IIDMiddlewareConfig {
    /// <summary>
    /// Set to the list of paths you want the middleware to exclude authorization
    /// </summary>
    List<string> Paths { get; }

    /// <summary>
    /// Set to the timespan that API keys are valid for. 
    /// </summary>
    TimeSpan ExpirationDate { get; }
}
