import type { ReactNode } from "react";
import { cn } from "../lib/utils";
import { buttonVariants } from "./ui/button";

// Shared application shell used by every React view: a nav bar + a centered content container.
export default function Layout({ active, children }: { active?: string; children: ReactNode }) {
  const links = [
    { href: "/", label: "Home", key: "home" },
    { href: "/Dashboard", label: "Dashboard", key: "dashboard" },
    { href: "/Users", label: "Users", key: "users" },
    { href: "/Privacy", label: "Privacy", key: "privacy" },
  ];

  return (
    <div className="min-h-screen bg-background text-foreground">
      <header className="sticky top-0 z-10 border-b bg-background/80 backdrop-blur">
        <nav className="mx-auto flex max-w-6xl items-center gap-1 px-6 py-3">
          <a href="/" className="mr-auto flex items-center gap-2 font-bold">
            <span className="text-lg">⚛️</span> ReactDotNetCore
          </a>
          {links.map((l) => (
            <a
              key={l.key}
              href={l.href}
              className={cn(buttonVariants({ variant: l.key === active ? "secondary" : "ghost", size: "sm" }))}
            >
              {l.label}
            </a>
          ))}
        </nav>
      </header>
      <main className="mx-auto max-w-6xl px-6 py-8">{children}</main>
    </div>
  );
}
