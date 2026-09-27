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
    return make_mouse_switch("classic")

def make_mouse_switch(switch_type="classic"):
    if switch_type == "classic":
        # Classic Omron Mechanical Microswitch (Crisp, sharp snap, clean body, distinct release tick)
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
            samples.append(snap + body + tick)
        return samples

    elif switch_type == "optical":
        # Optical Gaming Switch (Ultra snappy, instant metallic crisp snap, fast decay)
        duration = 0.048
        total_samples = int(SAMPLE_RATE * duration)
        samples = []
        for i in range(total_samples):
            t = i / SAMPLE_RATE
            env_snap = math.exp(-t * 380.0)
            noise = (random.random() * 2.0 - 1.0)
            snap = (math.sin(2.0 * math.pi * 6800.0 * t) * 0.8 + 
                    math.sin(2.0 * math.pi * 4200.0 * t) * 0.5 + 
                    0.5 * noise) * env_snap * 1.4
            # Metallic spring ring
            env_ring = math.exp(-t * 190.0)
            ring = math.sin(2.0 * math.pi * 2100.0 * t) * env_ring * 0.35
            # Crisp reset click
            tick = 0.0
            if t >= 0.018:
                t_tick = t - 0.018
                tick = math.sin(2.0 * math.pi * 5900.0 * t_tick) * math.exp(-t_tick * 450.0) * 0.5
            samples.append(snap + ring + tick)
        return samples

    else: # "silent"
        # Mute Dampened Office Switch (Subtle soft rubberized thud, no high-pitch click)
        duration = 0.065
        total_samples = int(SAMPLE_RATE * duration)
        samples = []
        for i in range(total_samples):
            t = i / SAMPLE_RATE
            env_thud = math.exp(-t * 140.0)
            noise = (random.random() * 2.0 - 1.0) * 0.15
            thud = (math.sin(2.0 * math.pi * 380.0 * t) * 0.8 + 
                    math.sin(2.0 * math.pi * 210.0 * t) * 0.6 + noise) * env_thud * 0.75
            samples.append(thud)
        return samples

def make_corgi_bark():
    # Cute, friendly double corgi woof ("Arf-arf!")
    duration = 0.36
    total_samples = int(SAMPLE_RATE * duration)
    samples = [0.0] * total_samples
    
    # Two rapid cheerful barks at 0.0s and 0.16s
    barks = [(0.0, 0.12, 420.0), (0.16, 0.14, 460.0)]
    for start_t, bark_len, base_pitch in barks:
        start_idx = int(start_t * SAMPLE_RATE)
        b_samples = int(bark_len * SAMPLE_RATE)
        for i in range(b_samples):
            if start_idx + i >= total_samples: break
            t = i / SAMPLE_RATE
            t_norm = t / bark_len
            
            # Formant envelope (rapid attack, quick decay)
            env = math.sin(t_norm * math.pi) * math.exp(-t_norm * 2.8) * 0.85
            # Pitch drop
            pitch = base_pitch * (1.15 - t_norm * 0.35)
            # Canine formant harmonics + slight rasp
            noise = (random.random() * 2.0 - 1.0) * 0.18
            vocal = (math.sin(2.0 * math.pi * pitch * t) + 
                     0.55 * math.sin(4.0 * math.pi * pitch * t) + 
                     0.30 * math.sin(6.0 * math.pi * pitch * t) + noise)
            samples[start_idx + i] += vocal * env
            
    return samples

def make_robo_beep():
    # Cheerful electronic sci-fi companion chirp (triple rising beep arpeggio)
    duration = 0.38
    total_samples = int(SAMPLE_RATE * duration)
    samples = [0.0] * total_samples
    
    beeps = [(0.0, 0.08, 880.0), (0.09, 0.09, 1320.0), (0.19, 0.16, 1760.0)]
    for start_t, b_dur, freq in beeps:
        start_idx = int(start_t * SAMPLE_RATE)
        b_len = int(b_dur * SAMPLE_RATE)
        for i in range(b_len):
            if start_idx + i >= total_samples: break
            t = i / SAMPLE_RATE
            env = math.sin((t / b_dur) * math.pi) * math.exp(-(t / b_dur) * 1.5) * 0.42
            # Retro digital synth square/sine hybrid
            sig = (math.sin(2.0 * math.pi * freq * t) * 0.7 + 
                   (1.0 if math.sin(2.0 * math.pi * freq * t) > 0 else -1.0) * 0.25)
            samples[start_idx + i] += sig * env
            
    return samples

