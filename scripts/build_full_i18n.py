# -*- coding: utf-8 -*-
import json

langs = ['ru', 'en', 'fr', 'de', 'ar', 'zh', 'es', 'tr']

# Read ru to get key ordering and types
with open('scripts/locales/ru.json', 'r', encoding='utf-8') as f:
    ru_dict = json.load(f)

lines = []
lines.append("export type Language = 'ru' | 'en' | 'fr' | 'de' | 'ar' | 'zh' | 'es' | 'tr';\n")
lines.append("export interface TranslationDictionary {")
for k in ru_dict.keys():
    lines.append(f"  {k}: string;")
lines.append("}\n")

lines.append("export const TRANSLATIONS: Record<Language, TranslationDictionary> = {")
for idx, l in enumerate(langs):
    with open(f'scripts/locales/{l}.json', 'r', encoding='utf-8') as f:
        data = json.load(f)
    lines.append(f"  {l}: {{")
    entry_lines = []
    for k, v in data.items():
        v_escaped = v.replace('\\', '\\\\').replace('"', '\\"').replace('\n', '\\n')
        entry_lines.append(f'    {k}: "{v_escaped}"')
    lines.append(",\n".join(entry_lines))
    comma = "," if idx < len(langs) - 1 else ""
    lines.append(f"  }}{comma}")
lines.append("};\n")

lines.append("""export function detectInitialLanguage(): Language {
  try {
    const saved = localStorage.getItem("HELLOTAP_LANG") as Language;
    if (['ru', 'en', 'fr', 'de', 'ar', 'zh', 'es', 'tr'].includes(saved)) return saved;

    const navLang = navigator.language?.toLowerCase() || '';
    if (navLang.startsWith('ru') || navLang.startsWith('be') || navLang.startsWith('uk') || navLang.startsWith('kz')) {
      return 'ru';
    }
    if (navLang.startsWith('fr')) return 'fr';
    if (navLang.startsWith('de')) return 'de';
    if (navLang.startsWith('ar')) return 'ar';
    if (navLang.startsWith('zh')) return 'zh';
    if (navLang.startsWith('es')) return 'es';
    if (navLang.startsWith('tr')) return 'tr';
    if (navLang.startsWith('en')) return 'en';
  } catch {
    // fallback
  }
  return 'ru';
}
""")

content = "\n".join(lines)
with open('src/utils/i18n.ts', 'w', encoding='utf-8') as f:
    f.write(content)

print(f"Successfully generated src/utils/i18n.ts with {len(langs)} languages!")
