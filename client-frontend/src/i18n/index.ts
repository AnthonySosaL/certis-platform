import i18n from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import { initReactI18next } from 'react-i18next'

import en from '@/i18n/locales/en/common.json'
import es from '@/i18n/locales/es/common.json'

// English is the only language exposed in the UI for now (platform is
// English-first by design). The Spanish resources stay wired in so a
// future language switcher only has to add UI, not a translation pass.
void i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: { common: en },
      es: { common: es },
    },
    lng: 'en',
    fallbackLng: 'en',
    ns: ['common'],
    defaultNS: 'common',
    interpolation: { escapeValue: false },
  })

export default i18n
