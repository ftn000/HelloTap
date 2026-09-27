import wave
import struct
import math
import random
import os

SAMPLE_RATE = 44100

def save_wav(filename, samples):
    # Normalize and clip
    max_val = max(abs(s) for s in samples) if samples else 1.0
    if max_val > 0.98:
        scale = 0.98 / max_val
    else:
        scale = 1.0
    
    with wave.open(filename, 'w') as wf:
        wf.setnchannels(1)        # mono
        wf.setsampwidth(2)        # 16-bit
        wf.setframerate(SAMPLE_RATE)
        raw_data = bytearray()
        for s in samples:
            val = int(max(-1.0, min(1.0, s * scale)) * 32767.0)
            raw_data.extend(struct.pack('<h', val))
        wf.writeframes(raw_data)
    print(f"Generated {filename} ({len(samples)/SAMPLE_RATE:.3f}s)")

def make_mech_switch_click(switch_type="blue", duration=0.075):
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    
    if switch_type == "blue": # Cherry MX Blue (Crisp tactile click + solid bottom out)
        click_freq = 4100.0
        click_decay = 220.0
        click_amp = 1.20
        thud_freq = 230.0
        thud_decay = 60.0
        spring_ping_freq = 6600.0
        spring_ping_amp = 0.18
        bottom_out_delay = 0.003
    elif switch_type == "brown": # Cherry MX Brown (Tactile bump + deep clack)
        click_freq = 2800.0
        click_decay = 170.0
        click_amp = 1.05
        thud_freq = 200.0
        thud_decay = 50.0
        spring_ping_freq = 5000.0
        spring_ping_amp = 0.11
        bottom_out_delay = 0.004
    elif switch_type == "spacebar": # Heavy Spacebar stabilizer thud + bar clack
        click_freq = 2100.0
        click_decay = 150.0
        click_amp = 0.95
        thud_freq = 150.0
        thud_decay = 38.0
        spring_ping_freq = 4000.0
        spring_ping_amp = 0.14
        bottom_out_delay = 0.002
    else: # Snappy Blue secondary clack
        click_freq = 3700.0
        click_decay = 210.0
        click_amp = 1.12
        thud_freq = 250.0
        thud_decay = 65.0
        spring_ping_freq = 6000.0
        spring_ping_amp = 0.14
        bottom_out_delay = 0.003

    for i in range(total_samples):
        t = i / SAMPLE_RATE
        
        # 1. Tactile click transient (crisp leaf snap)
        env_click = math.exp(-t * click_decay)
        noise = (random.random() * 2.0 - 1.0)
        click = (math.sin(2.0 * math.pi * click_freq * t) + 0.45 * noise) * env_click * click_amp
        
        # 2. Metallic spring ping (subtle high-frequency resonance)
        env_spring = math.exp(-t * 90.0)
        spring = math.sin(2.0 * math.pi * spring_ping_freq * t) * env_spring * spring_ping_amp
        
        # 3. Bottoming-out thud (stem hits the plate slightly after the click)
        thud = 0.0
        if t >= bottom_out_delay:
            t_thud = t - bottom_out_delay
            env_thud = math.exp(-t_thud * thud_decay)
            thud = (math.sin(2.0 * math.pi * thud_freq * t_thud) + 
                    0.35 * math.sin(4.0 * math.pi * thud_freq * t_thud) + 
                    0.25 * (random.random() * 2.0 - 1.0)) * env_thud * 0.85
                    
        s = click + spring + thud
        samples.append(s)
        
    return samples

def make_crit_click():
    duration = 0.20
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        # Crisp mechanical switch click transient
        env_click = math.exp(-t * 190.0)
        click = (math.sin(2.0 * math.pi * 4200.0 * t) + 0.45 * (random.random() * 2.0 - 1.0)) * env_click * 1.0
        
        # Soft musical crystal sparkle (delicate, not harsh)
        env_bell = math.exp(-t * 24.0)
        bell = (0.24 * math.sin(2.0 * math.pi * 1760.0 * t) + 
                0.14 * math.sin(2.0 * math.pi * 3520.0 * t) + 
                0.07 * math.sin(2.0 * math.pi * 5280.0 * t)) * env_bell
        
        # Low mechanical thud punch
        thud = math.sin(2.0 * math.pi * 190.0 * t) * math.exp(-t * 55.0) * 0.5
        
        s = click + bell + thud
        samples.append(s)
    return samples

def make_upgrade_chime():
    # 3-tone arpeggio: C5 (523Hz), E5 (659Hz), G5 (784Hz), C6 (1046Hz)
    notes = [523.25, 659.25, 783.99, 1046.5]
    note_dur = 0.07
    total_samples = int(SAMPLE_RATE * (len(notes) * note_dur + 0.25))
    samples = [0.0] * total_samples
    
    for idx, freq in enumerate(notes):
        start_sample = int(idx * note_dur * SAMPLE_RATE)
        for i in range(int(0.3 * SAMPLE_RATE)):
            sample_idx = start_sample + i
            if sample_idx >= total_samples:
                break
            t = i / SAMPLE_RATE
            env = math.exp(-t * 14.0)
            val = math.sin(2.0 * math.pi * freq * t) + 0.3 * math.sin(4.0 * math.pi * freq * t)
            samples[sample_idx] += val * env * 0.35
    return samples