def make_streak_claim():
    # Triumphant streak fanfare: bright fanfare chords + sparkling coin jingle
    duration = 0.95
    total_samples = int(SAMPLE_RATE * duration)
    samples = [0.0] * total_samples
    
    # Golden fanfare chords: F4, A4, C5, F5, A5
    fanfare = [
        (0.00, 349.23, 0.22),
        (0.10, 440.00, 0.22),
        (0.20, 523.25, 0.25),
        (0.32, 698.46, 0.35),
        (0.44, 880.00, 0.50),
        (0.44, 1046.50, 0.45)
    ]
    for start_t, freq, note_dur in fanfare:
        start_idx = int(start_t * SAMPLE_RATE)
        n_len = min(int(note_dur * SAMPLE_RATE), total_samples - start_idx)
        for i in range(n_len):
            t = i / SAMPLE_RATE
            env = math.exp(-t * 5.0) * 0.35
            sig = (math.sin(2.0 * math.pi * freq * t) + 
                   0.4 * math.sin(4.0 * math.pi * freq * t) + 
                   0.15 * math.sin(6.0 * math.pi * freq * t))
            samples[start_idx + i] += sig * env
            
    # Sparkle shimmer layer
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        if t >= 0.44:
            t_shimmer = t - 0.44
            shimmer = math.sin(2.0 * math.pi * (2400.0 + 400.0 * math.sin(2.0 * math.pi * 12.0 * t)) * t)
            samples[i] += shimmer * math.exp(-t_shimmer * 4.5) * 0.15
            
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

def make_cassette_click():
    duration = 0.075
    total_samples = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 190.0)
        click = math.sin(2.0 * math.pi * 2400.0 * t) * env * 0.9
        thud = math.sin(2.0 * math.pi * 280.0 * t) * math.exp(-t * 90.0) * 0.7
        samples.append(click + thud)
    return samples

def make_lofi_track():
    duration = 4.0 # 1 seamless loop
    total_samples = int(SAMPLE_RATE * duration)
    samples = [0.0] * total_samples
    
    # 4 jazz chords (Fmaj7, Em7, Dm7, Cmaj7) - 1.0s each
    chord_freqs = [
        [174.61, 220.00, 261.63, 329.63], # Fmaj7
        [164.81, 196.00, 246.94, 293.66], # Em7
        [146.83, 174.61, 220.00, 261.63], # Dm7
        [130.81, 164.81, 196.00, 246.94], # Cmaj7
    ]
    
    # 1. Rhodes electric piano chords
    for c_idx, freqs in enumerate(chord_freqs):
        start_t = c_idx * 1.0
        start_idx = int(start_t * SAMPLE_RATE)
        end_idx = int((start_t + 1.0) * SAMPLE_RATE)
        for i in range(start_idx, min(end_idx, total_samples)):
            lt = (i - start_idx) / SAMPLE_RATE
            env = math.exp(-lt * 2.8) * 0.22
            vib = math.sin(2.0 * math.pi * 4.5 * lt) * 0.02
            val = 0.0
            for f in freqs:
                f_vib = f * (1.0 + vib)
                val += math.sin(2.0 * math.pi * f_vib * lt) * 0.25
                val += math.sin(2.0 * math.pi * (f_vib * 2.0) * lt) * 0.08
            samples[i] += val * env

    # 2. Lo-fi drums (Kick at 0.0s, 2.0s; Snare at 1.0s, 3.0s; Hi-hats every 0.25s)
    # Kicks
    for kt in [0.0, 2.0]:
        k_start = int(kt * SAMPLE_RATE)
        k_len = int(0.20 * SAMPLE_RATE)
        for i in range(k_len):
            idx = (k_start + i) % total_samples
            t = i / SAMPLE_RATE
            f = max(45.0, 160.0 * math.exp(-t * 30.0))
            env = math.exp(-t * 18.0)
            samples[idx] += math.sin(2.0 * math.pi * f * t) * env * 0.35

    # Snares / Rimshots
    for st in [1.0, 3.0]:
        s_start = int(st * SAMPLE_RATE)
        s_len = int(0.18 * SAMPLE_RATE)
        for i in range(s_len):
            idx = (s_start + i) % total_samples
            t = i / SAMPLE_RATE
            env = math.exp(-t * 22.0)
            noise = (random.random() * 2.0 - 1.0) * 0.18 * env
            tone = math.sin(2.0 * math.pi * 220.0 * t) * 0.12 * math.exp(-t * 35.0)
            samples[idx] += noise + tone

    # Vinyl crackle background
    for i in range(total_samples):
        if random.random() < 0.003:
            samples[i] += (random.random() * 2.0 - 1.0) * 0.06
        samples[i] += (random.random() * 2.0 - 1.0) * 0.008

    return samples

