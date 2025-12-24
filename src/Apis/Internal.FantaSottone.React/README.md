# FantaSottone Frontend

React + TypeScript + Vite frontend application for FantaSottone game.

## Features

- **React 18+** with TypeScript
- **Vite** for fast development and building
- **shadcn/ui** as the ONLY UI component library
- **Tailwind CSS** for styling
- **Dark/Light theme** with toggle and persistence
- **Pluggable authentication strategies** (Mock / JWT)
- **Mock server** for development without backend
- **Zero code duplication** with reusable components, hooks, and utilities

## Setup

### Prerequisites

- Node.js 18+
- npm or yarn

### Installation

```bash
cd Frontend
npm install
```

### Environment Configuration

The app uses environment variables for configuration. Create a `.env.local` file for local overrides:

```bash
# API Configuration
VITE_API_BASE_URL=http://localhost:5001

# Use mock server (true) or real API (false)
VITE_USE_MOCKS=true

# Authentication strategy: 'mock' or 'jwt'
VITE_AUTH_STRATEGY=mock

# Polling interval in milliseconds
VITE_POLLING_INTERVAL_MS=3000

# App name
VITE_APP_NAME=FantaSottone
```

## Development

### Start Development Server

```bash
npm run dev
```

The app will be available at `http://localhost:5173`

### Using Mock Server

By default, the app uses the in-memory mock server:

```bash
VITE_USE_MOCKS=true
VITE_AUTH_STRATEGY=mock
```

**Test credentials:**

- Username: `test1`, Code: `code1` (Creator)
- Username: `test2`, Code: `code2` (Player)
- Username: `test3`, Code: `code3` (Player)

### Switching to Real Backend

To connect to a real backend API:

1. Update `.env.local`:

```bash
VITE_USE_MOCKS=false
VITE_AUTH_STRATEGY=jwt
VITE_API_BASE_URL=http://localhost:5001
```

2. Ensure your backend API is running on the specified URL

The frontend will automatically use `HttpClient` instead of `MockTransport`.

## Building for Production

```bash
npm run build
```

The built files will be in the `dist` directory.

### Preview Production Build

```bash
npm run preview
```

      tseslint.configs.stylisticTypeChecked,

      // Other configs...
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },

},
])

````

You can also install [eslint-plugin-react-x](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-x) and [eslint-plugin-react-dom](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-dom) for React-specific lint rules:

```js
// eslint.config.js
import reactX from 'eslint-plugin-react-x'
import reactDom from 'eslint-plugin-react-dom'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...
      // Enable lint rules for React
      reactX.configs['recommended-typescript'],
      // Enable lint rules for React DOM
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])
````
