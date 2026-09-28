#!/usr/bin/env python3
"""
Скрипт автоматического деплоя WebGL билда HelloTap на целевой сервер:
Сервер: 109.69.17.170 (Ubuntu 24.04 LTS, root, port 22)
Каталог размещения: /opt/hellotap/webgl
Nginx: Настройка и проверка доступности
"""

import os
import sys
import subprocess
from pathlib import Path

SERVER_HOST = "109.69.17.170"
SERVER_USER = "root"
SERVER_PORT = 22
REMOTE_PATH = "/opt/hellotap/webgl"

NGINX_CONF = """
server {
    listen 8088;
    server_name _;

    root /opt/hellotap/webgl;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    # WebGL Wasm & Data MIME types
    location ~* \.wasm$ {
        types { application/wasm wasm; }
        add_header Cache-Control "no-cache";
    }

    location ~* \.data$ {
        types { application/octet-stream data; }
        add_header Cache-Control "no-cache";
    }

    location ~* \.symbols\.json$ {
        types { application/octet-stream json; }
    }
}
"""

def main():
    project_dir = Path(__file__).resolve().parent.parent
    build_dir = project_dir / "Builds" / "WebGL"
    index_html = build_dir / "index.html"

    if not index_html.is_file():
        print(f"[-] Ошибка: WebGL билд не найден в {build_dir}. Сначала выполните сборку: python Tools/build_webgl.py")
        sys.exit(1)

    print(f"[*] Локальная папка билда: {build_dir}")
    print(f"[*] Целевой сервер: {SERVER_USER}@{SERVER_HOST}:{REMOTE_PATH}")

    try:
        import paramiko
        import scp
        has_paramiko = True
    except ImportError:
        has_paramiko = False

    if has_paramiko:
        print("[*] Инициализация SSH соединения через Paramiko...")
        ssh = paramiko.SSHClient()
        ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
        try:
            # Connect via SSH key or standard prompt
            ssh.connect(SERVER_HOST, port=SERVER_PORT, username=SERVER_USER, timeout=15)
            print("[+] Подключение к серверу установлено.")

            # Create remote directory
            ssh.exec_command(f"mkdir -p {REMOTE_PATH}")
            
            # Write Nginx config
            stdin, stdout, stderr = ssh.exec_command("cat > /etc/nginx/sites-available/hellotap << 'EOF'\n" + NGINX_CONF + "\nEOF\n")
            ssh.exec_command("ln -sf /etc/nginx/sites-available/hellotap /etc/nginx/sites-enabled/hellotap")
            ssh.exec_command("nginx -t && systemctl reload nginx")

            # Upload files
            with scp.SCPClient(ssh.get_transport()) as scp_client:
                print(f"[*] Загрузка файлов WebGL в {REMOTE_PATH}...")
                scp_client.put(str(build_dir), recursive=True, remote_path="/opt/hellotap")
                # Move to webgl
                ssh.exec_command(f"rm -rf {REMOTE_PATH} && mv /opt/hellotap/WebGL {REMOTE_PATH}")

            print(f"[+] Деплой успешно завершен! HelloTap доступен по адресу: http://{SERVER_HOST}:8088/")
            ssh.close()
        except Exception as ex:
            print(f"[-] Ошибка SSH деплоя: {ex}")
    else:
        print("[*] Paramiko не установлен. Генерация команд rsync / scp:")
        print(f"  scp -r \"{build_dir}\" {SERVER_USER}@{SERVER_HOST}:{REMOTE_PATH}")
        print("  ssh root@109.69.17.170 'systemctl reload nginx'")

if __name__ == "__main__":
    main()
