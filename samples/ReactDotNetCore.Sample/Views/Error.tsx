import { AlertTriangle } from "lucide-react";
import Layout from "./components/Layout";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./components/ui/card";

export interface ErrorProps {
  requestId?: string | null;
}

export default function Error(props: ErrorProps) {
  return (
    <Layout>
      <Card className="max-w-xl border-destructive/40">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-destructive">
            <AlertTriangle className="h-5 w-5" /> An error occurred
          </CardTitle>
          <CardDescription>An error occurred while processing your request.</CardDescription>
        </CardHeader>
        {props.requestId ? (
          <CardContent className="text-sm">
            <span className="text-muted-foreground">Request ID: </span>
            <code className="rounded bg-muted px-1.5 py-0.5 font-mono">{props.requestId}</code>
          </CardContent>
        ) : null}
      </Card>
    </Layout>
  );
}
