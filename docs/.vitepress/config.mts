import { defineConfig } from "vitepress";
import { withMermaid } from "vitepress-plugin-mermaid";

const repo = "https://github.com/vicheanath/ReactDotNetCore";

export default withMermaid(defineConfig({
  title: "ReactDotNetCore",
  description: "Use React components as ASP.NET Core MVC views — SSR + hydration, no API or SPA.",
  lang: "en-US",

  // GitHub Pages project site: https://vicheanath.github.io/ReactDotNetCore/
  base: "/ReactDotNetCore/",
  cleanUrls: true,
  lastUpdated: true,

  // README.md is kept for browsing the docs/ folder on GitHub; the site home is index.md.
  srcExclude: ["README.md"],

  head: [["link", { rel: "icon", href: "/ReactDotNetCore/favicon.svg" }]],

  themeConfig: {
    nav: [
      { text: "Guide", link: "/getting-started" },
      { text: "API", link: "/api-reference" },
      {
        text: "Packages",
        items: [
          { text: "@react-dotnetcore/runtime (npm)", link: "https://www.npmjs.com/package/@react-dotnetcore/runtime" },
          { text: "ReactDotNetCore.AspNetCore (NuGet)", link: "https://www.nuget.org/packages/ReactDotNetCore.AspNetCore" },
        ],
      },
    ],

    sidebar: [
      {
        text: "Introduction",
        items: [
          { text: "Overview", link: "/" },
          { text: "Getting Started", link: "/getting-started" },
        ],
      },
      {
        text: "Guides",
        items: [
          { text: "Writing Views", link: "/writing-views" },
          { text: "Configuration", link: "/configuration" },
          { text: "Development Mode (HMR)", link: "/development-mode" },
          { text: "Layouts & Components", link: "/layouts-and-components" },
          { text: "Migrating from Razor", link: "/migration-from-razor" },
        ],
      },
      {
        text: "Reference",
        items: [
          { text: "Architecture", link: "/architecture" },
          { text: "API Reference", link: "/api-reference" },
          { text: "Deployment", link: "/deployment" },
          { text: "Troubleshooting", link: "/troubleshooting" },
        ],
      },
    ],

    socialLinks: [{ icon: "github", link: repo }],
    editLink: { pattern: `${repo}/edit/main/docs/:path`, text: "Edit this page on GitHub" },
    search: { provider: "local" },
    footer: {
      message: "Released under the MIT License.",
      copyright: `Copyright © ${new Date().getFullYear()} ReactDotNetCore Contributors`,
    },
  },

  // Mermaid theme follows the site's light/dark mode.
  mermaid: {},
}));
