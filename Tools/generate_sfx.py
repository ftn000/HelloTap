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
        click_freq = 4200.0
        click_decay = 240.0
        click_amp = 1.0
        thud_freq = 240.0
        thud_decay = 65.0
        spring_ping_freq = 6800.0
        spring_ping_amp = 0.16
        bottom_out_delay = 0.003
    elif switch_type == "brown": # Cherry MX Brown (Tactile bump + deep clack)
        click_freq = 2900.0
        click_decay = 180.0
        click_amp = 0.82
        thud_freq = 210.0
        thud_decay = 55.0
        spring_ping_freq = 5200.0
        spring_ping_amp = 0.09
        bottom_out_delay = 0.004
    elif switch_type == "spacebar": # Heavy Spacebar stabilizer thud + bar clack
        click_freq = 2200.0
        click_decay = 160.0
        click_amp = 0.72
        thud_freq = 160.0
        thud_decay = 40.0
        spring_ping_freq = 4100.0
        spring_ping_amp = 0.13
        bottom_out_delay = 0.002
    else: # Snappy Blue secondary clack
        click_freq = 3800.0
        click_decay = 220.0
        click_amp = 0.92
        thud_freq = 260.0
        thud_decay = 70.0
        spring_ping_freq = 6200.0
        spring_ping_amp = 0.12
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
                    0.25 * (random.random() * 2.0 - 1.0)) * env_thud * 0.68
                    
        s = click + spring + thud
        samples.append(s)
        
    return samples

def make_crit_click():
    duration = 0.24
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        # Crisp mechanical switch click transient
        env_click = math.exp(-t * 180.0)
        click = (math.sin(2.0 * math.pi * 4400.0 * t) + 0.5 * (random.random() * 2.0 - 1.0)) * env_click * 0.85
        
        # Bright crystalline crit harmonic chime
        env_bell = math.exp(-t * 16.0)
        bell = (0.55 * math.sin(2.0 * math.pi * 1760.0 * t) + 
                0.35 * math.sin(2.0 * math.pi * 3520.0 * t) + 
                0.20 * math.sin(2.0 * math.pi * 5280.0 * t)) * env_bell
        
        # Low punch
        thud = math.sin(2.0 * math.pi * 180.0 * t) * math.exp(-t * 50.0) * 0.4
        
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
print("All sound effects generated successfully!")
