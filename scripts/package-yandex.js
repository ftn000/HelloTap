import fs from 'fs';
import path from 'path';
import { execSync } from 'child_process';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const rootDir = path.resolve(__dirname, '..');
const distDir = path.join(rootDir, 'dist');
const zipOutput = path.join(rootDir, 'hellotap-yandex.zip');

console.log('📦 [Yandex Packager] Начинаем упаковку бандла для Яндекс Игр...');

// 1. Проверяем наличие папки dist
if (!fs.existsSync(distDir) || !fs.existsSync(path.join(distDir, 'index.html'))) {
  console.error('❌ Ошибка: папка dist/ или dist/index.html не найдена! Сначала выполните npm run build.');
  process.exit(1);
}

// 2. Удаляем старый zip если есть
if (fs.existsSync(zipOutput)) {
  fs.unlinkSync(zipOutput);
  console.log('🧹 Старый архив удален.');
}

// 3. Создаем zip-архив с содержимым папки dist прямо в корне архива
try {
  if (process.platform === 'win32') {
    // В PowerShell архивируем содержимое dist/* в hellotap-yandex.zip
    const psCmd = `powershell -Command "Compress-Archive -Path '${distDir}\\*' -DestinationPath '${zipOutput}' -Force"`;
    execSync(psCmd, { stdio: 'inherit' });
  } else {
    execSync(`cd "${distDir}" && zip -r "${zipOutput}" ./*`, { stdio: 'inherit' });
  }

  const stats = fs.statSync(zipOutput);
  const sizeKb = (stats.size / 1024).toFixed(1);
  console.log(`✅ [Yandex Packager] Успешно создан архив: ${zipOutput} (${sizeKb} KB)`);
  console.log('🚀 Этот архив готов к загрузке в Консоль Разработчика Яндекс Игр!');
} catch (err) {
  console.error('❌ Ошибка при создании архива:', err);
  process.exit(1);
}
