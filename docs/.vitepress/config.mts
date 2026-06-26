import { defineConfig } from 'vitepress'
import llmstxt from 'vitepress-plugin-llms'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  vite: {
    plugins: [llmstxt()],
  },

  title: "Tabrixel",
  description: "CLI for working with Google Sheets",
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Guide', link: '/guide/introduction' },
      { text: 'Commands', link: '/commands/' },
      { text: 'Concepts', link: '/concepts/output-formats' }
    ],

    sidebar: [
      {
        text: 'Guide',
        items: [
          { text: 'Introduction', link: '/guide/introduction' },
          { text: 'Installation', link: '/guide/installation' },
          { text: 'Getting Started', link: '/guide/getting-started' },
          { text: 'Configuration', link: '/guide/configuration' }
        ]
      },
      {
        text: 'Commands',
        items: [
          { text: 'Overview', link: '/commands/' },
          { text: 'auth check', link: '/commands/auth-check' },
          { text: 'config', link: '/commands/config' },
          { text: 'describe', link: '/commands/describe' },
          { text: 'columns', link: '/commands/columns' },
          { text: 'rows', link: '/commands/rows' }
        ]
      },
      {
        text: 'Concepts',
        items: [
          { text: 'Output formats', link: '/concepts/output-formats' },
          { text: 'Matching & safety', link: '/concepts/matching-and-safety' },
          { text: 'Exit codes & errors', link: '/concepts/exit-codes-and-errors' }
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/tabrixel/cli' }
    ]
  },
  transformHtml(code) {
    return code.replace(
      '</body>',
      `<script async src="https://scripts.simpleanalyticscdn.com/latest.js"></script>` +
      `<noscript><img src="https://queue.simpleanalyticscdn.com/noscript.gif" alt="" referrerpolicy="no-referrer-when-downgrade"/></noscript>` +
      `</body>`
    )
  }
})
