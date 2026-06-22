import Layout from "./components/Layout";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./components/ui/card";

export default function Privacy() {
  return (
    <Layout active="privacy">
      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle>Privacy Policy</CardTitle>
          <CardDescription>How this sample handles your data.</CardDescription>
        </CardHeader>
        <CardContent className="text-sm leading-relaxed text-muted-foreground">
          This is a sample privacy page rendered entirely by React on the server. Use this page to keep
          per-page policy content while the surrounding layout stays shared.
        </CardContent>
      </Card>
    </Layout>
  );
}
