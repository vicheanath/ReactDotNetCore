# Layouts & Shared Components

There's no special "layout" concept to learn — a layout is just a React component you import. This is
one of the advantages of React-as-views: composition is ordinary React.

## A shared layout

Create a `components/Layout.tsx`:

```tsx
import type { ReactNode } from "react";

export default function Layout({ active, children }: { active?: string; children: ReactNode }) {
  return (
    <div>
      <header>
        <nav>
          <a href="/">Home</a>
          <a href="/Dashboard">Dashboard</a>
          <a href="/Users">Users</a>
        </nav>
      </header>
      <main>{children}</main>
    </div>
  );
}
```

Use it from any view:

```tsx
import Layout from "../components/Layout";

export default function Dashboard(props: DashboardProps) {
  return (
    <Layout active="dashboard">
      <h1>Dashboard</h1>
      {/* ... */}
    </Layout>
  );
}
```

> Only files in `Views/` are registered as views. Shared components live elsewhere (e.g.
> `components/`) and are imported — they are not addressable as views themselves.

## Partial views / shared widgets

Same idea — extract any reusable piece into a component and import it. There's no registry entry and
no controller needed for shared components; they're bundled with whatever view imports them.

```tsx
// components/StatCard.tsx
export function StatCard({ label, value }: { label: string; value: string }) {
  return <div className="card"><span>{label}</span><strong>{value}</strong></div>;
}
```

## Styling with Tailwind CSS

Import your CSS from `entry-client.tsx` so Vite bundles it and the engine links it automatically:

```tsx
// entry-client.tsx
import "./styles/globals.css";
import { createRegistry } from "@react-dotnetcore/runtime";
import { mount } from "@react-dotnetcore/runtime/client";

const registry = createRegistry(import.meta.glob("./Views/*.tsx", { eager: true }));
mount(registry);
```

Set up Tailwind the standard way (`tailwind.config.cjs`, `postcss.config.cjs`, `@tailwind` directives
in `globals.css`). Point the `content` globs at your views and components:

```js
// tailwind.config.cjs
module.exports = {
  content: ["./components/**/*.{ts,tsx}", "./Views/**/*.{ts,tsx}", "./entry-client.tsx"],
  theme: { extend: {} },
  plugins: [],
};
```

In production the generated CSS is emitted as an asset and the engine injects a `<link>` into every
page `<head>` (from the Vite manifest). In development Vite injects styles via HMR. Either way the
class names are present in the server-rendered HTML, so there's no flash of unstyled content.

## shadcn/ui

[shadcn/ui](https://ui.shadcn.com) components are copy-pasted React components on Tailwind — they
work unchanged. Put them under `components/ui/` and the `cn()` helper under `lib/utils.ts`, then
import them from your views. The `samples/ReactDotNetCore.Sample` project includes a full dashboard
built this way (Card, Button, Badge, lucide-react icons).

## Important: SSR-safe components

Views render on the server first, so component bodies must not touch browser-only globals during
render. Guard or defer browser APIs:

```tsx
import { useEffect, useState } from "react";

export default function Widget() {
  const [w, setW] = useState(0);
  useEffect(() => setW(window.innerWidth), []); // runs only after hydration, in the browser
  return <span>{w}</span>;
}
```

Reading `window`/`document`/`localStorage` directly in the component body will throw during SSR — use
`useEffect` (client-only) or check `typeof window !== "undefined"`.