def make_cash_release():
    # Cash register ka-ching: 2 bright metallic dings + coin shower
    duration = 0.65
    total_samples = int(SAMPLE_RATE * duration)
    samples = [0.0] * total_samples
    
    # Ding 1 at t=0
    f1 = 2093.0 # C7
    for i in range(int(SAMPLE_RATE * 0.45)):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 12.0)
        samples[i] += (math.sin(2*math.pi*f1*t) + 0.4*math.sin(4*math.pi*f1*t)) * env * 0.45
        
    # Ding 2 at t=0.08
    f2 = 2793.8 # F7
    start2 = int(SAMPLE_RATE * 0.08)
    for i in range(int(SAMPLE_RATE * 0.5)):
        sample_idx = start2 + i
        if sample_idx >= total_samples: break
        t = i / SAMPLE_RATE
        env = math.exp(-t * 10.0)
        samples[sample_idx] += (math.sin(2*math.pi*f2*t) + 0.4*math.sin(4*math.pi*f2*t)) * env * 0.55

    # Coins cascade (little pings at random timings)
    random.seed(42)
    for _ in range(8):
        offset = int(SAMPLE_RATE * (0.12 + random.random() * 0.35))
        cfreq = random.choice([3135.9, 3520.0, 3951.0, 4186.0])
        for i in range(int(SAMPLE_RATE * 0.15)):
            idx = offset + i
            if idx >= total_samples: break
            t = i / SAMPLE_RATE
            env = math.exp(-t * 40.0)
            samples[idx] += math.sin(2*math.pi*cfreq*t) * env * 0.2
            
    return samples

def make_boost_sound():
    duration = 0.4
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        # Can pop fizz transient (white noise burst)
        fizz_env = math.exp(-t * 45.0)
        fizz = (random.random() * 2.0 - 1.0) * fizz_env * 0.7
        
        # Rising power synth whoosh (250Hz -> 900Hz)
        cur_freq = 250.0 + (t / duration) * 650.0
        phase = 2.0 * math.pi * (250.0 * t + 0.5 * (650.0 / duration) * t * t)
        whoosh_env = math.sin(math.pi * min(1.0, t / duration))
        whoosh = math.sin(phase) * whoosh_env * 0.5
        
        samples.append(fizz + whoosh)
    return samples

def make_cat_purr():
    duration = 0.45
    total_samples = int(SAMPLE_RATE * duration)
    samples = []

    for i in range(total_samples):
        t = i / SAMPLE_RATE
        if t < 0.05:
            env = t / 0.05
        elif t > duration - 0.12:
            env = (duration - t) / 0.12
        else:
            env = 1.0
        
        # 24Hz purr rumble flutter
        flutter = 0.5 + 0.5 * math.sin(2.0 * math.pi * 24.0 * t)
        
        # Pitch contour: slight friendly chirp rise from 135Hz to 210Hz then settling to 155Hz
        if t < 0.22:
            freq = 135.0 + (t / 0.22) * 75.0
        else:
            freq = 210.0 - ((t - 0.22) / (duration - 0.22)) * 55.0
            
        carrier = (math.sin(2.0 * math.pi * freq * t) + 
                   0.45 * math.sin(4.0 * math.pi * freq * t) + 
                   0.25 * math.sin(6.0 * math.pi * freq * t))
                   
        # Soft breath/fur noise
        breath = (random.random() * 2.0 - 1.0) * 0.12
        
        val = (carrier * flutter + breath) * env * 0.75
        samples.append(val)
    return samples

def make_drink_sip():
    duration = 0.36
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        
        # 1. Slurp / Sip suction (0.0s - 0.14s)
        slurp = 0.0
        if t < 0.14:
            t_slurp = t / 0.14
            slurp_env = math.sin(t_slurp * math.pi) * (0.8 + 0.2 * math.sin(2.0 * math.pi * 32.0 * t))
            # Resonant filtered noise
            noise = (random.random() * 2.0 - 1.0)
            slurp_carrier = (math.sin(2.0 * math.pi * (1800.0 + 800.0 * t_slurp) * t) * 0.6 + noise * 0.4)
            slurp = slurp_carrier * slurp_env * 0.55
            
        # 2. Resonant swallow / gulp (0.10s - 0.30s)
        gulp = 0.0
        if t >= 0.10 and t < 0.32:
            t_gulp = (t - 0.10) / 0.22
            gulp_env = math.sin(t_gulp * math.pi) * math.exp(-t_gulp * 3.5)
            # Pitch drops from 520Hz down to 210Hz
            cur_freq = 520.0 * (1.0 - t_gulp * 0.60)
            gulp_sine = (math.sin(2.0 * math.pi * cur_freq * t) + 
                         0.4 * math.sin(4.0 * math.pi * cur_freq * t) +
                         0.15 * math.sin(6.0 * math.pi * cur_freq * t))
            gulp = gulp_sine * gulp_env * 0.85
            
        # 3. Refreshing fizz / droplet pop (0.24s - 0.36s)
        droplet = 0.0
        if t >= 0.24:
            t_drop = (t - 0.24) / 0.12
            drop_env = math.exp(-t_drop * 18.0)
            drop_freq = 1400.0 - t_drop * 400.0
            droplet = math.sin(2.0 * math.pi * drop_freq * t) * drop_env * 0.28
            
        val = slurp + gulp + droplet
        samples.append(val)
    return samples

