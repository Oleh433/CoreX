/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.cshtml',
    './Pages/**/*.cshtml.cs',
    './wwwroot/js/**/*.js',
  ],
  theme: {
    extend: {
      colors: {
        brand: { '50': '#fff3ee', '300': '#ff8a00', '500': '#ff4d2e', '700': '#c43a18' },
        ink:   { '50': '#fafafa', '200': '#e6e6e6', '500': '#666666', '800': '#222222', '900': '#0d0f12' },
        success: '#1d6f3d',
        danger:  '#c43a18',
        warning: '#a86b00',
        info:    '#1c3f8a',
        owner:   '#6c2497',
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      backgroundImage: {
        'flame': 'linear-gradient(135deg, #ff4d2e 0%, #ff8a00 100%)',
      },
      borderRadius: {
        'pill': '9999px',
        'card': '14px',
      },
      boxShadow: {
        'flame': '0 8px 24px -10px rgba(255,77,46,0.45)',
      },
    },
  },
  plugins: [require('@tailwindcss/forms')],
};
