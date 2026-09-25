import os

sprites_dir = r"C:\HelloTap\Assets\Sprites"

sprite_configs = {
    "spr_desk_mat.png": {"border": "{x: 0, y: 0, z: 0, w: 0}"},
    "spr_monitor_frame.png": {"border": "{x: 0, y: 0, z: 0, w: 0}"},
    "spr_monitor_screen.png": {"border": "{x: 0, y: 0, z: 0, w: 0}"},
    "spr_keyboard.png": {"border": "{x: 0, y: 0, z: 0, w: 0}"},
    "spr_mouse.png": {"border": "{x: 0, y: 0, z: 0, w: 0}"},
    "spr_coffee_mug.png": {"border": "{x: 0, y: 0, z: 0, w: 0}"},
    "spr_energy_can.png": {"border": "{x: 0, y: 0, z: 0, w: 0}"},
    "spr_cat.png": {"border": "{x: 0, y: 0, z: 0, w: 0}"},
    "spr_card_bg.png": {"border": "{x: 16, y: 16, z: 16, w: 16}"},
    "spr_btn_cyan.png": {"border": "{x: 14, y: 14, z: 14, w: 14}"},
    "spr_btn_orange.png": {"border": "{x: 14, y: 14, z: 14, w: 14}"},
    "spr_btn_gold.png": {"border": "{x: 14, y: 14, z: 14, w: 14}"}
}

for fname, cfg in sprite_configs.items():
    meta_path = os.path.join(sprites_dir, fname + ".meta")
    name_no_ext = os.path.splitext(fname)[0]
    if os.path.exists(meta_path):
        with open(meta_path, "r", encoding="utf-8") as f:
            content = f.read()

        # Ensure fileIDToRecycleName is present
        target_recycle = f"fileIDToRecycleName:\n    21300000: {name_no_ext}"
        if "fileIDToRecycleName:" not in content:
            content = content.replace("TextureImporter:\n", f"TextureImporter:\n  {target_recycle}\n")
        elif "internalIDToNameTable: []" in content:
            content = content.replace("internalIDToNameTable: []", f"fileIDToRecycleName:\n    21300000: {name_no_ext}\n  internalIDToNameTable: []")

        # Set border
        border_val = cfg["border"]
        import re
        content = re.sub(r"spriteBorder: \{[^}]*\}", f"spriteBorder: {border_val}", content)

        with open(meta_path, "w", encoding="utf-8") as f:
            f.write(content)
        print(f"Updated {meta_path}")

print("All sprite metas fixed successfully!")
