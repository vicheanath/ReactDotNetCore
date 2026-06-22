import { useState } from "react";
import { ArrowRight, LayoutDashboard, UserRound, Users } from "lucide-react";
import Layout from "./components/Layout";
import { cn } from "./lib/utils";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./components/ui/card";
import { Button, buttonVariants } from "./components/ui/button";

export interface HomeProps {
  appName: string;
  userCount: number;
}

const features = [
  { href: "/Dashboard", icon: LayoutDashboard, title: "Dashboard", desc: "Stat cards, a chart, and recent sales — all shadcn/ui." },
  { href: "/Users", icon: Users, title: "Users", desc: "A server-rendered shadcn table of users." },
  { href: "/Users/Detail/1", icon: UserRound, title: "Profile", desc: "A user profile card with a hydration demo." },
];

export default function Home(props: HomeProps) {
  const [count, setCount] = useState(0);

  return (
    <Layout active="home">
      <div className="space-y-2">
        <h1 className="text-3xl font-bold tracking-tight">{props.appName}</h1>
        <p className="max-w-2xl text-muted-foreground">
          React components rendered as ASP.NET Core MVC views — server-side rendered and hydrated, with
          no separate API or SPA. Every page here is a React view styled with shadcn/ui.
        </p>
      </div>

      <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {features.map((f) => (
          <Card key={f.href} className="flex flex-col">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <f.icon className="h-5 w-5 text-muted-foreground" />
                {f.title}
              </CardTitle>
              <CardDescription>{f.desc}</CardDescription>
            </CardHeader>
            <CardContent className="mt-auto">
              <a href={f.href} className={cn(buttonVariants({ variant: "outline", size: "sm" }))}>
                Open <ArrowRight className="h-4 w-4" />
              </a>
            </CardContent>
          </Card>
        ))}
      </div>

      <Card className="mt-4">
        <CardHeader>
          <CardTitle>Hydration demo</CardTitle>
          <CardDescription>The markup is server-rendered; this button only works after hydration.</CardDescription>
        </CardHeader>
        <CardContent>
          <Button onClick={() => setCount((c) => c + 1)}>Clicked {count} times</Button>
        </CardContent>
      </Card>
    </Layout>
  );
}
