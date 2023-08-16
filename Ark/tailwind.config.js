/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./**/*.{razor,html}'],
    theme: {
        extend: {
            colors: {
                'white_light': '#DAE4EB',
                'white_normal': '#71757D',
                'gray_darkest': '#16191C',
                'gray_dark': '#1F2327',
                'gray': '#292E33',
                'red': '#E74545',
                'orange': '#E59432',
                'green': '#2AE441',
                'primary': '#235DDA'
            },
            fontFamily: {
                ark: ['arkicons', 'sans'],
                sans: ['gontserrat', 'sans']
            }
        }
    },
    future: {
        hoverOnlyWhenSupported: true,
    },
    plugins: [
        // add this line
        require('@tailwindcss/forms'),
    ],
}