def make_synthwave_track():
    duration = 4.0 # 120 BPM: 2 bars of 4/4
    total_samples = int(SAMPLE_RATE * duration)
    samples = [0.0] * total_samples

    # 120 BPM: 1 beat = 0.5s. 16th note = 0.125s.
    # 1. 16th note rolling analog synth bass
    bass_notes = [55.0, 55.0, 55.0, 55.0, 43.65, 43.65, 43.65, 43.65, 48.99, 48.99, 48.99, 48.99, 41.20, 41.20, 41.20, 41.20] # A1, F1, G1, E1
    for step in range(32): # 32 16th notes in 4.0s
        note_idx = (step // 2) % len(bass_notes)
        freq = bass_notes[note_idx]
        b_start = int(step * 0.125 * SAMPLE_RATE)
        b_len = int(0.12 * SAMPLE_RATE)
        for i in range(b_len):
            idx = (b_start + i) % total_samples
            t = i / SAMPLE_RATE
            env = math.exp(-t * 20.0) * 0.24
            # Saw approximation
            saw = (math.sin(2.0 * math.pi * freq * t) + 
                   0.5 * math.sin(2.0 * math.pi * freq * 2.0 * t) + 
                   0.25 * math.sin(2.0 * math.pi * freq * 3.0 * t))
            samples[idx] += saw * env

    # 2. Four on the floor kick drum (every 0.5s)
    for b in range(8):
        kt = b * 0.5
        k_start = int(kt * SAMPLE_RATE)
        k_len = int(0.18 * SAMPLE_RATE)
        for i in range(k_len):
            idx = (k_start + i) % total_samples
            t = i / SAMPLE_RATE
            f = max(40.0, 180.0 * math.exp(-t * 35.0))
            env = math.exp(-t * 20.0)
            samples[idx] += math.sin(2.0 * math.pi * f * t) * env * 0.38

    # 3. Gated 80s Snare on beats 2, 4, 6, 8 (t = 0.5, 1.5, 2.5, 3.5)
    for b in [1, 3, 5, 7]:
        st = b * 0.5
        s_start = int(st * SAMPLE_RATE)
        s_len = int(0.24 * SAMPLE_RATE)
        for i in range(s_len):
            idx = (s_start + i) % total_samples
            t = i / SAMPLE_RATE
            env = math.exp(-t * 14.0)
            noise = (random.random() * 2.0 - 1.0) * 0.22 * env
            tone = math.sin(2.0 * math.pi * 260.0 * t) * 0.15 * math.exp(-t * 25.0)
            samples[idx] += noise + tone

    # 4. Glittering neon arpeggio (Am, F, C, G)
    arp_notes = [440.0, 523.25, 659.25, 880.0, 349.23, 440.0, 523.25, 698.46, 523.25, 659.25, 783.99, 1046.5, 392.0, 493.88, 587.33, 783.99]
    for step in range(32):
        freq = arp_notes[step % len(arp_notes)]
        a_start = int(step * 0.125 * SAMPLE_RATE)
        a_len = int(0.11 * SAMPLE_RATE)
        for i in range(a_len):
            idx = (a_start + i) % total_samples
            t = i / SAMPLE_RATE
            env = math.exp(-t * 24.0) * 0.12
            sine = math.sin(2.0 * math.pi * freq * t) + 0.3 * math.sin(2.0 * math.pi * freq * 2.0 * t)
            samples[idx] += sine * env

    return samples

def make_rain_ambience(duration=4.0):
    total_samples = int(SAMPLE_RATE * duration)
    samples = [0.0] * total_samples
    # 1. Pink-filtered rain bed
    b0, b1, b2 = 0.0, 0.0, 0.0
    for i in range(total_samples):
        white = (random.random() * 2.0 - 1.0)
        b0 = 0.99765 * b0 + white * 0.0990460
        b1 = 0.96300 * b1 + white * 0.1384000
        b2 = 0.57000 * b2 + white * 0.3029590
        pink = b0 + b1 + b2 + white * 0.5362
        samples[i] = pink * 0.055

    # 2. Individual rain drops hitting window pane
    num_drops = int(duration * 28)
    for _ in range(num_drops):
        start_idx = random.randint(0, total_samples - int(0.04 * SAMPLE_RATE))
        freq = random.uniform(1900.0, 3600.0)
        d_len = int(random.uniform(0.015, 0.035) * SAMPLE_RATE)
        amp = random.uniform(0.08, 0.22)
        for i in range(d_len):
            idx = (start_idx + i) % total_samples
            t = i / SAMPLE_RATE
            env = math.exp(-t * 180.0)
            samples[idx] += math.sin(2.0 * math.pi * freq * t) * env * amp

    # Crossfade loop boundaries (0.1s)
    xfade = int(0.1 * SAMPLE_RATE)
    for i in range(xfade):
        frac = i / float(xfade)
        samples[i] = samples[i] * frac + samples[total_samples - xfade + i] * (1.0 - frac)
        samples[total_samples - xfade + i] = samples[i]

    return samples

def make_night_ambience(duration=4.0):
    total_samples = int(SAMPLE_RATE * duration)
    samples = [0.0] * total_samples
    # 1. Warm low nocturnal hum (distant city resonance)
    for i in range(total_samples):
        t = i / SAMPLE_RATE
        lfo = 1.0 + 0.18 * math.sin(2.0 * math.pi * 0.35 * t)
        hum = (math.sin(2.0 * math.pi * 86.0 * t) * 0.06 +
               math.sin(2.0 * math.pi * 128.0 * t) * 0.04) * lfo
        samples[i] = hum

    # 2. Vinyl crackle & gentle pops
    for i in range(total_samples):
        if random.random() < 0.0018: # crackle pop
            pop_len = min(int(SAMPLE_RATE * 0.004), total_samples - i)
            pop_amp = random.uniform(0.08, 0.25)
            for p in range(pop_len):
                pt = p / SAMPLE_RATE
                samples[i + p] += (random.random() * 2.0 - 1.0) * math.exp(-pt * 900.0) * pop_amp

    # Crossfade loop boundaries (0.1s)
    xfade = int(0.1 * SAMPLE_RATE)
    for i in range(xfade):
        frac = i / float(xfade)
        samples[i] = samples[i] * frac + samples[total_samples - xfade + i] * (1.0 - frac)
        samples[total_samples - xfade + i] = samples[i]

    return samples

def make_quest_complete(duration=0.85):
    total_samples = int(SAMPLE_RATE * duration)
    samples = [0.0] * total_samples
    # Triumphant 4-note retro arpeggio: C5 (523Hz), E5 (659Hz), G5 (784Hz), C6 (1046Hz)
    notes = [(0.0, 523.25), (0.11, 659.25), (0.22, 783.99), (0.33, 1046.50)]
    for n_start, freq in notes:
        s_idx = int(n_start * SAMPLE_RATE)
        n_len = min(int((duration - n_start) * SAMPLE_RATE), total_samples - s_idx)
        for i in range(n_len):
            t = i / SAMPLE_RATE
            env = math.exp(-t * 6.5) * 0.32
            # Bright retro bell/chime harmonics
            sig = (math.sin(2.0 * math.pi * freq * t) +
                   0.45 * math.sin(2.0 * math.pi * freq * 2.0 * t) +
                   0.20 * math.sin(2.0 * math.pi * freq * 3.0 * t))
            samples[s_idx + i] += sig * env

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
save_wav(os.path.join(out_dir, "mouse_click.wav"), make_mouse_switch("classic"))
save_wav(os.path.join(out_dir, "mouse_click_classic.wav"), make_mouse_switch("classic"))
save_wav(os.path.join(out_dir, "mouse_click_optical.wav"), make_mouse_switch("optical"))
save_wav(os.path.join(out_dir, "mouse_click_silent.wav"), make_mouse_switch("silent"))
save_wav(os.path.join(out_dir, "pet_corgi_bark.wav"), make_corgi_bark())
save_wav(os.path.join(out_dir, "pet_robo_beep.wav"), make_robo_beep())
save_wav(os.path.join(out_dir, "streak_claim.wav"), make_streak_claim())
save_wav(os.path.join(out_dir, "lamp_switch.wav"), make_lamp_switch())
save_wav(os.path.join(out_dir, "bug_squash.wav"), make_bug_squash())
save_wav(os.path.join(out_dir, "crate_collect.wav"), make_crate_collect())
save_wav(os.path.join(out_dir, "build_complete.wav"), make_build_complete())
save_wav(os.path.join(out_dir, "cassette_click.wav"), make_cassette_click())
save_wav(os.path.join(out_dir, "lofi_chill_loop.wav"), make_lofi_track())
save_wav(os.path.join(out_dir, "synthwave_night_loop.wav"), make_synthwave_track())
save_wav(os.path.join(out_dir, "rain_ambience_loop.wav"), make_rain_ambience())
save_wav(os.path.join(out_dir, "night_ambience_loop.wav"), make_night_ambience())
save_wav(os.path.join(out_dir, "quest_complete.wav"), make_quest_complete())
print("All sound effects generated successfully!")
