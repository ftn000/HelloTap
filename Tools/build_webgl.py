#!/usr/bin/env python3
"""
Скрипт автоматизированной сборки WebGL для HelloTap (GameDev Clicker).
Запускает Unity в headless/batchmode режиме, собирает проект через HelloTap.Editor.WebGLBuilder.BuildWebGL
и выводит подробный отчет о размере и статусе артефактов.
"""

import os
import sys
import subprocess
import time
from pathlib import Path

UNITY_PATHS = [
    r"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe",
    r"C:\Program Files\Unity 6000.3.11f1\Editor\Unity.exe",
    r"C:\Program Files\Unity\Hub\Editor\6000.0.31f1\Editor\Unity.exe",
]

def find_unity_exe():
    for p in UNITY_PATHS:
        if os.path.isfile(p):
            return p
    # Search in Unity Hub default directory
    hub_dir = r"C:\Program Files\Unity\Hub\Editor"
    if os.path.isdir(hub_dir):
        for entry in os.listdir(hub_dir):
            candidate = os.path.join(hub_dir, entry, "Editor", "Unity.exe")
            if os.path.isfile(candidate):
                return candidate
    return None

def main():
    project_dir = Path(__file__).resolve().parent.parent
    log_file = project_dir / "Builds" / "webgl_build.log"
    log_file.parent.mkdir(parents=True, exist_ok=True)

    unity_exe = find_unity_exe()
    if not unity_exe:
        print("[-] Ошибка: Unity.exe не найден в стандартных путях!")
        sys.exit(1)

    print(f"[*] Используется Unity: {unity_exe}")
    print(f"[*] Проект: {project_dir}")
    print(f"[*] Лог сборки: {log_file}")

    cmd = [
        unity_exe,
        "-batchmode",
        "-nographics",
        "-quit",
        "-projectPath", str(project_dir),
        "-executeMethod", "HelloTap.Editor.WebGLBuilder.BuildWebGL",
        "-logFile", str(log_file)
    ]

    print("[*] Запуск компиляции WebGL билда...")
    start_time = time.time()
    proc = subprocess.Popen(cmd)
    
    while proc.poll() is None:
        time.sleep(2)
        print(".", end="", flush=True)

    elapsed = time.time() - start_time
    print(f"\n[*] Процесс Unity завершился за {elapsed:.1f} сек. Код возврата: {proc.returncode}")

    build_dir = project_dir / "Builds" / "WebGL"
    index_html = build_dir / "index.html"

    if proc.returncode == 0 and index_html.is_file():
        total_size_bytes = sum(f.stat().st_size for f in build_dir.rglob('*') if f.is_file())
        print(f"[+] WebGL билд успешно создан в {build_dir}")
        print(f"[+] Общий размер билда: {total_size_bytes / (1024*1024):.2f} MB")
        sys.exit(0)
    else:
        print("[-] Ошибка: WebGL билд не удался. Проверьте лог сборки:")
        if log_file.is_file():
            with open(log_file, "r", encoding="utf-8", errors="ignore") as f:
                lines = f.readlines()
                print("".join(lines[-40:]))
        sys.exit(1)

if __name__ == "__main__":
    main()
