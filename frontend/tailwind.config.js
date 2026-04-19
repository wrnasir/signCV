/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{js,jsx,ts,tsx}",],
  theme: {
    extend: {
      colors: {
        brand: {
          900: '#0a0a0f',
          800: '#12121a',
          700: '#1a1a26',
          600: '#2a2a3a',
          500: '#6c5ce7',
          400: '#8b7cf0',
        },
        muted: '#8888a0',
      },
      fontFamily: {
        display: ['"Space Mono"', 'monospace'],
        body: ['"Sora"', 'sans-serif'],
      },
    },
  },
  plugins: [],
}

