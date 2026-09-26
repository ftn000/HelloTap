import os
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

# 2. Dual Monitor Frame & Stand (900 x 480)
def make_monitor_frame():
    im = Image.new("RGBA", (900, 480), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Monitor Stand Base
    create_rounded_rect(d, (370, 430, 530, 470), 8, fill=(35, 38, 48, 255), outline=(60, 65, 80, 255), width=2)
    # Stand column
    d.rectangle([435, 370, 465, 435], fill=(30, 32, 40, 255))
    
    # Left Monitor (Main IDE) - (50, 40, 560, 375)
    create_rounded_rect(d, (50, 40, 560, 375), 12, fill=(20, 22, 28, 255), outline=(50, 55, 70, 255), width=3)
    # Screen area
    d.rectangle([62, 52, 548, 363], fill=(13, 15, 20, 255))
    # Top IDE Window Bar
    d.rectangle([62, 52, 548, 76], fill=(22, 25, 33, 255))
    d.ellipse([70, 60, 78, 68], fill=(255, 95, 86, 255))   # Red close
    d.ellipse([84, 60, 92, 68], fill=(255, 189, 46, 255))  # Yellow min
    d.ellipse([98, 60, 106, 68], fill=(39, 201, 63, 255))  # Green max

    # Right Monitor (Game / Preview) - (575, 70, 860, 375)
    create_rounded_rect(d, (575, 70, 860, 375), 12, fill=(20, 22, 28, 255), outline=(50, 55, 70, 255), width=3)
    d.rectangle([585, 82, 850, 363], fill=(15, 18, 24, 255))
    # Top Bar
    d.rectangle([585, 82, 850, 104], fill=(24, 28, 38, 255))
    d.ellipse([592, 89, 598, 95], fill=(0, 229, 255, 255))
    d.ellipse([603, 89, 609, 95], fill=(0, 255, 136, 255))
    
    im.save(os.path.join(sprites_dir, "spr_monitor_frame.png"))
    print("Created spr_monitor_frame.png")

# 3. Monitor Screen Content (IDE Code Lines + Game View)
def make_monitor_screen():
    im = Image.new("RGBA", (800, 320), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Background
    d.rectangle([0, 0, 480, 320], fill=(13, 16, 23, 255))
    # Code Lines (Left IDE Screen)
    code_colors = [
        (255, 123, 114, 255), # Red keyword
        (121, 192, 255, 255), # Blue type
        (126, 231, 135, 255), # Green string
        (227, 179, 65, 255),  # Yellow class
        (139, 148, 158, 255)  # Grey comment
    ]
    # Draw simulated code lines
    code_shapes = [
        [(20, 20, 80, 26, 0), (90, 20, 210, 26, 3), (220, 20, 280, 26, 1)],
        [(40, 35, 120, 41, 1), (130, 35, 260, 41, 2)],
        [(40, 50, 180, 56, 4)],
        [(40, 65, 90, 71, 0), (100, 65, 190, 71, 1), (200, 65, 250, 71, 3)],
        [(60, 80, 140, 86, 0), (150, 80, 290, 86, 2)],
        [(60, 95, 220, 101, 3), (230, 95, 340, 101, 1)],
        [(60, 110, 110, 116, 0), (120, 110, 240, 116, 2)],
        [(40, 125, 60, 131, 1)],
        [(40, 140, 160, 146, 4)],
        [(40, 155, 100, 161, 0), (110, 155, 280, 161, 3)],
        [(60, 170, 180, 176, 1), (190, 170, 320, 176, 2)],
        [(60, 185, 240, 191, 2)],
        [(40, 200, 70, 206, 1)],
    ]
    for line in code_shapes:
        for x0, y0, x1, y1, c_idx in line:
            create_rounded_rect(d, (x0, y0, x1, y1), 3, fill=code_colors[c_idx])
            
    # Right Game Preview Screen (490, 0, 800, 320)
    d.rectangle([490, 0, 800, 320], fill=(20, 24, 34, 255))
    # Ground in preview
    d.rectangle([490, 240, 800, 320], fill=(35, 45, 65, 255))
    # Pixel Hero in preview
    create_rounded_rect(d, (620, 180, 670, 240), 6, fill=(0, 229, 255, 255))
    # Sword / Staff
    d.rectangle([670, 170, 676, 230], fill=(255, 189, 46, 255))
    # Coin in air
    d.ellipse([700, 130, 725, 155], fill=(255, 215, 0, 255), outline=(200, 160, 0, 255), width=2)
    # Neon Grid lines
    for gx in range(510, 790, 40):
        d.line([(gx, 240), (gx - 30, 320)], fill=(0, 229, 255, 40), width=1)
        
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

# 5. Gaming Mouse (100 x 160)
def make_mouse():
    im = Image.new("RGBA", (100, 160), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    # Body
    create_rounded_rect(d, (10, 15, 90, 145), 28, fill=(28, 30, 38, 255), outline=(55, 60, 75, 255), width=2)
    # RGB Side strips
    d.arc([14, 25, 86, 135], start=45, end=135, fill=(0, 229, 255, 255), width=3)
    d.arc([14, 25, 86, 135], start=225, end=315, fill=(255, 0, 128, 255), width=3)
    # Scroll wheel
    create_rounded_rect(d, (44, 28, 56, 58), 4, fill=(0, 229, 255, 255), outline=(255, 255, 255, 200))
    # Split seam
    d.line([(50, 15), (50, 28)], fill=(55, 60, 75, 255), width=2)
    d.line([(50, 58), (50, 95)], fill=(55, 60, 75, 255), width=2)
    im.save(os.path.join(sprites_dir, "spr_mouse.png"))
    print("Created spr_mouse.png")

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

make_desk()
make_monitor_frame()
make_monitor_screen()
make_keyboard()
make_mouse()
make_coffee_mug()
make_energy_can()
make_cat()
make_card_bg()
make_button_sprites()
print("All sprites successfully generated!")
