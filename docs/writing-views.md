# Writing Views

A view is an ordinary React component with a `default` export, living in `Views/*.tsx`. The file
name is the component name used everywhere (`Views/UserProfile.tsx` → `UserProfile`).

```tsx
// Views/UserProfile.tsx
export interface UserProfileProps {
  id: number;
  name: string;
  email: string;
}

export default function UserProfile(props: UserProfileProps) {
  return (
    <main>
      <h1>{props.name}</h1>
      <p>{props.email}</p>
    </main>
  );
}
```

## Returning a view from a controller

There are two equivalent ways:

```csharp
// Strongly typed — requires a marker type (see below). Recommended.
return this.ReactView<UserProfile>(model);

// String based — no marker needed; handy for dynamic component names.
return this.ReactView("UserProfile", model);
```

Both return a `ReactDotNetCoreResult` (an `ActionResult`), so they compose with everything MVC
expects.

### Setting the page title

```csharp
var result = this.ReactView<UserProfile>(model);
result.Title = "Profile — Acme";
return result;
```

If you don't set it, the title defaults to the component name.

## Marker types

`ReactView<T>()` needs a C# type whose **name matches the component**. Implement the empty
`IReactComponent` marker interface:

```csharp
using ReactDotNetCore;
namespace MyApp;

public sealed class UserProfile : IReactComponent;
public sealed class Dashboard   : IReactComponent;
public sealed class Orders      : IReactComponent;
```

Keep them together in one file (e.g. `ReactComponents.cs`). They carry no logic — they exist so the
compiler can map `ReactView<Dashboard>()` to the `Dashboard.tsx` component by name.

> Prefer not to maintain markers by hand? Use the string overload `this.ReactView("Dashboard", model)`
> instead — it skips the marker entirely.

## Model → props

The model you pass is serialized to JSON and handed to the component as its props. Serialization
uses **camelCase** by default (`JsonSerializerDefaults.Web`), so:

```csharp
public sealed class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime JoinedUtc { get; set; }
}
```

arrives in the component as:

```ts
interface UserProfileProps {
  id: number;
  name: string;
  joinedUtc: string; // DateTime is serialized as an ISO 8601 string
}
```

Notes:

- **Dates** become ISO strings — parse with `new Date(props.joinedUtc)` on the client.
- **Collections** become arrays. Wrap a list in a view-model object so props is an object, not a
  bare array:

  ```csharp
  public sealed class UsersViewModel { public IReadOnlyList<UserDto> Users { get; set; } = []; }
  return this.ReactView<Users>(new UsersViewModel { Users = list });
  ```
  ```tsx
  export default function Users(props: { users: UserDto[] }) { /* ... */ }
  ```

- The **exact same props bytes** are used for SSR and embedded for hydration, so server and client
  always render identically.

You can customize serialization globally via `JsonOptions` — see [Configuration](configuration.md).

## Interactivity & hydration

Server rendering produces static HTML; **hydration** makes it interactive in the browser. Hooks,
event handlers, and state all work once hydrated:

```tsx
import { useState } from "react";

export default function Counter(props: { start: number }) {
  const [n, setN] = useState(props.start);
  return <button onClick={() => setN((x) => x + 1)}>Clicked {n} times</button>;
}
```

The button renders on the server with its initial value and becomes clickable after the client
bundle loads. Because both renders use identical props, there is no hydration mismatch.

## Linking between views

These are plain server-rendered MVC routes, so just use normal links and full-page navigation:

```tsx
<a href="/Users/Detail/7">Open profile</a>
```

No client-side router is required (or included). Each navigation is a fresh server render + hydrate.

## Naming rules

- One component per file, `export default`.
- The registry key is the file's base name without extension (`Orders.tsx` → `Orders`).
- Component (and marker) names are case-sensitive and must match the `.tsx` file name.
- Only files directly under `Views/` are registered by the default glob (`./Views/*.tsx`). To include
  subfolders, widen the glob in `entry-server.tsx`/`entry-client.tsx` to `./Views/**/*.tsx`.
