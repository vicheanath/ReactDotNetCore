import { describe, it, expect } from "vitest";
import { createElement } from "react";
import { createRegistry } from "./registry";
import { createServerRenderer } from "./server";

const Hello = (props: { name: string }) => createElement("h1", null, `Hi ${props.name}`);

describe("createServerRenderer", () => {
  const registry = createRegistry({ "./Views/Hello.tsx": { default: Hello } });
  const render = createServerRenderer(registry);

  it("renders a registered component to an HTML string", () => {
    expect(render("Hello", { name: "Ada" })).toContain("<h1>Hi Ada</h1>");
  });

  it("passes props through to the component", () => {
    expect(render("Hello", { name: "Grace" })).toContain("Hi Grace");
  });

  it("throws with the known component list when the name is missing", () => {
    expect(() => render("Nope", {})).toThrowError(/not registered/);
    expect(() => render("Nope", {})).toThrowError(/Hello/);
  });
});
