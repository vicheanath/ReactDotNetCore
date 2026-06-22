import { describe, it, expect } from "vitest";
import type { ComponentType } from "react";
import { createRegistry, registeredNames } from "./registry";

const Comp: ComponentType<any> = () => null;

describe("createRegistry", () => {
  it("keys components by file base name without extension", () => {
    const reg = createRegistry({
      "./Views/UserProfile.tsx": { default: Comp },
      "../Views/Dashboard.tsx": { default: Comp },
    });
    expect(Object.keys(reg).sort()).toEqual(["Dashboard", "UserProfile"]);
  });

  it("handles .tsx/.ts/.jsx/.js extensions", () => {
    const reg = createRegistry({
      "./Views/A.tsx": { default: Comp },
      "./Views/B.ts": { default: Comp },
      "./Views/C.jsx": { default: Comp },
      "./Views/D.js": { default: Comp },
    });
    expect(Object.keys(reg).sort()).toEqual(["A", "B", "C", "D"]);
  });

  it("skips modules without a default export", () => {
    const reg = createRegistry({
      "./Views/NoDefault.tsx": {},
      "./Views/AlsoNo.tsx": undefined,
      "./Views/Ok.tsx": { default: Comp },
    });
    expect(Object.keys(reg)).toEqual(["Ok"]);
  });

  it("registeredNames returns the registry keys", () => {
    const reg = createRegistry({ "./Views/A.tsx": { default: Comp }, "./Views/B.tsx": { default: Comp } });
    expect(registeredNames(reg).sort()).toEqual(["A", "B"]);
  });
});
