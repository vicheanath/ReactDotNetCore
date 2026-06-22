// @vitest-environment jsdom
import { describe, it, expect, beforeEach, vi } from "vitest";
import { createElement } from "react";
import { renderToString } from "react-dom/server";
import { createRegistry } from "./registry";
import { mount, readProps } from "./client";

const Hello = (props: { name: string }) => createElement("h1", null, `Hi ${props.name}`);

beforeEach(() => {
  document.body.innerHTML = "";
});

describe("readProps", () => {
  it("parses the embedded JSON props", () => {
    document.body.innerHTML = `<script id="react-dotnetcore-props" type="application/json">{"a":1,"b":"x"}</script>`;
    expect(readProps()).toEqual({ a: 1, b: "x" });
  });

  it("returns {} when the props element is absent", () => {
    expect(readProps()).toEqual({});
  });

  it("returns {} on invalid JSON", () => {
    const spy = vi.spyOn(console, "error").mockImplementation(() => {});
    document.body.innerHTML = `<script id="react-dotnetcore-props">{not json}</script>`;
    expect(readProps()).toEqual({});
    spy.mockRestore();
  });
});

describe("mount", () => {
  it("hydrates the root using the embedded component name + props", async () => {
    const props = { name: "Ada" };
    const ssr = renderToString(createElement(Hello, props));
    document.body.innerHTML =
      `<div id="react-dotnetcore-root" data-react-dotnetcore-component="Hello">${ssr}</div>` +
      `<script id="react-dotnetcore-props" type="application/json">${JSON.stringify(props)}</script>`;

    mount(createRegistry({ "./Views/Hello.tsx": { default: Hello } }));
    await new Promise((r) => setTimeout(r, 10)); // let hydration flush

    expect(document.getElementById("react-dotnetcore-root")!.innerHTML).toContain("Hi Ada");
  });

  it("no-ops when the root element is absent", () => {
    expect(() => mount(createRegistry({ "./Views/Hello.tsx": { default: Hello } }))).not.toThrow();
  });

  it("logs an error when the component is not registered on the client", () => {
    const spy = vi.spyOn(console, "error").mockImplementation(() => {});
    document.body.innerHTML = `<div id="react-dotnetcore-root" data-react-dotnetcore-component="Missing"></div>`;
    mount(createRegistry({}));
    expect(spy).toHaveBeenCalledWith(expect.stringContaining("Missing"));
    spy.mockRestore();
  });
});
