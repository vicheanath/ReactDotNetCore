import { useState } from "react";
import { Activity, ArrowDownRight, ArrowUpRight, CreditCard, DollarSign, Users, type LucideIcon } from "lucide-react";
import Layout from "./components/Layout";
import { cn } from "./lib/utils";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./components/ui/card";
import { Button } from "./components/ui/button";
import { Badge } from "./components/ui/badge";
import { Avatar, AvatarFallback } from "./components/ui/avatar";

export interface DashboardStat {
  key: string;
  label: string;
  value: string;
  delta: string;
  trend: "up" | "down";
}
export interface RecentSale {
  name: string;
  email: string;
  amount: string;
}
export interface DashboardProps {
  stats: DashboardStat[];
  revenue: number[];
  months: string[];
  recentSales: RecentSale[];
}

const ICONS: Record<string, LucideIcon> = {
  revenue: DollarSign,
  subscriptions: Users,
  sales: CreditCard,
  active: Activity,
};

const RANGES = ["7d", "30d", "90d"] as const;

export default function Dashboard(props: DashboardProps) {
  const [range, setRange] = useState<(typeof RANGES)[number]>("30d");
  const max = Math.max(...props.revenue, 1);

  return (
    <Layout active="dashboard">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Dashboard</h1>
          <p className="text-sm text-muted-foreground">An overview of your store's performance.</p>
        </div>
        <div className="inline-flex items-center gap-1 rounded-lg border bg-card p-1">
          {RANGES.map((r) => (
            <Button key={r} size="sm" variant={range === r ? "default" : "ghost"} onClick={() => setRange(r)}>
              {r}
            </Button>
          ))}
        </div>
      </div>

      <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {props.stats.map((s) => {
          const Icon = ICONS[s.key] ?? Activity;
          const up = s.trend === "up";
          return (
            <Card key={s.key}>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">{s.label}</CardTitle>
                <Icon className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{s.value}</div>
                <p className={cn("mt-1 flex items-center gap-1 text-xs", up ? "text-emerald-600" : "text-red-600")}>
                  {up ? <ArrowUpRight className="h-3 w-3" /> : <ArrowDownRight className="h-3 w-3" />}
                  {s.delta} from last {range}
                </p>
              </CardContent>
            </Card>
          );
        })}
      </div>

      <div className="mt-4 grid gap-4 lg:grid-cols-7">
        <Card className="lg:col-span-4">
          <CardHeader>
            <CardTitle>Revenue</CardTitle>
            <CardDescription>Monthly revenue (last 12 months)</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex h-[240px] items-end gap-2">
              {props.revenue.map((v, i) => (
                <div key={i} className="flex flex-1 flex-col items-center gap-2">
                  <div
                    className="w-full rounded-t-md bg-primary/80 transition-all hover:bg-primary"
                    style={{ height: `${Math.round((v / max) * 200)}px` }}
                    title={`${props.months[i]}: $${v.toLocaleString()}`}
                  />
                  <span className="text-[10px] text-muted-foreground">{props.months[i]}</span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card className="lg:col-span-3">
          <CardHeader className="flex flex-row items-center justify-between space-y-0">
            <div className="space-y-1.5">
              <CardTitle>Recent Sales</CardTitle>
              <CardDescription>{props.recentSales.length} sales this month</CardDescription>
            </div>
            <Badge variant="success">+12.5%</Badge>
          </CardHeader>
          <CardContent className="space-y-5">
            {props.recentSales.map((sale, i) => (
              <div key={i} className="flex items-center gap-4">
                <Avatar>
                  <AvatarFallback>{initials(sale.name)}</AvatarFallback>
                </Avatar>
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium leading-none">{sale.name}</p>
                  <p className="mt-1 truncate text-xs text-muted-foreground">{sale.email}</p>
                </div>
                <div className="ml-auto text-sm font-medium">{sale.amount}</div>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
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
