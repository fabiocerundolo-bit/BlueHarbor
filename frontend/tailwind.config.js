/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      colors: {
        navy: {
          DEFAULT: '#000033',
          50:  '#E8E8F0',
          100: '#C0C0D4',
          200: '#8080A8',
          300: '#40407C',
          400: '#1A1A5C',
          500: '#000033',
          600: '#000029',
          700: '#00001F',
          800: '#000014',
        },
        harbor: {
          accent:  '#0066CC',
          light:   '#E8F0FE',
          border:  '#C5D5EA',
          success: '#16A34A',
          warn:    '#D97706',
          danger:  '#DC2626',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      boxShadow: {
        card: '0 1px 3px 0 rgba(0,0,51,0.08), 0 1px 2px -1px rgba(0,0,51,0.06)',
        'card-md': '0 4px 6px -1px rgba(0,0,51,0.10), 0 2px 4px -2px rgba(0,0,51,0.08)',
      },
    },
  },
  plugins: [],
}
