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

def make_key_click(base_freq=2500, thud_freq=350, duration=0.065, click_strength=0.9):
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        # Fast click impulse
        env_click = math.exp(-t * 120.0)
        click_osc = math.sin(2.0 * math.pi * base_freq * t) + 0.4 * (random.random() * 2.0 - 1.0)
        
        # Bottom-out thud
        env_thud = math.exp(-t * 60.0)
        thud_osc = math.sin(2.0 * math.pi * thud_freq * t)
        
        s = (click_osc * env_click * click_strength) + (thud_osc * env_thud * 0.35)
        samples.append(s)
    return samples

def make_crit_click():
    duration = 0.22
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        # Sharp click transient
        env_click = math.exp(-t * 150.0)
        click = math.sin(2.0 * math.pi * 3200 * t) + 0.6 * (random.random() * 2.0 - 1.0)
        
        # Bright magic chime ring
        env_bell = math.exp(-t * 18.0)
        bell = 0.6 * math.sin(2.0 * math.pi * 1760.0 * t) + 0.3 * math.sin(2.0 * math.pi * 3520.0 * t) + 0.2 * math.sin(2.0 * math.pi * 5280.0 * t)
        
        s = (click * env_click * 0.7) + (bell * env_bell * 0.6)
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

save_wav(os.path.join(out_dir, "click_key1.wav"), make_key_click(base_freq=2500, thud_freq=380, duration=0.06))
save_wav(os.path.join(out_dir, "click_key2.wav"), make_key_click(base_freq=2800, thud_freq=340, duration=0.055))
save_wav(os.path.join(out_dir, "click_key3.wav"), make_key_click(base_freq=2300, thud_freq=410, duration=0.065))
save_wav(os.path.join(out_dir, "click_key4.wav"), make_key_click(base_freq=1800, thud_freq=220, duration=0.075, click_strength=0.7))
save_wav(os.path.join(out_dir, "click_crit.wav"), make_crit_click())
save_wav(os.path.join(out_dir, "upgrade_buy.wav"), make_upgrade_chime())
save_wav(os.path.join(out_dir, "project_release.wav"), make_cash_release())
save_wav(os.path.join(out_dir, "boost_activate.wav"), make_boost_sound())
print("All sound effects generated successfully!")
