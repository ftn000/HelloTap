import os
import math
from PIL import Image, ImageDraw, ImageFont

sprites_dir = r"C:\HelloTap\Assets\Sprites"
os.makedirs(sprites_dir, exist_ok=True)

def create_rounded_rect(draw, bbox, radius, fill, outline=None, width=1):
    x0, y0, x1, y1 = bbox
    draw.rounded_rectangle([x0, y0, x1, y1], radius=radius, fill=fill, outline=outline, width=width)

# 1. Desk Background & RGB Mat (1200 x 700)
def make_desk():
    im = Image.new("RGBA", (1200, 700), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Dark desk wood / carbon surface
    create_rounded_rect(d, (20, 20, 1180, 680), 24, fill=(24, 26, 32, 255), outline=(42, 46, 58, 255), width=3)
    # Desk mat (RGB Mousepad)
    create_rounded_rect(d, (80, 120, 1120, 640), 16, fill=(16, 17, 22, 255), outline=(0, 229, 255, 180), width=2)
    # RGB Underglow accent (magenta / cyan gradient line)
    d.line([(85, 638), (1115, 638)], fill=(255, 0, 128, 200), width=3)
    im.save(os.path.join(sprites_dir, "spr_desk_mat.png"))
    print("Created spr_desk_mat.png")

# 2. Sleek Modern Gamedev Monitor Frame & Stand (840 x 420)
def make_monitor_frame():
    im = Image.new("RGBA", (840, 420), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Monitor Stand Base (centered at X=420)
    create_rounded_rect(d, (330, 396, 510, 416), 6, fill=(30, 34, 44, 255), outline=(52, 58, 74, 255), width=2)
    # Stand Column / Arm (centered at X=420)
    d.rectangle([404, 348, 436, 400], fill=(24, 27, 36, 255))
    
    # Single Sleek Ultrawide Frame (Outer Bezel: 30, 18, 810, 348 - width 780, height 330)
    create_rounded_rect(d, (30, 18, 810, 348), 12, fill=(20, 23, 31, 255), outline=(48, 56, 76, 255), width=3)
    # Inner Bezel Screen Cutout (42, 30, 798, 336 - exact width 756, height 306, centered at X=420, Y=183)
    create_rounded_rect(d, (42, 30, 798, 336), 4, fill=(10, 12, 17, 255))
    # Bottom chin power LED (subtle cyan glow centered at X=420)
    d.ellipse([417, 341, 423, 347], fill=(0, 229, 255, 230))
    
    im.save(os.path.join(sprites_dir, "spr_monitor_frame.png"))
    print("Created spr_monitor_frame.png")

# 3. Clean Gamedev Monitor Screen (756 x 306 - exact fit for monitor cutout)
def make_monitor_screen():
    im = Image.new("RGBA", (756, 306), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    
    # ---------------- Left 65%: IDE Code Editor (0, 0, 490, 306) ----------------
    d.rectangle([0, 0, 490, 306], fill=(10, 13, 19, 255))
    # Top Tab Bar
    d.rectangle([0, 0, 490, 28], fill=(18, 22, 30, 255))
    # Window controls (macOS / Linux style mini dots)
    d.ellipse([12, 10, 18, 16], fill=(255, 95, 86, 255))
    d.ellipse([24, 10, 30, 16], fill=(255, 189, 46, 255))
    d.ellipse([36, 10, 42, 16], fill=(39, 201, 63, 255))
    
    # Active Editor Tab (GameManager.cs)
    create_rounded_rect(d, (52, 4, 180, 28), 4, fill=(10, 13, 19, 255), outline=(32, 38, 52, 255), width=1)
    d.ellipse([62, 12, 68, 18], fill=(0, 180, 216, 255)) # C# icon dot
    
    # Line Numbers Gutter
    d.rectangle([0, 28, 36, 306], fill=(13, 16, 23, 255))
    d.line([(36, 28), (36, 306)], fill=(25, 30, 42, 255), width=1)
    # Subtle line number markers
    for idx, ly in enumerate(range(38, 290, 22)):
        d.rectangle([18, ly+6, 26, ly+8], fill=(42, 50, 68, 200))
        
    # Divider between IDE and Right Engine Viewport
    d.line([(490, 0), (490, 306)], fill=(32, 38, 52, 255), width=2)
            
    # ---------------- Right 35%: Game Engine Viewport (491, 0, 756, 306) ----------------
    d.rectangle([491, 0, 756, 306], fill=(11, 14, 21, 255))
    # Top Viewport Header Bar
    d.rectangle([491, 0, 756, 28], fill=(18, 22, 30, 255))
    # Engine status indicator (green dot = 60 FPS active)
    d.ellipse([504, 11, 512, 19], fill=(0, 255, 136, 255))
    
    # Isometric Tech Grid Lines
    grid_col = (0, 180, 255, 24)
    for x in range(500, 750, 32):
        d.line([(x, 140), (x - 50, 270)], fill=grid_col, width=1)
        d.line([(x, 140), (x + 50, 270)], fill=grid_col, width=1)
        
    # Wireframe 3D Gem / Diamond (representing active game asset in scene)
    cx, cy = 623, 145
    d.polygon([(cx, cy - 32), (cx + 32, cy), (cx, cy + 32), (cx - 32, cy)], outline=(0, 229, 255, 170), fill=(0, 180, 255, 30))
    d.line([(cx, cy - 32), (cx, cy + 32)], fill=(0, 229, 255, 150), width=1)
    d.line([(cx - 32, cy), (cx + 32, cy)], fill=(0, 229, 255, 150), width=1)
    
    # Bottom telemetry bar
    d.rectangle([491, 280, 756, 306], fill=(14, 17, 24, 255))
    d.line([(491, 280), (756, 280)], fill=(26, 32, 44, 255), width=1)
    
    im.save(os.path.join(sprites_dir, "spr_monitor_screen.png"))
    print("Created spr_monitor_screen.png")

# 4. Mechanical Keyboard (540 x 200)
def make_keyboard():
    im = Image.new("RGBA", (540, 200), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Keyboard case (Chassis)
    create_rounded_rect(d, (10, 10, 530, 190), 14, fill=(28, 30, 38, 255), outline=(55, 60, 75, 255), width=3)
    # Inner switch plate
    create_rounded_rect(d, (20, 20, 520, 180), 8, fill=(18, 19, 24, 255))
    
    # 5 Rows of keycaps
    row_y = [28, 58, 88, 118, 148]
    key_h = 24
    
    for r_idx, y in enumerate(row_y):
        if r_idx == 4: # Bottom spacebar row
            # Ctrl, Win, Alt
            create_rounded_rect(d, (28, y, 62, y+key_h), 4, fill=(40, 44, 56, 255), outline=(60, 66, 85, 255))
            create_rounded_rect(d, (68, y, 102, y+key_h), 4, fill=(40, 44, 56, 255), outline=(60, 66, 85, 255))
            create_rounded_rect(d, (108, y, 142, y+key_h), 4, fill=(40, 44, 56, 255), outline=(60, 66, 85, 255))
            # Spacebar (illuminated cyan)
            create_rounded_rect(d, (148, y, 380, y+key_h), 4, fill=(0, 180, 216, 220), outline=(0, 229, 255, 255))
            # Right modifiers
            create_rounded_rect(d, (386, y, 420, y+key_h), 4, fill=(40, 44, 56, 255), outline=(60, 66, 85, 255))
            create_rounded_rect(d, (426, y, 460, y+key_h), 4, fill=(40, 44, 56, 255), outline=(60, 66, 85, 255))
            create_rounded_rect(d, (466, y, 512, y+key_h), 4, fill=(255, 75, 120, 230), outline=(255, 110, 150, 255))
        else:
            # Normal key rows
            cur_x = 28
            while cur_x < 510:
                kw = 30
                if r_idx == 0 and cur_x > 460: kw = 48 # Backspace
                elif r_idx == 1 and cur_x == 28: kw = 42 # Tab
                elif r_idx == 2 and cur_x == 28: kw = 48 # Caps
                elif r_idx == 2 and cur_x > 450: kw = 58 # Enter
                elif r_idx == 3 and cur_x == 28: kw = 64 # LShift
                elif r_idx == 3 and cur_x > 430: kw = 78 # RShift
                
                if cur_x + kw > 514: kw = 514 - cur_x
                if kw < 10: break
                
                # Accent colors for WASD or special keys
                kfill = (38, 42, 54, 255)
                kout = (58, 64, 82, 255)
                if r_idx == 0 and cur_x == 28: # ESC key (illuminated cyan)
                    kfill = (0, 180, 216, 220)
                    kout = (0, 229, 255, 255)
                elif r_idx == 1 and cur_x in range(95, 140): # W key
                    kfill = (0, 180, 216, 200)
                    kout = (0, 229, 255, 255)
                elif r_idx == 2 and cur_x in range(75, 185): # A, S, D keys
                    kfill = (0, 180, 216, 200)
                    kout = (0, 229, 255, 255)
                elif r_idx == 2 and cur_x > 440: # Enter
                    kfill = (0, 230, 118, 220)
                    kout = (0, 255, 136, 255)
                    
                create_rounded_rect(d, (cur_x, y, cur_x+kw, y+key_h), 4, fill=kfill, outline=kout)
                cur_x += kw + 5
                
    im.save(os.path.join(sprites_dir, "spr_keyboard.png"))
    print("Created spr_keyboard.png")

# 5. Gaming Mouse with Braided Cord (100 x 180)
def make_mouse():
    im = Image.new("RGBA", (100, 180), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Braided mouse cord going upward
    d.line([(50, 25), (50, 0)], fill=(20, 22, 28, 255), width=4)
    d.line([(50, 25), (50, 0)], fill=(45, 50, 65, 255), width=2)
    # Cord strain relief boot
    create_rounded_rect(d, (46, 20, 54, 28), 2, fill=(35, 38, 48, 255))
    # Mouse Body
    create_rounded_rect(d, (10, 25, 90, 165), 28, fill=(28, 30, 38, 255), outline=(55, 60, 75, 255), width=2)
    # Split seam
    d.line([(50, 25), (50, 42)], fill=(55, 60, 75, 255), width=2)
    d.line([(50, 72), (50, 110)], fill=(55, 60, 75, 255), width=2)
    # Scroll wheel
    create_rounded_rect(d, (44, 42, 56, 72), 4, fill=(30, 34, 44, 255), outline=(65, 75, 95, 255))
    im.save(os.path.join(sprites_dir, "spr_mouse.png"))
    print("Created spr_mouse.png")

# 5b. Gaming Mouse RGB Glow Layer (100 x 180)
def make_mouse_glow():
    im = Image.new("RGBA", (100, 180), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # RGB side strips
    d.arc([14, 38, 86, 155], start=45, end=135, fill=(255, 255, 255, 240), width=4)
    d.arc([14, 38, 86, 155], start=225, end=315, fill=(255, 255, 255, 240), width=4)
    # Illuminated scroll wheel
    create_rounded_rect(d, (45, 43, 55, 71), 3, fill=(255, 255, 255, 220))
    # Cord RGB pulse dot
    d.ellipse([47, 6, 53, 12], fill=(255, 255, 255, 200))
    im.save(os.path.join(sprites_dir, "spr_mouse_glow.png"))
    print("Created spr_mouse_glow.png")

# 6. Coffee Mug (120 x 140)
def make_coffee_mug():
    im = Image.new("RGBA", (120, 140), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Mug Handle
    create_rounded_rect(d, (72, 45, 110, 105), 14, fill=(0, 0, 0, 0), outline=(220, 225, 235, 255), width=7)
    # Mug Body
    create_rounded_rect(d, (15, 30, 85, 125), 12, fill=(235, 240, 248, 255), outline=(180, 190, 205, 255), width=2)
    # Coffee inside top
    d.ellipse([18, 32, 82, 48], fill=(62, 38, 24, 255))
    # Steam curls
    d.arc([30, 4, 46, 26], start=200, end=340, fill=(200, 220, 240, 140), width=3)
    d.arc([52, 2, 68, 24], start=200, end=340, fill=(200, 220, 240, 140), width=3)
    # C# Code logo on mug
    d.rectangle([34, 66, 66, 92], fill=(81, 43, 212, 255))
    d.arc([38, 70, 54, 88], start=60, end=300, fill=(255, 255, 255, 255), width=3)
    d.line([(57, 72), (57, 86)], fill=(255, 255, 255, 255), width=2)
    d.line([(62, 72), (62, 86)], fill=(255, 255, 255, 255), width=2)
    d.line([(54, 76), (65, 76)], fill=(255, 255, 255, 255), width=2)
    d.line([(54, 82), (65, 82)], fill=(255, 255, 255, 255), width=2)
    im.save(os.path.join(sprites_dir, "spr_coffee_mug.png"))
    print("Created spr_coffee_mug.png")

# 7. Energy Drink Can (100 x 170)
def make_energy_can():
    im = Image.new("RGBA", (100, 170), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Top rim & tab
    d.ellipse([22, 10, 78, 26], fill=(190, 195, 205, 255), outline=(130, 135, 145, 255), width=2)
    d.ellipse([42, 14, 58, 22], fill=(120, 125, 135, 255))
    # Can body
    create_rounded_rect(d, (20, 20, 80, 155), 10, fill=(20, 24, 34, 255), outline=(0, 229, 255, 255), width=2)
    # Neon Lightning Bolt / x2 Logo
    bolt = [(56, 36), (36, 75), (50, 75), (42, 110), (66, 68), (52, 68)]
    d.polygon(bolt, fill=(0, 255, 136, 255))
    # "2X" text badge
    create_rounded_rect(d, (28, 120, 72, 145), 6, fill=(255, 0, 110, 230))
    im.save(os.path.join(sprites_dir, "spr_energy_can.png"))
    print("Created spr_energy_can.png")

# 8. Cute Sleeping Desk Cat (160 x 110)
def make_cat():
    im = Image.new("RGBA", (160, 110), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Cat body ball
    d.ellipse([30, 30, 140, 95], fill=(235, 140, 50, 255), outline=(190, 100, 30, 255), width=2)
    # Cat head
    d.ellipse([15, 38, 75, 88], fill=(245, 150, 60, 255), outline=(190, 100, 30, 255), width=2)
    # Ears
    d.polygon([(22, 45), (14, 20), (38, 35)], fill=(245, 150, 60, 255))
    d.polygon([(48, 35), (66, 18), (62, 45)], fill=(245, 150, 60, 255))
    # Sleeping eyes (curved lines)
    d.arc([28, 55, 42, 68], start=20, end=160, fill=(60, 30, 10, 255), width=2)
    d.arc([46, 55, 60, 68], start=20, end=160, fill=(60, 30, 10, 255), width=2)
    # Tail curled around
    d.arc([80, 45, 150, 102], start=280, end=120, fill=(220, 125, 40, 255), width=9)
    # White paws
    d.ellipse([25, 78, 45, 96], fill=(255, 250, 240, 255))
    im.save(os.path.join(sprites_dir, "spr_cat.png"))
    print("Created spr_cat.png")

def make_cat_awake():
    im = Image.new("RGBA", (160, 110), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Cat body ball
    d.ellipse([30, 30, 140, 95], fill=(235, 140, 50, 255), outline=(190, 100, 30, 255), width=2)
    # Cat head
    d.ellipse([15, 38, 75, 88], fill=(245, 150, 60, 255), outline=(190, 100, 30, 255), width=2)
    # Outer Ears
    d.polygon([(22, 45), (14, 18), (38, 33)], fill=(245, 150, 60, 255))
    d.polygon([(48, 33), (68, 16), (62, 45)], fill=(245, 150, 60, 255))
    # Inner pink ears
    d.polygon([(22, 40), (16, 22), (34, 34)], fill=(255, 185, 195, 255))
    d.polygon([(50, 34), (64, 20), (59, 41)], fill=(255, 185, 195, 255))
    
    # Big sparkling open eyes!
    # Left eye
    d.ellipse([25, 52, 41, 72], fill=(20, 30, 48, 255), outline=(15, 20, 35, 255), width=1)
    d.ellipse([27, 54, 34, 62], fill=(255, 255, 255, 255))
    d.ellipse([34, 64, 39, 69], fill=(255, 255, 255, 255))
    # Right eye
    d.ellipse([47, 52, 63, 72], fill=(20, 30, 48, 255), outline=(15, 20, 35, 255), width=1)
    d.ellipse([49, 54, 56, 62], fill=(255, 255, 255, 255))
    d.ellipse([56, 64, 61, 69], fill=(255, 255, 255, 255))
    
    # Cheerful pink blush
    d.ellipse([18, 68, 28, 75], fill=(255, 120, 150, 160))
    d.ellipse([60, 68, 70, 75], fill=(255, 120, 150, 160))
    
    # Cute nose & w mouth
    d.polygon([(43, 67), (47, 67), (45, 70)], fill=(255, 130, 150, 255))
    d.arc([39, 68, 45, 75], start=0, end=180, fill=(60, 30, 10, 255), width=2)
    d.arc([45, 68, 51, 75], start=0, end=180, fill=(60, 30, 10, 255), width=2)
    
    # Whiskers
    d.line([(9, 64), (22, 66)], fill=(80, 40, 15, 220), width=1)
    d.line([(8, 70), (22, 69)], fill=(80, 40, 15, 220), width=1)
    d.line([(65, 66), (79, 64)], fill=(80, 40, 15, 220), width=1)
    d.line([(65, 69), (80, 70)], fill=(80, 40, 15, 220), width=1)
    
    # Tail curled around
    d.arc([80, 45, 150, 102], start=280, end=120, fill=(220, 125, 40, 255), width=9)
    # White paws
    d.ellipse([25, 78, 45, 96], fill=(255, 250, 240, 255))
    im.save(os.path.join(sprites_dir, "spr_cat_awake.png"))
    print("Created spr_cat_awake.png")

def make_heart():
    im = Image.new("RGBA", (48, 48), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.ellipse([6, 6, 26, 26], fill=(255, 55, 105, 255))
    d.ellipse([22, 6, 42, 26], fill=(255, 55, 105, 255))
    d.polygon([(7, 18), (41, 18), (24, 43)], fill=(255, 55, 105, 255))
    d.ellipse([10, 10, 18, 18], fill=(255, 190, 210, 220))
    im.save(os.path.join(sprites_dir, "spr_heart.png"))
    print("Created spr_heart.png")

# 9. Rounded UI Card Background (400 x 120, 9-sliceable)
def make_card_bg():
    im = Image.new("RGBA", (400, 120), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    create_rounded_rect(d, (4, 4, 396, 116), 14, fill=(25, 29, 39, 245), outline=(48, 54, 72, 255), width=2)
    im.save(os.path.join(sprites_dir, "spr_card_bg.png"))
    print("Created spr_card_bg.png")

# 10. UI Buttons (Active Cyan, Inactive Grey, Orange Reset)
def make_button_sprites():
    # Active Cyan Button
    im = Image.new("RGBA", (200, 70), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    create_rounded_rect(d, (3, 3, 197, 67), 10, fill=(0, 175, 145, 255), outline=(0, 255, 180, 255), width=2)
    im.save(os.path.join(sprites_dir, "spr_btn_cyan.png"))

    # Orange Reset Button
    im = Image.new("RGBA", (200, 70), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    create_rounded_rect(d, (3, 3, 197, 67), 10, fill=(220, 70, 50, 255), outline=(255, 110, 90, 255), width=2)
    im.save(os.path.join(sprites_dir, "spr_btn_orange.png"))

    # Energy Gold Button
    im = Image.new("RGBA", (200, 70), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    create_rounded_rect(d, (3, 3, 197, 67), 10, fill=(230, 160, 15, 255), outline=(255, 215, 0, 255), width=2)
    im.save(os.path.join(sprites_dir, "spr_btn_gold.png"))
    print("Created button sprites")

# 11. Soft Steam Cloud Particle (48 x 48)
def make_steam_particle():
    im = Image.new("RGBA", (48, 48), (0, 0, 0, 0))
    for y in range(48):
        for x in range(48):
            dx = x - 23.5
            dy = y - 23.5
            dist = math.sqrt(dx*dx + dy*dy) / 22.0
            if dist <= 1.0:
                a = int(((1.0 - dist) ** 1.6) * 220)
                im.putpixel((x, y), (240, 245, 255, a))
    im.save(os.path.join(sprites_dir, "spr_steam_puff.png"))
    print("Created spr_steam_puff.png")

# 12. Glowing Fizzy Bubble Particle (24 x 24)
def make_bubble_particle():
    im = Image.new("RGBA", (24, 24), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.ellipse([2, 2, 21, 21], fill=(0, 220, 255, 60), outline=(0, 255, 220, 230), width=2)
    d.ellipse([6, 5, 10, 9], fill=(255, 255, 255, 240))
    im.save(os.path.join(sprites_dir, "spr_bubble_spark.png"))
    print("Created spr_bubble_spark.png")

def make_stickers():
    # 1. Unity Hex Badge (64x64)
    im = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    hex_pts = [(32, 2), (58, 17), (58, 47), (32, 62), (6, 47), (6, 17)]
    d.polygon(hex_pts, fill=(18, 22, 30, 255), outline=(0, 220, 255, 230), width=2)
    d.polygon([(32, 22), (42, 28), (42, 38), (32, 44), (22, 38), (22, 28)], fill=(30, 36, 48, 255))
    d.polygon([(32, 10), (46, 18), (38, 23), (32, 19), (26, 23), (18, 18)], fill=(255, 255, 255, 255))
    d.polygon([(54, 28), (54, 44), (44, 49), (44, 42), (48, 38), (44, 25)], fill=(210, 225, 240, 255))
    d.polygon([(10, 28), (10, 44), (20, 49), (20, 42), (16, 38), (20, 25)], fill=(210, 225, 240, 255))
    d.ellipse([30, 31, 34, 35], fill=(0, 255, 200, 255))
    im.save(os.path.join(sprites_dir, "spr_sticker_unity.png"))
    print("Created spr_sticker_unity.png")

    # 2. C# Purple Badge (64x64)
    im = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    hex_pts = [(32, 2), (58, 17), (58, 47), (32, 62), (6, 47), (6, 17)]
    d.polygon(hex_pts, fill=(35, 15, 45, 255), outline=(180, 80, 230, 230), width=2)
    d.arc([14, 18, 36, 46], start=50, end=310, fill=(255, 255, 255, 255), width=5)
    d.line([(38, 22), (36, 44)], fill=(210, 110, 255, 255), width=3)
    d.line([(46, 22), (44, 44)], fill=(210, 110, 255, 255), width=3)
    d.line([(33, 28), (49, 28)], fill=(210, 110, 255, 255), width=3)
    d.line([(31, 38), (47, 38)], fill=(210, 110, 255, 255), width=3)
    im.save(os.path.join(sprites_dir, "spr_sticker_csharp.png"))
    print("Created spr_sticker_csharp.png")

    # 3. Git Branch Diamond Badge (64x64)
    im = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    diamond_pts = [(32, 4), (60, 32), (32, 60), (4, 32)]
    d.polygon(diamond_pts, fill=(240, 80, 50, 255), outline=(255, 140, 110, 240), width=2)
    d.line([(22, 42), (42, 22)], fill=(255, 255, 255, 255), width=4)
    d.line([(30, 34), (40, 38)], fill=(255, 255, 255, 255), width=4)
    d.ellipse([18, 38, 26, 46], fill=(255, 255, 255, 255))
    d.ellipse([38, 18, 46, 26], fill=(255, 255, 255, 255))
    d.ellipse([37, 34, 45, 42], fill=(255, 255, 255, 255))
    im.save(os.path.join(sprites_dir, "spr_sticker_git.png"))
    print("Created spr_sticker_git.png")

    # 4. "It Works On My Machine" Bumper Sticker (110x44)
    im = Image.new("RGBA", (110, 44), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    create_rounded_rect(d, (2, 2, 108, 42), 8, fill=(16, 20, 26, 250), outline=(0, 255, 136, 220), width=2)
    d.ellipse([8, 14, 22, 28], fill=(0, 255, 136, 230))
    d.line([(11, 21), (14, 25), (19, 17)], fill=(16, 20, 26, 255), width=2)
    d.text((28, 8), "IT WORKS", fill=(0, 255, 136, 255))
    d.text((28, 22), "ON MY MACHINE", fill=(180, 210, 240, 240))
    im.save(os.path.join(sprites_dir, "spr_sticker_works.png"))
    print("Created spr_sticker_works.png")

# 12. Modern Desk Lamp (140 x 220)
def make_desk_lamp():
    im = Image.new("RGBA", (140, 220), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Heavy weighted base
    create_rounded_rect(d, (25, 192, 115, 214), 8, fill=(28, 32, 42, 255), outline=(55, 62, 80, 255), width=2)
    # Metallic switch button on base
    create_rounded_rect(d, (62, 186, 78, 194), 3, fill=(255, 200, 50, 255), outline=(180, 140, 20, 255), width=1)
    
    # Lower metal arm
    d.line([(70, 192), (48, 115)], fill=(45, 52, 68, 255), width=6)
    d.line([(70, 192), (48, 115)], fill=(75, 86, 110, 255), width=2)
    # Middle joint pivot
    d.ellipse([42, 108, 54, 120], fill=(25, 28, 36, 255), outline=(90, 105, 135, 255), width=2)
    
    # Upper arm leaning toward right
    d.line([(48, 114), (92, 58)], fill=(45, 52, 68, 255), width=6)
    d.line([(48, 114), (92, 58)], fill=(75, 86, 110, 255), width=2)
    # Head joint
    d.ellipse([86, 52, 98, 64], fill=(25, 28, 36, 255), outline=(90, 105, 135, 255), width=2)
    
    # Lamp shade (cone pointing down-right)
    shade_pts = [(88, 48), (124, 76), (114, 94), (74, 62)]
    d.polygon(shade_pts, fill=(32, 38, 50, 255), outline=(65, 75, 98, 255))
    # Inner reflector warm glow
    d.polygon([(114, 74), (128, 85), (112, 97)], fill=(255, 235, 160, 255))
    d.ellipse([108, 76, 124, 92], fill=(255, 250, 200, 255))
    
    im.save(os.path.join(sprites_dir, "spr_desk_lamp.png"))
    print("Created spr_desk_lamp.png")

# 13. Lamp Light Cone Overlay (600 x 500)
def make_lamp_cone():
    im = Image.new("RGBA", (600, 500), (0, 0, 0, 0))
    # Build soft gradient light cone
    cx, cy = 110, 50 # Lamp head source
    for y in range(500):
        for x in range(600):
            dx = x - cx
            dy = y - cy
            dist = math.sqrt(dx * dx + dy * dy)
            if dist < 10 or dist > 550:
                continue
            angle = math.atan2(dy, dx) # in radians (-pi to pi)
            # Beam points down-right: angle between 0.35 rad (~20 deg) and 1.35 rad (~77 deg)
            beam_center = 0.85
            beam_width = 0.50
            diff = abs(angle - beam_center)
            if diff < beam_width:
                ang_factor = math.cos((diff / beam_width) * (math.pi / 2.0))
                dist_factor = max(0.0, 1.0 - (dist / 550.0))
                intensity = ang_factor * dist_factor * 0.40 # max alpha ~0.40
                if intensity > 0.01:
                    alpha = int(intensity * 255)
                    im.putpixel((x, y), (255, 230, 140, alpha))
    im.save(os.path.join(sprites_dir, "spr_lamp_cone.png"))
    print("Created spr_lamp_cone.png")

# 14. Cute Stretching Cat Pose (170 x 110)
def make_cat_stretch():
    im = Image.new("RGBA", (170, 110), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Cat body arched up in back, sloping down to front paws
    # Back arch
    d.ellipse([60, 22, 148, 86], fill=(235, 140, 50, 255), outline=(190, 100, 30, 255), width=2)
    # Front torso sloping down
    d.polygon([(50, 72), (90, 48), (92, 85), (45, 90)], fill=(245, 150, 60, 255))
    
    # Head down near mat
    d.ellipse([26, 52, 74, 94], fill=(245, 150, 60, 255), outline=(190, 100, 30, 255), width=2)
    # Ears flat/perked backwards
    d.polygon([(46, 54), (58, 34), (68, 52)], fill=(245, 150, 60, 255))
    d.polygon([(48, 50), (58, 38), (64, 49)], fill=(255, 185, 195, 255))
    
    # Stretched front paws reaching out forward
    d.ellipse([10, 84, 46, 98], fill=(255, 250, 240, 255), outline=(200, 120, 40, 255), width=1)
    d.line([(22, 84), (22, 98)], fill=(200, 180, 170, 255), width=1)
    d.line([(32, 84), (32, 98)], fill=(200, 180, 170, 255), width=1)
    
    # Rear standing legs & paws
    d.ellipse([118, 72, 144, 98], fill=(245, 150, 60, 255), outline=(190, 100, 30, 255), width=1)
    d.ellipse([124, 88, 148, 99], fill=(255, 250, 240, 255))
    
    # Joyful arched tail curling up
    d.arc([115, 10, 168, 75], start=240, end=90, fill=(220, 125, 40, 255), width=8)
    
    # Blissful stretching eyes (happy slits)
    d.arc([35, 68, 47, 80], start=200, end=340, fill=(70, 35, 15, 255), width=2)
    d.arc([51, 68, 63, 80], start=200, end=340, fill=(70, 35, 15, 255), width=2)
    # Pink nose
    d.polygon([(47, 80), (51, 80), (49, 83)], fill=(255, 130, 150, 255))
    # Blush
    d.ellipse([30, 78, 38, 84], fill=(255, 130, 160, 150))
    d.ellipse([60, 78, 68, 84], fill=(255, 130, 160, 150))
    
    im.save(os.path.join(sprites_dir, "spr_cat_stretch.png"))
    print("Created spr_cat_stretch.png")

# 15. Cat Accessories (Hacker Glasses & Bowtie)
def make_cat_accessories():
    # Cool Hacker Sunglasses (54 x 26)
    im = Image.new("RGBA", (54, 26), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Left lens
    create_rounded_rect(d, (4, 4, 24, 22), 4, fill=(18, 20, 28, 255), outline=(0, 229, 255, 255), width=2)
    # Right lens
    create_rounded_rect(d, (30, 4, 50, 22), 4, fill=(18, 20, 28, 255), outline=(0, 229, 255, 255), width=2)
    # Bridge
    d.line([(24, 11), (30, 11)], fill=(0, 229, 255, 255), width=2)
    # Glint shine
    d.line([(7, 7), (14, 14)], fill=(255, 255, 255, 220), width=2)
    d.line([(33, 7), (40, 14)], fill=(255, 255, 255, 220), width=2)
    im.save(os.path.join(sprites_dir, "spr_cat_glasses.png"))
    print("Created spr_cat_glasses.png")

    # Red Dapper Bowtie (44 x 26)
    im = Image.new("RGBA", (44, 26), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Left wing
    d.polygon([(22, 13), (4, 4), (4, 22)], fill=(230, 45, 65, 255), outline=(170, 20, 40, 255))
    # Right wing
    d.polygon([(22, 13), (40, 4), (40, 22)], fill=(230, 45, 65, 255), outline=(170, 20, 40, 255))
    # Center golden knot
    create_rounded_rect(d, (18, 8, 26, 18), 3, fill=(255, 205, 50, 255), outline=(190, 150, 20, 255), width=1)
    im.save(os.path.join(sprites_dir, "spr_cat_bowtie.png"))
    print("Created spr_cat_bowtie.png")

# 16. Build Mini-Game Sprites (Falling Bug & Patch Crate)
def make_minigame_sprites():
    # Compile Bug (50 x 50)
    im = Image.new("RGBA", (50, 50), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # 6 Twitchy Legs
    d.line([(10, 16), (22, 22)], fill=(180, 20, 50, 255), width=3)
    d.line([(38, 22), (50, 16)], fill=(180, 20, 50, 255), width=3)
    d.line([(8, 28), (20, 28)], fill=(180, 20, 50, 255), width=3)
    d.line([(30, 28), (42, 28)], fill=(180, 20, 50, 255), width=3)
    d.line([(10, 40), (22, 34)], fill=(180, 20, 50, 255), width=3)
    d.line([(38, 34), (50, 40)], fill=(180, 20, 50, 255), width=3)
    # Bug Shell / Body
    d.ellipse([16, 14, 34, 42], fill=(255, 45, 85, 255), outline=(140, 15, 40, 255), width=2)
    # Bug Head
    d.ellipse([19, 8, 31, 20], fill=(50, 10, 20, 255))
    # Glowing yellow eyes
    d.ellipse([21, 10, 24, 13], fill=(255, 230, 0, 255))
    d.ellipse([26, 10, 29, 13], fill=(255, 230, 0, 255))
    # Antennae
    d.line([(22, 9), (16, 3)], fill=(50, 10, 20, 255), width=2)
    d.line([(28, 9), (34, 3)], fill=(50, 10, 20, 255), width=2)
    im.save(os.path.join(sprites_dir, "spr_minigame_bug.png"))
    print("Created spr_minigame_bug.png")

    # Patch Crate (54 x 54)
    im = Image.new("RGBA", (54, 54), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Box body
    create_rounded_rect(d, (4, 4, 50, 50), 6, fill=(28, 34, 48, 255), outline=(0, 229, 255, 255), width=2)
    # Cross banding
    d.line([(4, 4), (50, 50)], fill=(45, 58, 80, 255), width=3)
    d.line([(4, 50), (50, 4)], fill=(45, 58, 80, 255), width=3)
    # Golden package label in center
    create_rounded_rect(d, (14, 16, 40, 38), 4, fill=(255, 190, 40, 255), outline=(200, 140, 20, 255), width=1)
    d.polygon([(27, 20), (21, 28), (25, 28), (25, 34), (29, 34), (29, 28), (33, 28)], fill=(30, 30, 40, 255))
    im.save(os.path.join(sprites_dir, "spr_minigame_crate.png"))
    print("Created spr_minigame_crate.png")

# 17. Vintage Retro Mechanical Keyboard (540 x 200)
def make_keyboard_retro():
    im = Image.new("RGBA", (540, 200), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Retro Beige Chassis
    create_rounded_rect(d, (10, 10, 530, 190), 14, fill=(218, 212, 196, 255), outline=(175, 168, 150, 255), width=3)
    create_rounded_rect(d, (20, 20, 520, 180), 8, fill=(188, 182, 166, 255))
    row_y = [28, 58, 88, 118, 148]
    key_h = 24
    for r_idx, y in enumerate(row_y):
        if r_idx == 4:
            create_rounded_rect(d, (28, y, 62, y+key_h), 4, fill=(150, 144, 130, 255), outline=(125, 120, 108, 255))
            create_rounded_rect(d, (68, y, 102, y+key_h), 4, fill=(150, 144, 130, 255), outline=(125, 120, 108, 255))
            create_rounded_rect(d, (148, y, 380, y+key_h), 4, fill=(238, 234, 222, 255), outline=(195, 190, 178, 255))
            create_rounded_rect(d, (386, y, 420, y+key_h), 4, fill=(150, 144, 130, 255), outline=(125, 120, 108, 255))
            create_rounded_rect(d, (426, y, 460, y+key_h), 4, fill=(150, 144, 130, 255), outline=(125, 120, 108, 255))
            create_rounded_rect(d, (466, y, 512, y+key_h), 4, fill=(180, 50, 45, 255), outline=(145, 35, 30, 255))
        else:
            cur_x = 28
            while cur_x < 510:
                kw = 30
                if r_idx == 0 and cur_x > 460: kw = 48
                elif r_idx == 1 and cur_x == 28: kw = 42
                elif r_idx == 2 and cur_x == 28: kw = 48
                elif r_idx == 2 and cur_x > 450: kw = 58
                elif r_idx == 3 and cur_x == 28: kw = 64
                elif r_idx == 3 and cur_x > 430: kw = 78
                if cur_x + kw > 514: kw = 514 - cur_x
                if kw < 10: break

                kfill = (238, 234, 222, 255)
                kout = (195, 190, 178, 255)
                if r_idx == 0 and cur_x == 28:
                    kfill = (205, 55, 45, 255) # Red vintage ESC
                    kout = (165, 40, 30, 255)
                elif (r_idx == 1 and cur_x in range(95, 140)) or (r_idx == 2 and cur_x in range(75, 185)):
                    kfill = (248, 245, 235, 255) # Light ivory WASD
                    kout = (185, 180, 168, 255)
                elif (r_idx in (1, 2, 3) and cur_x == 28) or (r_idx in (0, 1, 2, 3) and cur_x > 430):
                    kfill = (150, 144, 130, 255) # Vintage gray modifiers
                    kout = (125, 120, 108, 255)

                create_rounded_rect(d, (cur_x, y, cur_x+kw, y+key_h), 4, fill=kfill, outline=kout)
                cur_x += kw + 5
    im.save(os.path.join(sprites_dir, "spr_keyboard_retro.png"))
    print("Created spr_keyboard_retro.png")

# 18. Tokyo Night Cyberpunk Neon Keyboard (540 x 200)
def make_keyboard_neon():
    im = Image.new("RGBA", (540, 200), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Deep Neon Chassis
    create_rounded_rect(d, (10, 10, 530, 190), 14, fill=(28, 16, 44, 255), outline=(190, 40, 220, 255), width=3)
    create_rounded_rect(d, (20, 20, 520, 180), 8, fill=(18, 10, 30, 255))
    row_y = [28, 58, 88, 118, 148]
    key_h = 24
    for r_idx, y in enumerate(row_y):
        if r_idx == 4:
            create_rounded_rect(d, (28, y, 62, y+key_h), 4, fill=(0, 200, 230, 220), outline=(0, 240, 255, 255))
            create_rounded_rect(d, (68, y, 102, y+key_h), 4, fill=(45, 24, 70, 255), outline=(75, 45, 110, 255))
            # Spacebar Neon Magenta
            create_rounded_rect(d, (148, y, 380, y+key_h), 4, fill=(255, 30, 130, 230), outline=(255, 80, 170, 255))
            create_rounded_rect(d, (386, y, 420, y+key_h), 4, fill=(45, 24, 70, 255), outline=(75, 45, 110, 255))
            create_rounded_rect(d, (426, y, 460, y+key_h), 4, fill=(0, 200, 230, 220), outline=(0, 240, 255, 255))
            create_rounded_rect(d, (466, y, 512, y+key_h), 4, fill=(255, 190, 30, 230), outline=(255, 220, 80, 255))
        else:
            cur_x = 28
            while cur_x < 510:
                kw = 30
                if r_idx == 0 and cur_x > 460: kw = 48
                elif r_idx == 1 and cur_x == 28: kw = 42
                elif r_idx == 2 and cur_x == 28: kw = 48
                elif r_idx == 2 and cur_x > 450: kw = 58
                elif r_idx == 3 and cur_x == 28: kw = 64
                elif r_idx == 3 and cur_x > 430: kw = 78
                if cur_x + kw > 514: kw = 514 - cur_x
                if kw < 10: break

                kfill = (45, 25, 70, 255)
                kout = (85, 50, 120, 255)
                if r_idx == 0 and cur_x == 28:
                    kfill = (255, 30, 130, 240) # Bright neon pink ESC
                    kout = (255, 90, 180, 255)
                elif (r_idx == 1 and cur_x in range(95, 140)) or (r_idx == 2 and cur_x in range(75, 185)):
                    kfill = (255, 45, 140, 220) # Neon Pink WASD
                    kout = (255, 100, 190, 255)
                elif r_idx == 2 and cur_x > 440:
                    kfill = (0, 230, 255, 240) # Cyan Enter
                    kout = (120, 255, 255, 255)

                create_rounded_rect(d, (cur_x, y, cur_x+kw, y+key_h), 4, fill=kfill, outline=kout)
                cur_x += kw + 5
    im.save(os.path.join(sprites_dir, "spr_keyboard_neon.png"))
    print("Created spr_keyboard_neon.png")

# 19. Desktop Lo-Fi Chillhop Player (160 x 95)
def make_lofi_player():
    im = Image.new("RGBA", (160, 95), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Chassis
    create_rounded_rect(d, (4, 4, 156, 91), 8, fill=(22, 25, 34, 255), outline=(50, 58, 76, 255), width=2)
    # LED Screen window
    create_rounded_rect(d, (12, 10, 148, 52), 6, fill=(10, 14, 20, 255), outline=(0, 229, 255, 150), width=1)
    # Mini cassette reels or visualizer bars
    d.ellipse([26, 20, 48, 42], fill=(20, 26, 36, 255), outline=(0, 229, 255, 220), width=2)
    d.ellipse([34, 28, 40, 34], fill=(0, 229, 255, 255))
    d.ellipse([112, 20, 134, 42], fill=(20, 26, 36, 255), outline=(0, 229, 255, 220), width=2)
    d.ellipse([120, 28, 126, 34], fill=(0, 229, 255, 255))
    # Sound wave line between reels
    d.line([(52, 31), (108, 31)], fill=(0, 229, 255, 180), width=2)
    # Mini EQ bars in center
    eq_x = [62, 70, 78, 86, 94]
    eq_h = [12, 18, 14, 20, 10]
    for x, h in zip(eq_x, eq_h):
        d.line([(x, 40), (x, 40 - h)], fill=(0, 255, 136, 220), width=3)
    # Control buttons row
    create_rounded_rect(d, (20, 60, 52, 82), 4, fill=(35, 42, 58, 255), outline=(60, 72, 98, 255), width=1)
    d.polygon([(32, 66), (42, 71), (32, 76)], fill=(0, 255, 136, 255)) # Play icon
    create_rounded_rect(d, (64, 60, 96, 82), 4, fill=(35, 42, 58, 255), outline=(60, 72, 98, 255), width=1)
    d.polygon([(74, 66), (82, 71), (74, 76)], fill=(0, 229, 255, 255)) # Next icon
    d.polygon([(82, 66), (90, 71), (82, 76)], fill=(0, 229, 255, 255))
    create_rounded_rect(d, (108, 60, 140, 82), 4, fill=(35, 42, 58, 255), outline=(60, 72, 98, 255), width=1)
    d.ellipse([120, 67, 128, 75], fill=(255, 90, 140, 255)) # Mute/Mode led
    im.save(os.path.join(sprites_dir, "spr_lofi_player.png"))
    print("Created spr_lofi_player.png")

# 24. Wallpapers for Room Backdrop (Cozy Indie Studio, Cyberpunk Loft, Minimal Dark Tech)
def make_wallpapers():
    # 1. Cozy Indie Studio (Warm dark cedar, fairy lights, bookshelf)
    im_cozy = Image.new("RGBA", (540, 960), (28, 24, 22, 255))
    dc = ImageDraw.Draw(im_cozy)
    # Horizontal cedar plank seams
    for y in range(0, 960, 48):
        dc.line([(0, y), (540, y)], fill=(20, 17, 15, 255), width=2)
        dc.line([(0, y+1), (540, y+1)], fill=(38, 32, 28, 180), width=1)
    # Wooden wall shelf at top
    create_rounded_rect(dc, (40, 150, 500, 168), 4, fill=(45, 36, 30, 255), outline=(65, 52, 42, 255), width=2)
    # Books on shelf
    colors = [(180, 50, 60), (45, 120, 170), (50, 150, 90), (200, 150, 40), (140, 70, 150)]
    for i, col in enumerate(colors):
        bx = 70 + i * 22
        dc.rectangle([bx, 110, bx + 18, 150], fill=(*col, 255))
        dc.rectangle([bx + 3, 114, bx + 15, 146], fill=(*col, 200))
    # Plant succulent pot
    create_rounded_rect(dc, (430, 130, 470, 150), 3, fill=(160, 90, 60, 255))
    dc.ellipse([435, 115, 465, 132], fill=(60, 160, 80, 255))
    # Hanging fairy lights garland
    for i in range(12):
        lx = 30 + i * 42
        ly = 65 + int(math.sin(i * 0.6) * 14)
        # Glow halo
        dc.ellipse([lx - 12, ly - 12, lx + 12, ly + 12], fill=(255, 220, 120, 45))
        dc.ellipse([lx - 6, ly - 6, lx + 6, ly + 6], fill=(255, 240, 160, 120))
        # Bulb
        dc.ellipse([lx - 3, ly - 3, lx + 3, ly + 3], fill=(255, 255, 220, 255))
    im_cozy.save(os.path.join(sprites_dir, "spr_wallpaper_cozy.png"))
    print("Created spr_wallpaper_cozy.png")

    # 2. Cyberpunk Loft (Dark brick, glowing neon kanji/signs, neon grid)
    im_cyber = Image.new("RGBA", (540, 960), (16, 14, 24, 255))
    dcy = ImageDraw.Draw(im_cyber)
    # Subtle dark brick texture
    for row in range(0, 960, 32):
        shift = 30 if (row // 32) % 2 == 1 else 0
        dcy.line([(0, row), (540, row)], fill=(22, 19, 32, 255), width=2)
        for col in range(-30 + shift, 570, 60):
            dcy.line([(col, row), (col, row + 32)], fill=(22, 19, 32, 255), width=2)
    # Glowing neon signs on wall
    # "DEV" sign box
    create_rounded_rect(dcy, (70, 90, 210, 145), 8, fill=(24, 18, 42, 200), outline=(0, 229, 255, 180), width=3)
    # Outer neon glow
    create_rounded_rect(dcy, (66, 86, 214, 149), 10, fill=(0, 0, 0, 0), outline=(0, 229, 255, 60), width=4)
    # Neon letters "DEV"
    dcy.line([(95, 105), (95, 130)], fill=(0, 255, 255, 255), width=3)
    dcy.line([(95, 105), (115, 105)], fill=(0, 255, 255, 255), width=3)
    dcy.line([(115, 105), (115, 117)], fill=(0, 255, 255, 255), width=3)
    dcy.line([(95, 117), (115, 117)], fill=(0, 255, 255, 255), width=3)
    dcy.line([(95, 130), (115, 117)], fill=(0, 255, 255, 255), width=3)
    # E
    dcy.line([(130, 105), (130, 130)], fill=(0, 255, 255, 255), width=3)
    dcy.line([(130, 105), (148, 105)], fill=(0, 255, 255, 255), width=3)
    dcy.line([(130, 117), (144, 117)], fill=(0, 255, 255, 255), width=3)
    dcy.line([(130, 130), (148, 130)], fill=(0, 255, 255, 255), width=3)
    # V
    dcy.line([(160, 105), (170, 130)], fill=(0, 255, 255, 255), width=3)
    dcy.line([(180, 105), (170, 130)], fill=(0, 255, 255, 255), width=3)

    # Magenta Neon "< / >" sign on right
    create_rounded_rect(dcy, (330, 95, 470, 145), 8, fill=(35, 14, 38, 200), outline=(255, 40, 150, 180), width=3)
    create_rounded_rect(dcy, (326, 91, 474, 149), 10, fill=(0, 0, 0, 0), outline=(255, 40, 150, 60), width=4)
    # <
    dcy.line([(370, 108), (355, 120)], fill=(255, 60, 180, 255), width=3)
    dcy.line([(355, 120), (370, 132)], fill=(255, 60, 180, 255), width=3)
    # /
    dcy.line([(405, 106), (395, 134)], fill=(255, 200, 240, 255), width=3)
    # >
    dcy.line([(430, 108), (445, 120)], fill=(255, 60, 180, 255), width=3)
    dcy.line([(445, 120), (430, 132)], fill=(255, 60, 180, 255), width=3)
    im_cyber.save(os.path.join(sprites_dir, "spr_wallpaper_cyberpunk.png"))
    print("Created spr_wallpaper_cyberpunk.png")

    # 3. Minimal Dark Tech (Clean slate, geometric circuit accents, subtle carbon lines)
    im_min = Image.new("RGBA", (540, 960), (14, 16, 21, 255))
    dm = ImageDraw.Draw(im_min)
    # Subtle diagonal carbon stripe pattern
    for diag in range(-500, 1500, 36):
        dm.line([(diag, 0), (diag + 960, 960)], fill=(18, 21, 28, 255), width=6)
    # Tech circuit nodes on wall
    for cy in [110, 180, 250]:
        dm.line([(60, cy), (480, cy)], fill=(28, 34, 46, 200), width=1)
        dm.ellipse([140 - 4, cy - 4, 140 + 4, cy + 4], fill=(0, 229, 255, 140))
        dm.ellipse([380 - 4, cy - 4, 380 + 4, cy + 4], fill=(0, 229, 255, 140))
    # Elegant glowing line accent
    dm.line([(100, 145), (440, 145)], fill=(0, 229, 255, 90), width=2)
    im_min.save(os.path.join(sprites_dir, "spr_wallpaper_minimal.png"))
    print("Created spr_wallpaper_minimal.png")

# 25. Daily Quest Icon (128 x 128 Badge)
def make_quest_icon():
    im = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Outer golden hex / circle shield
    d.ellipse([10, 10, 118, 118], fill=(24, 28, 38, 255), outline=(255, 200, 60, 255), width=4)
    d.ellipse([18, 18, 110, 110], fill=(0, 0, 0, 0), outline=(0, 229, 255, 140), width=2)
    # Target concentric rings
    d.ellipse([34, 34, 94, 94], fill=(32, 38, 52, 255), outline=(255, 210, 80, 255), width=3)
    d.ellipse([48, 48, 80, 80], fill=(255, 180, 40, 255))
    d.ellipse([58, 58, 70, 70], fill=(255, 250, 200, 255))
    # Crosshair ticks
    d.line([(64, 22), (64, 34)], fill=(255, 210, 80, 255), width=3)
    d.line([(64, 94), (64, 106)], fill=(255, 210, 80, 255), width=3)
    d.line([(22, 64), (34, 64)], fill=(255, 210, 80, 255), width=3)
    d.line([(94, 64), (106, 64)], fill=(255, 210, 80, 255), width=3)
    im.save(os.path.join(sprites_dir, "spr_quest_icon.png"))
    print("Created spr_quest_icon.png")

# 26. Corgi Pet Companion (Sleep & Awake) (256 x 180)
def make_corgi_pet():
    # Sleep Corgi (curled up sleeping peacefully)
    im_sleep = Image.new("RGBA", (256, 180), (0, 0, 0, 0))
    d = ImageDraw.Draw(im_sleep)
    
    # Shadow underneath
    d.ellipse([30, 130, 226, 172], fill=(12, 14, 20, 130))
    
    # Body (Warm golden-tan corgi coat)
    body_col = (228, 148, 64, 255)
    shadow_col = (195, 120, 48, 255)
    white_fur = (248, 246, 242, 255)
    
    # Curled Body
    d.ellipse([50, 60, 210, 155], fill=body_col, outline=shadow_col, width=3)
    # Stubby corgi rump & tiny fluffy tail
    d.ellipse([180, 75, 230, 125], fill=body_col)
    d.ellipse([215, 88, 236, 108], fill=white_fur) # White fluffy tail tip
    
    # White belly & chest patch
    d.ellipse([70, 95, 170, 155], fill=white_fur)
    
    # Head resting on paws
    d.ellipse([40, 68, 135, 150], fill=body_col, outline=shadow_col, width=3)
    # White blaze on forehead down to snout
    d.polygon([(82, 70), (92, 70), (98, 115), (76, 115)], fill=white_fur)
    # White muzzle
    d.ellipse([60, 105, 115, 145], fill=white_fur)
    
    # Cute black button nose
    d.ellipse([82, 110, 94, 122], fill=(28, 28, 34, 255))
    
    # Closed sleeping eyes (curved peaceful crescents)
    d.arc([56, 92, 74, 108], start=20, end=160, fill=(45, 30, 20, 255), width=3)
    d.arc([100, 92, 118, 108], start=20, end=160, fill=(45, 30, 20, 255), width=3)
    
    # Big expressive corgi ears (folded back softly)
    # Left ear
    d.polygon([(45, 78), (20, 40), (62, 58)], fill=body_col, outline=shadow_col)
    d.polygon([(43, 72), (25, 46), (56, 60)], fill=(245, 175, 170, 255)) # Pink inner ear
    # Right ear
    d.polygon([(115, 68), (145, 32), (138, 72)], fill=body_col, outline=shadow_col)
    d.polygon([(118, 66), (140, 38), (134, 69)], fill=(245, 175, 170, 255))
    
    # Folded front paws
    d.ellipse([65, 135, 95, 158], fill=white_fur, outline=shadow_col, width=2)
    d.ellipse([98, 135, 128, 158], fill=white_fur, outline=shadow_col, width=2)
    # Toe lines
    d.line([(75, 146), (75, 156)], fill=(180, 160, 150, 255), width=2)
    d.line([(85, 146), (85, 156)], fill=(180, 160, 150, 255), width=2)
    d.line([(108, 146), (108, 156)], fill=(180, 160, 150, 255), width=2)
    d.line([(118, 146), (118, 156)], fill=(180, 160, 150, 255), width=2)
    
    im_sleep.save(os.path.join(sprites_dir, "spr_corgi_sleep.png"))
    print("Created spr_corgi_sleep.png")
    
    # Awake Corgi (Perky ears, big sparkly eyes, happy open mouth with pink tongue!)
    im_awake = Image.new("RGBA", (256, 180), (0, 0, 0, 0))
    d2 = ImageDraw.Draw(im_awake)
    
    d2.ellipse([30, 130, 226, 172], fill=(12, 14, 20, 130))
    # Body
    d2.ellipse([50, 65, 210, 155], fill=body_col, outline=shadow_col, width=3)
    d2.ellipse([180, 78, 230, 128], fill=body_col)
    # Tail wagging up
    d2.ellipse([216, 75, 240, 98], fill=white_fur)
    d2.ellipse([70, 100, 170, 155], fill=white_fur)
    
    # Head held higher
    d2.ellipse([45, 50, 142, 142], fill=body_col, outline=shadow_col, width=3)
    d2.polygon([(86, 52), (98, 52), (104, 98), (80, 98)], fill=white_fur)
    d2.ellipse([64, 90, 122, 138], fill=white_fur)
    
    # Perky big erect corgi ears!
    # Left ear erect
    d2.polygon([(52, 65), (32, 12), (78, 48)], fill=body_col, outline=shadow_col, width=2)
    d2.polygon([(52, 60), (37, 20), (72, 48)], fill=(248, 178, 174, 255))
    # Right ear erect
    d2.polygon([(110, 48), (145, 12), (130, 68)], fill=body_col, outline=shadow_col, width=2)
    d2.polygon([(114, 48), (140, 20), (128, 64)], fill=(248, 178, 174, 255))
    
    # Big sparkling black eyes
    d2.ellipse([60, 74, 78, 92], fill=(24, 20, 26, 255))
    d2.ellipse([64, 76, 70, 82], fill=(255, 255, 255, 255)) # Glint
    d2.ellipse([72, 84, 75, 87], fill=(255, 255, 255, 200))
    
    d2.ellipse([106, 74, 124, 92], fill=(24, 20, 26, 255))
    d2.ellipse([110, 76, 116, 82], fill=(255, 255, 255, 255)) # Glint
    d2.ellipse([118, 84, 121, 87], fill=(255, 255, 255, 200))
    
    # Nose
    d2.ellipse([86, 96, 98, 108], fill=(28, 28, 34, 255))
    
    # Open happy smiling mouth & tongue!
    d2.ellipse([82, 108, 102, 128], fill=(160, 40, 50, 255))
    d2.ellipse([84, 114, 100, 134], fill=(255, 120, 140, 255)) # Tongue out!
    
    # Front paws
    d2.ellipse([68, 132, 98, 156], fill=white_fur, outline=shadow_col, width=2)
    d2.ellipse([102, 132, 132, 156], fill=white_fur, outline=shadow_col, width=2)
    
    im_awake.save(os.path.join(sprites_dir, "spr_corgi_awake.png"))
    print("Created spr_corgi_awake.png")

# 27. Cyber Robot Assistant (Sleep & Awake) (256 x 180)
def make_robo_pet():
    # Robo Sleep (Standby Low-Power Mode)
    im_sleep = Image.new("RGBA", (256, 180), (0, 0, 0, 0))
    d = ImageDraw.Draw(im_sleep)
    
    # Shadow
    d.ellipse([30, 132, 226, 172], fill=(8, 12, 18, 140))
    
    # Chassis colors (Sleek white / dark graphite / neon teal)
    armor_white = (225, 232, 242, 255)
    armor_dark = (42, 48, 62, 255)
    armor_stroke = (28, 32, 42, 255)
    neon_cyan = (0, 229, 255, 255)
    dim_cyan = (0, 120, 140, 180)
    
    # Body shell
    create_rounded_rect(d, (60, 65, 205, 148), 24, fill=armor_white, outline=armor_stroke, width=3)
    # Dark belly plate
    create_rounded_rect(d, (80, 95, 185, 142), 14, fill=armor_dark, outline=armor_stroke, width=2)
    
    # Mechanical rear thruster / tail antenna
    d.line([(200, 90), (228, 70)], fill=armor_dark, width=5)
    d.ellipse([224, 64, 236, 76], fill=dim_cyan)
    
    # Robot Head unit
    create_rounded_rect(d, (42, 60, 138, 142), 20, fill=armor_white, outline=armor_stroke, width=3)
    
    # Dark glass visor screen
    create_rounded_rect(d, (50, 78, 130, 120), 12, fill=(16, 20, 28, 255), outline=armor_stroke, width=2)
    # Standby visor status (dim cyan pulsing dot / sleep line)
    d.line([(70, 99), (110, 99)], fill=dim_cyan, width=3)
    
    # Antennae
    d.line([(58, 60), (45, 34)], fill=armor_dark, width=4)
    d.ellipse([41, 28, 51, 38], fill=dim_cyan)
    d.line([(118, 60), (135, 34)], fill=armor_dark, width=4)
    d.ellipse([131, 28, 141, 38], fill=dim_cyan)
    
    # Metallic articulated paws
    create_rounded_rect(d, (65, 134, 96, 156), 6, fill=armor_dark, outline=armor_stroke, width=2)
    create_rounded_rect(d, (102, 134, 133, 156), 6, fill=armor_dark, outline=armor_stroke, width=2)
    # Paw status LEDs
    d.ellipse([76, 142, 84, 148], fill=dim_cyan)
    d.ellipse([113, 142, 121, 148], fill=dim_cyan)
    
    im_sleep.save(os.path.join(sprites_dir, "spr_robo_sleep.png"))
    print("Created spr_robo_sleep.png")
    
    # Robo Awake (Active Online Mode: glowing neon eyes, active antennae, energized circuits)
    im_awake = Image.new("RGBA", (256, 180), (0, 0, 0, 0))
    d2 = ImageDraw.Draw(im_awake)
    
    d2.ellipse([30, 132, 226, 172], fill=(8, 12, 18, 140))
    # Body
    create_rounded_rect(d2, (60, 60, 205, 148), 24, fill=armor_white, outline=armor_stroke, width=3)
    create_rounded_rect(d2, (80, 90, 185, 142), 14, fill=armor_dark, outline=armor_stroke, width=2)
    
    # Active glowing power core on chest
    d2.ellipse([124, 106, 142, 124], fill=neon_cyan)
    d2.ellipse([128, 110, 138, 120], fill=(255, 255, 255, 255))
    
    # Tail antenna with glowing neon orb
    d2.line([(200, 85), (232, 58)], fill=armor_dark, width=5)
    d2.ellipse([226, 50, 242, 66], fill=neon_cyan)
    d2.ellipse([230, 54, 238, 62], fill=(255, 255, 255, 255))
    
    # Robot Head
    create_rounded_rect(d2, (40, 52, 140, 140), 20, fill=armor_white, outline=armor_stroke, width=3)
    create_rounded_rect(d2, (48, 70, 132, 118), 12, fill=(10, 14, 22, 255), outline=armor_stroke, width=2)
    
    # Bright glowing digital expressive LED eyes (happy curved wedges / arches: ^_^)
    # Left eye arch
    d2.arc([58, 80, 84, 104], start=180, end=360, fill=neon_cyan, width=4)
    # Right eye arch
    d2.arc([96, 80, 122, 104], start=180, end=360, fill=neon_cyan, width=4)
    
    # Antennae glowing bright
    d2.line([(55, 52), (40, 24)], fill=armor_dark, width=4)
    d2.ellipse([34, 16, 48, 30], fill=neon_cyan)
    d2.ellipse([38, 20, 44, 26], fill=(255, 255, 255, 255))
    
    d2.line([(122, 52), (142, 24)], fill=armor_dark, width=4)
    d2.ellipse([136, 16, 150, 30], fill=neon_cyan)
    d2.ellipse([140, 20, 146, 26], fill=(255, 255, 255, 255))
    
    # Paws energized
    create_rounded_rect(d2, (65, 134, 96, 156), 6, fill=armor_dark, outline=armor_stroke, width=2)
    create_rounded_rect(d2, (102, 134, 133, 156), 6, fill=armor_dark, outline=armor_stroke, width=2)
    d2.ellipse([74, 140, 86, 150], fill=neon_cyan)
    d2.ellipse([111, 140, 123, 150], fill=neon_cyan)
    
    im_awake.save(os.path.join(sprites_dir, "spr_robo_awake.png"))
    print("Created spr_robo_awake.png")

# 28. Streak Flame & Crown Badge (128 x 128)
def make_streak_sprites():
    # Streak Flame Icon (Vibrant Fire)
    im_flame = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    d = ImageDraw.Draw(im_flame)
    
    # Outer orange glow
    d.ellipse([24, 30, 104, 116], fill=(255, 100, 0, 120))
    
    # Stylized Flame Silhouette (Outer Orange/Red)
    flame_pts = [
        (64, 10), (78, 36), (96, 48), (88, 70), (102, 85), 
        (96, 112), (64, 120), (32, 112), (26, 85), (40, 70),
        (32, 48), (50, 36)
    ]
    d.polygon(flame_pts, fill=(255, 75, 20, 255), outline=(255, 140, 0, 255))
    
    # Mid-layer Bright Gold/Yellow
    mid_flame = [
        (64, 26), (75, 48), (86, 68), (82, 94), (64, 108), 
        (46, 94), (42, 68), (53, 48)
    ]
    d.polygon(mid_flame, fill=(255, 185, 0, 255))
    
    # Core Pure White/Cyan Hot Spark
    core_flame = [
        (64, 46), (72, 66), (70, 92), (64, 100), (58, 92), (56, 66)
    ]
    d.polygon(core_flame, fill=(255, 255, 220, 255))
    
    im_flame.save(os.path.join(sprites_dir, "spr_streak_flame.png"))
    print("Created spr_streak_flame.png")
    
    # Crown Badge for Day 7 Streak Master
    im_crown = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    dc = ImageDraw.Draw(im_crown)
    
    # Outer glow
    dc.ellipse([14, 26, 114, 112], fill=(255, 215, 0, 90))
    
    # Royal Golden Crown Silhouette
    crown_pts = [
        (20, 48), (38, 86), (64, 32), (90, 86), (108, 48),
        (102, 104), (26, 104)
    ]
    dc.polygon(crown_pts, fill=(255, 195, 30, 255), outline=(255, 240, 120, 255))
    
    # Bottom Crown Band
    create_rounded_rect(dc, (22, 96, 106, 114), 4, fill=(230, 160, 15, 255), outline=(255, 230, 80, 255), width=2)
    
    # Crown Jewels / Rubies / Sapphires
    dc.ellipse([60, 46, 68, 54], fill=(255, 255, 255, 255)) # Center peak diamond
    dc.ellipse([16, 44, 24, 52], fill=(0, 229, 255, 255)) # Left sapphire
    dc.ellipse([104, 44, 112, 52], fill=(0, 229, 255, 255)) # Right sapphire
    # Band rubies
    for bx in [36, 64, 92]:
        dc.ellipse([bx - 4, 102, bx + 4, 110], fill=(255, 50, 80, 255))
        
    im_crown.save(os.path.join(sprites_dir, "spr_crown_badge.png"))
    print("Created spr_crown_badge.png")

make_desk()
make_monitor_frame()
make_monitor_screen()
make_keyboard()
make_mouse()
make_mouse_glow()
make_coffee_mug()
make_energy_can()
make_cat()
make_cat_awake()
make_heart()
make_card_bg()
make_button_sprites()
make_steam_particle()
make_bubble_particle()
make_stickers()
make_desk_lamp()
make_lamp_cone()
make_cat_stretch()
make_cat_accessories()
make_minigame_sprites()
make_keyboard_retro()
make_keyboard_neon()
make_lofi_player()
make_wallpapers()
make_quest_icon()
make_corgi_pet()
make_robo_pet()
make_streak_sprites()
print("All sprites successfully generated!")
