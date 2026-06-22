import { useState } from "react";
import { ArrowLeft, ThumbsUp } from "lucide-react";
import Layout from "./components/Layout";
import { cn } from "./lib/utils";
import { Avatar, AvatarFallback } from "./components/ui/avatar";
import { Badge } from "./components/ui/badge";
import { Button, buttonVariants } from "./components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "./components/ui/card";

// Props are produced by serializing the C# model (UserDto) to camelCase JSON.
export interface UserProfileProps {
  id: number;
  name: string;
  email: string;
  role: string;
  joinedUtc: string;
}

export default function UserProfile(props: UserProfileProps) {
  // Local state that only works once the client has hydrated — a visible proof of hydration.
  const [likes, setLikes] = useState(0);

  return (
    <Layout active="users">
      <a href="/Users" className={cn(buttonVariants({ variant: "ghost", size: "sm" }), "mb-4")}>
        <ArrowLeft className="h-4 w-4" /> Back to users
      </a>

      <Card className="max-w-xl">
        <CardHeader className="flex flex-row items-center gap-4 space-y-0">
          <Avatar className="h-12 w-12">
            <AvatarFallback>{initials(props.name)}</AvatarFallback>
          </Avatar>
          <div className="space-y-1">
            <CardTitle>{props.name}</CardTitle>
            <CardDescription>{props.email}</CardDescription>
          </div>
          <Badge variant="secondary" className="ml-auto">
            {props.role}
          </Badge>
        </CardHeader>
        <CardContent className="grid gap-2 text-sm">
          <div className="flex justify-between border-t pt-3">
            <span className="text-muted-foreground">Member since</span>
            <span>{new Date(props.joinedUtc).toLocaleDateString()}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">User ID</span>
            <span>#{props.id}</span>
          </div>
        </CardContent>
        <CardFooter>
          <Button variant="outline" onClick={() => setLikes((l) => l + 1)}>
            <ThumbsUp className="h-4 w-4" /> {likes} — works after hydration
          </Button>
        </CardFooter>
      </Card>
    </Layout>
  );
}

function initials(name: string): string {
  return name
    .split(" ")
    .map((p) => p[0])
    .slice(0, 2)
    .join("")
    .toUpperCase();
}
