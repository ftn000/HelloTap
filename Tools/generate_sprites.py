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
print("All sprites successfully generated!")
