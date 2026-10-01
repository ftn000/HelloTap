import os
import sys
import paramiko

# Ensure UTF-8 output on Windows console
if sys.platform == "win32":
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")

HOST = os.environ.get("VPS_HOST", "109.69.17.170")
USER = os.environ.get("VPS_USER", "root")
PORT = int(os.environ.get("VPS_PORT", "22"))
REMOTE_DIR = os.environ.get("VPS_REMOTE_DIR", "/opt/hellotap/webgl")
PROJECT_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
LOCAL_DIR = os.path.join(PROJECT_ROOT, "dist")

# Load password from .env if present, otherwise environment variable
PASS = os.environ.get("VPS_PASS")
env_path = os.path.join(PROJECT_ROOT, ".env")
if not PASS and os.path.exists(env_path):
    with open(env_path, "r", encoding="utf-8") as f:
        for line in f:
            if line.strip().startswith("VPS_PASS="):
                PASS = line.strip().split("=", 1)[1].strip().strip('"').strip("'")
            elif line.strip().startswith("VPS_HOST="):
                HOST = line.strip().split("=", 1)[1].strip().strip('"').strip("'")

def deploy():
    print(f"Connecting to {HOST}:{PORT} as {USER}...")
    ssh = paramiko.SSHClient()
    ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    ssh.connect(HOST, port=PORT, username=USER, password=PASS, timeout=15)
    print("SSH Connected successfully!")

    sftp = ssh.open_sftp()
    print("SFTP session opened.")

    stdin, stdout, stderr = ssh.exec_command(f"mkdir -p {REMOTE_DIR} && rm -rf {REMOTE_DIR}/*")
    stdout.channel.recv_exit_status()

    print(f"Syncing React build files from {LOCAL_DIR} to {REMOTE_DIR}...")
    count = 0
    for root, dirs, files in os.walk(LOCAL_DIR):
        rel_dir = os.path.relpath(root, LOCAL_DIR).replace("\\", "/")
        remote_curr_dir = REMOTE_DIR if rel_dir == "." else f"{REMOTE_DIR}/{rel_dir}"

        stdin, stdout, stderr = ssh.exec_command(f"mkdir -p '{remote_curr_dir}'")
        stdout.channel.recv_exit_status()

        for file in files:
            local_file = os.path.join(root, file)
            remote_file = f"{remote_curr_dir}/{file}"
            size_mb = os.path.getsize(local_file) / (1024 * 1024)
            print(f"Uploading {rel_dir}/{file} ({size_mb:.2f} MB)...")
            sftp.put(local_file, remote_file)
            count += 1

    sftp.close()
    print(f"Sync complete! Total uploaded files: {count}")

    print("Reloading Nginx...")
    stdin, stdout, stderr = ssh.exec_command("nginx -t && systemctl reload nginx")
    out = stdout.read().decode('utf-8', errors='replace')
    err = stderr.read().decode('utf-8', errors='replace')
    status = stdout.channel.recv_exit_status()
    print(f"Nginx reload status: {status}")
    if err:
        print(f"Nginx output: {err.strip()}")

    print("Verifying deployed site...")
    stdin, stdout, stderr = ssh.exec_command("curl -Is https://109.69.17.170.sslip.io/hellotap/ | head -n 10")
    site_out = stdout.read().decode('utf-8', errors='replace')
    print("Site response:")
    print(site_out)

    ssh.close()
    print("HelloTap React deployment finished successfully!")

if __name__ == "__main__":
    deploy()
