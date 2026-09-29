/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        cyber: {
          bg: "#0B0F19",
          card: "#121826",
          cardHover: "#1A2234",
          border: "#1E293B",
          neonGreen: "#10B981",
          neonBlue: "#06B6D4",
          neonGold: "#F59E0B",
          neonPurple: "#8B5CF6",
          neonPink: "#EC4899"
        }
      },
      fontFamily: {
        mono: ['"Fira Code"', 'JetBrains Mono', 'Consolas', 'monospace']
      },
      animation: {
        'pulse-glow': 'pulseGlow 2s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'float-up': 'floatUp 0.8s ease-out forwards',
      },
      keyframes: {
        pulseGlow: {
          '0%, 100%': { opacity: '1', filter: 'drop-shadow(0 0 15px rgba(6, 182, 212, 0.6))' },
          '50%': { opacity: '.7', filter: 'drop-shadow(0 0 5px rgba(6, 182, 212, 0.2))' },
        },
        floatUp: {
          '0%': { opacity: '1', transform: 'translateY(0) scale(1)' },
          '100%': { opacity: '0', transform: 'translateY(-40px) scale(1.15)' }
        }
      }
    },
  },
  plugins: [],
}
