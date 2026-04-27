module.exports = {
  ci: {
    collect: {
      startServerCommand: "NUXT_HOST=127.0.0.1 NUXT_PORT=3000 make frontend-preview-only",
      startServerReadyPattern: "Listening",
      url: ["http://127.0.0.1:3000/"],
      numberOfRuns: 3,
    },
    assert: {
      preset: "lighthouse:recommended",
    },
    upload: {
      target: "temporary-public-storage",
    },
  },
};
