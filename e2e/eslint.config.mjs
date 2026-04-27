import eslint from '@eslint/js'
import eslintConfigPrettier from 'eslint-config-prettier'
import playwright from 'eslint-plugin-playwright'
import tseslint from 'typescript-eslint'

export default tseslint.config(
  { ignores: ['node_modules/**', 'playwright-report/**', 'test-results/**'] },
  eslint.configs.recommended,
  ...tseslint.configs.recommended,
  eslintConfigPrettier,
  {
    files: ['tests/**/*.ts', 'playwright.config.ts', 'constants.ts'],
    ...playwright.configs['flat/recommended'],
  }
)
