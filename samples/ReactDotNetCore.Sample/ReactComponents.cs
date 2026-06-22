using ReactDotNetCore;

namespace ReactDotNetCore.Sample;

// Strongly-typed markers for React views, enabling ReactView<UserProfile>(model).
// One marker per Views/*.tsx component (a source generator could emit these automatically).

public sealed class Home : IReactComponent;
public sealed class Dashboard : IReactComponent;
public sealed class Privacy : IReactComponent;
public sealed class Users : IReactComponent;
public sealed class UserProfile : IReactComponent;
public sealed class Error : IReactComponent;