def make_mouse_click():
    duration = 0.055
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        env_snap = math.exp(-t * 320.0)
        noise = (random.random() * 2.0 - 1.0)
        snap = (math.sin(2.0 * math.pi * 5200.0 * t) + 0.4 * noise) * env_snap * 1.3
        
        env_body = math.exp(-t * 160.0)
        body = (math.sin(2.0 * math.pi * 850.0 * t) * 0.5 + 
                math.sin(2.0 * math.pi * 340.0 * t) * 0.4) * env_body
                
        tick = 0.0
        if t >= 0.022:
            t_tick = t - 0.022
            env_tick = math.exp(-t_tick * 400.0)
            tick = math.sin(2.0 * math.pi * 4800.0 * t_tick) * env_tick * 0.45
            
        s = snap + body + tick
        samples.append(s)
    return samples

def make_lamp_switch():
    duration = 0.08
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        # Metallic clack & snap
        env = math.exp(-t * 220.0)
        snap = math.sin(2.0 * math.pi * 3200.0 * t) * env * 0.9
        body = math.sin(2.0 * math.pi * 420.0 * t) * math.exp(-t * 120.0) * 0.6
        samples.append(snap + body)
    return samples

def make_bug_squash():
    duration = 0.12
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 80.0)
        noise = (random.random() * 2.0 - 1.0) * 0.4
        f = max(180.0, 1200.0 - t * 8000.0)
        pop = (math.sin(2.0 * math.pi * f * t) + noise) * env
        samples.append(pop * 0.9)
    return samples

def make_crate_collect():
    duration = 0.22
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    # Upward arpeggio (C5 -> E5 -> G5)
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        if t < 0.07:
            f = 523.25
            local_t = t
        elif t < 0.14:
            f = 659.25
            local_t = t - 0.07
        else:
            f = 783.99
            local_t = t - 0.14
        env = math.exp(-local_t * 35.0)
        chime = (math.sin(2.0 * math.pi * f * t) + 0.3 * math.sin(4.0 * math.pi * f * t)) * env
        samples.append(chime * 0.8)
    return samples

def make_build_complete():
    duration = 0.45
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    # Fanfare: C5 (0.09s), G5 (0.09s), C6 (0.27s)
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        if t < 0.10:
            f = 523.25
            env = math.exp(-(t) * 15.0)
        elif t < 0.20:
            f = 659.25
            env = math.exp(-(t - 0.10) * 15.0)
        elif t < 0.30:
            f = 783.99
            env = math.exp(-(t - 0.20) * 15.0)
        else:
            f = 1046.50
            env = math.exp(-(t - 0.30) * 10.0)
        bell = (math.sin(2.0 * math.pi * f * t) + 0.35 * math.sin(4.0 * math.pi * f * t) + 0.15 * math.sin(6.0 * math.pi * f * t)) * env
        samples.append(bell * 0.9)
    return samples

out_dir = r"C:\HelloTap\Assets\Audio\SFX"
os.makedirs(out_dir, exist_ok=True)

save_wav(os.path.join(out_dir, "click_key1.wav"), make_mech_switch_click("blue"))
save_wav(os.path.join(out_dir, "click_key2.wav"), make_mech_switch_click("brown"))
save_wav(os.path.join(out_dir, "click_key3.wav"), make_mech_switch_click("speed"))
save_wav(os.path.join(out_dir, "click_key4.wav"), make_mech_switch_click("spacebar"))
save_wav(os.path.join(out_dir, "click_crit.wav"), make_crit_click())
save_wav(os.path.join(out_dir, "upgrade_buy.wav"), make_upgrade_chime())
save_wav(os.path.join(out_dir, "project_release.wav"), make_cash_release())
save_wav(os.path.join(out_dir, "boost_activate.wav"), make_boost_sound())
save_wav(os.path.join(out_dir, "cat_purr.wav"), make_cat_purr())
save_wav(os.path.join(out_dir, "drink_sip.wav"), make_drink_sip())
save_wav(os.path.join(out_dir, "mouse_click.wav"), make_mouse_click())
save_wav(os.path.join(out_dir, "lamp_switch.wav"), make_lamp_switch())
save_wav(os.path.join(out_dir, "bug_squash.wav"), make_bug_squash())
save_wav(os.path.join(out_dir, "crate_collect.wav"), make_crate_collect())
save_wav(os.path.join(out_dir, "build_complete.wav"), make_build_complete())
print("All sound effects generated successfully!")
