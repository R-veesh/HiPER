/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        sans: ['"Space Grotesk"', 'ui-sans-serif', 'system-ui'],
        display: ['"Chakra Petch"', 'ui-sans-serif', 'system-ui'],
      },
      colors: {
        night: '#05060a',
        neon: '#5af0ff',
        magenta: '#ff4fd8',
        violet: '#8b5cf6',
        hyper: '#00f6ff',
        aurora: '#12a8ff',
        plasma: '#ff7cf7',
        carbon: '#0c0f18',
      },
      boxShadow: {
        glow: '0 0 25px rgba(90, 240, 255, 0.45), 0 0 60px rgba(255, 79, 216, 0.35)',
        neon: '0 0 10px rgba(0, 246, 255, 0.65), 0 0 25px rgba(255, 124, 247, 0.55)',
      },
      backgroundImage: {
        grid: 'radial-gradient(circle at 1px 1px, rgba(255, 255, 255, 0.08) 1px, transparent 0)',
        hyper: 'linear-gradient(120deg, rgba(0, 255, 255, 0.14), rgba(255, 79, 216, 0.12))',
      },
      animation: {
        float: 'float 6s ease-in-out infinite',
        pulsefast: 'pulsefast 1.2s ease-in-out infinite',
        shimmer: 'shimmer 12s linear infinite',
      },
      keyframes: {
        float: {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-12px)' },
        },
        pulsefast: {
          '0%, 100%': { opacity: 0.35 },
          '50%': { opacity: 1 },
        },
        shimmer: {
          '0%': { backgroundPosition: '0% 50%' },
          '100%': { backgroundPosition: '200% 50%' },
        },
      },
    },
  },
  plugins: [],
}
