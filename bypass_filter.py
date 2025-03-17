import os
import numpy as np
import pandas as pd
import tkinter as tk
import matplotlib
matplotlib.use('TkAgg')
import matplotlib.pyplot as plt
import time

from scipy.signal import butter, lfilter, hilbert
from pylsl import StreamInlet, resolve_stream, StreamInfo, StreamOutlet
from collections import deque
from scipy.fft import rfft, rfftfreq
from datetime import datetime

from tcp_client import UnityTCPClient


BETA_ALPHA_RATIO_LIMIT = 1.0 # modify according to user


# ------------------------------------------------------------------------------------
# 1) Create a dictionary to hold session data
# ------------------------------------------------------------------------------------
session_data = {
    "raw_eeg": [],          # will store raw samples: [timestamp, ch1, ch2, ch3, ch4, ...]
    "processed_eeg": [],    # will store processed info: [timestamp, alphaZ, betaZ, gammaZ, ratio, etc.]
    "baseline_samples": [], # optional if you want to store raw amplitude from warm-up
    "baseline_stats": {},   # final mean/std for each band
}

_root = tk.Tk()
_root.withdraw()

fig, (ax1, ax2, ax3) = plt.subplots(1, 3, figsize=(15, 5))
plt.ion()  # Enable interactive mode
"""
Size Range: 30-35 cm, ear to ear
Wireless Connection: Bluetooth 4.2
Charging Port: Micro-USB
Battery: Rechargeable Li-ion
Charge Time: 3 hours
Usage Time: 5 hours, or 10-15 days with 20-min daily use
EEG Channels: 4 EEG channels + 2 amplified Aux channels
Sample Rate: 256 Hz
Sample Resolution: 12 bits / sample
Reference Electrode Position: FPz (CMS/DRL)
Channel Electrode Position: TP9, AF7, AF8, TP10 (dry)
Materials: Flexible Polycarbonate & Conductive Silicone
EEG Electrode Materials: Conductive Gold
Weight: 38.5g
Accelerometer: Three-axis @ 52Hz, 16 bit resolution, range +/ - 4G; 52 Hz sample rate; 16 bit / sample resolution
PPG: 3 LEDs: IR, IR, red; 64 Hz sample rate; 16-bit resolution
Aux Channel Connection: 1.5mV p- p AC coupled signal
Muse App Compatibility: iOS 13, Android 8 or higher, Huawei devices not supported
"""

# question1 : do i need to worry about phase distortion
# question 2: is my thought process of nyquist correct? 

# ====================================================================================
# EEG Frequency Bands Definition
# ====================================================================================

FREQUENCY_BANDS = {
    # 'Delta': (0.5, 4),
    # 'Theta': (4, 8),
    'Alpha': (8, 13),
    'Beta': (13, 30),
    'Gamma': (30, 100)
}

CHARGE_DELAY = 5  # seconds
last_charge_time = 0  # Track last time a charge was added

def is_window_minimized():
    """Check if the Matplotlib figure window is minimized (iconified)."""
    try:
        return fig.canvas.manager.window.state() == 'iconic'
    except AttributeError:
        return False
    
def bandpass_filter(data, lowcut, highcut, fs, order=4):
    """
    Apply a bandpass filter to the input data.
    
    Args:
        data (np.array): Input EEG data
        lowcut (float): Lower cutoff frequency
        highcut (float): Upper cutoff frequency
        fs (float): Sampling frequency
        order (int): Filter order
        
    Returns:
        np.array: Filtered data
    """
    nyquist = 0.5 * fs #since our sampling rate is 256, our max freq should be 256/2=128, and we are drawing 100hz which is within the limit
    low = lowcut / nyquist #normalzing cutoff freq to 0-1
    high = highcut / nyquist #you know that cant exceed nyquist, so normalizing
    b, a = butter(order, [low, high], btype='band') #designing bandpass filter
    y = lfilter(b, a, data, axis=0) #applying the  IIR (Infinite Impulse Response) filter, meaning past outputs influence future outputs. may have phase distortions
    return y

def compute_amplitude_envelope(filtered_data):
    """
    Compute the amplitude envelope using the Hilbert transform.
    
    Args:
        filtered_data (np.array): Bandpass-filtered EEG data
        
    Returns:
        np.array: Amplitude envelope
    """
    analytic_signal = hilbert(filtered_data, axis=0)
    amplitude_envelope = np.abs(analytic_signal)
    return amplitude_envelope

def compute_fft(data, fs):
    """
    Compute the FFT of the EEG signal.
    
    Args:
        data (np.array): Input EEG data.
        fs (float): Sampling frequency (Hz).
    
    Returns:
        tuple: (frequencies, FFT magnitudes)
    """
    N = len(data)
    fft_vals = rfft(data)  # SciPy's optimized FFT
    fft_freq = rfftfreq(N, 1/fs)  # SciPy's optimized frequency computation
    fft_magnitude = np.abs(fft_vals) / N
    return fft_freq, fft_magnitude

# ------------------------------------------------------------------------------------
# 2) Function to save session data at the end
# ------------------------------------------------------------------------------------
def save_session_data(session_data):
    """
    Saves raw and processed EEG data to CSV files in a folder called 'EEG_Data'.
    Filenames will be timestamped to avoid overwriting.
    """
    folder_name = "EEG_Data"
    os.makedirs(folder_name, exist_ok=True)

    timestamp_str = datetime.now().strftime("%Y%m%d_%H%M%S")

    # 1. Save raw EEG
    raw_filename = os.path.join(folder_name, f"session_{timestamp_str}_raw.csv")
    if len(session_data["raw_eeg"]) > 0:
        df_raw = pd.DataFrame(session_data["raw_eeg"],
            columns=["timestamp"] + [f"ch{i}" for i in range(1, len(session_data["raw_eeg"][0]))]
        )
        df_raw.to_csv(raw_filename, index=False)
        print(f"Raw EEG data saved to {raw_filename}")
    else:
        print("No raw EEG data collected, skipping raw CSV file.")

    # 2. Save processed EEG
    processed_filename = os.path.join(folder_name, f"session_{timestamp_str}_processed.csv")
    if len(session_data["processed_eeg"]) > 0:
        df_proc = pd.DataFrame(session_data["processed_eeg"],
            columns=["timestamp", "alpha_z", "beta_z", "gamma_z", "beta_alpha_ratio"]
        )
        df_proc.to_csv(processed_filename, index=False)
        print(f"Processed EEG data saved to {processed_filename}")
    else:
        print("No processed EEG data collected, skipping processed CSV file.")

    # 3. Optionally, save baseline stats
    #    (If you want them in a separate CSV, or just print them out.)
    print("Baseline stats:")
    for band, stats in session_data["baseline_stats"].items():
        print(f"  {band}: mean={stats['mean']:.4f}, std={stats['std']:.4f}")

    print("Session data saved successfully.")

# ------------------------------------------------------------------------------------
# 3) Connect to Unity (TCP)
# ------------------------------------------------------------------------------------
try:
    unity_client = UnityTCPClient(host="127.0.0.1", port=5005)
    unity_client.connect()
    print("Connected to Unity TCP server.")
except Exception as e:
    print("Error connecting to Unity:", e)
    unity_client = None

print("Looking for an LSL EEG stream...")
streams = resolve_stream('type', 'EEG')
if not streams:
    print("No EEG streams found.")
    exit()

print("\nAvailable EEG streams:")
for i, st in enumerate(streams):
    print(f"Stream {i}: Name={st.name()}, Type={st.type()}, Channels={st.channel_count()}, Rate={st.nominal_srate()}")

stream_index = 0
print(f"\nConnecting to EEG stream {stream_index}...\n")
eeg_inlet = StreamInlet(streams[stream_index])

# Create LSL Outlets for processed data
info_amplitude = StreamInfo('EEG_Amplitude', 'Amplitude', len(FREQUENCY_BANDS), 0, 'float32', 'uniqueid12345')
amplitude_outlet = StreamOutlet(info_amplitude)

info_frequency = StreamInfo('EEG_Frequency', 'Frequency', 257, 0, 'float32', 'uniqueid67890')
frequency_outlet = StreamOutlet(info_frequency)
print("Receiving EEG data and processing...")
script_start_time = time.time()  # Store the start time when script begins

fs = eeg_inlet.info().nominal_srate()
if fs == 0:
    fs = 256  # default for Muse2

"""
We allocate buffers to hold 2 seconds of EEG data in memory

eeg_buffer store multi channel data for amplitude filtering

fft_buffer is for single channel that we want to run FFT on
"""
buffer_duration = 2  # Buffer duration in seconds
buffer_size = int(fs * buffer_duration)
eeg_buffer = np.zeros((buffer_size, eeg_inlet.info().channel_count()))

fft_window_duration = 2
fft_window_size = int(fs * fft_window_duration)
fft_buffer = np.zeros(fft_window_size)

# Rolling baseline
baseline_window_size = int(10 * fs)
rolling_baseline = {
    band: deque(maxlen=baseline_window_size)
    for band in FREQUENCY_BANDS.keys()
}

amplitude_envelopes = {band: [] for band in FREQUENCY_BANDS.keys()}
beta_alpha_ratio_list = []
freqs = None
fft_mag = None

warm_up_seconds = 2
warm_up_samples = int(warm_up_seconds * fs)
total_samples_collected = 0

print("Starting real-time EEG processing... (Ctrl+C to stop)")

try:
    while True:
        samples, timestamps = eeg_inlet.pull_chunk(timeout=1.0, max_samples=buffer_size)
        if samples:
            eeg_data = np.array(samples)
            chunk_len = len(eeg_data)
            total_samples_collected += chunk_len

            # -----------------------------
            # (A) Store raw EEG data
            # -----------------------------
            # For each sample, we store [timestamp, ch1, ch2, ...]
            for i in range(chunk_len):
                row = [timestamps[i] - script_start_time]  # Now relative to script start
                row.extend(eeg_data[i, :])  # all channels
                session_data["raw_eeg"].append(row)

            # -----------------------------
            # (B) Roll buffers
            # -----------------------------
            eeg_buffer = np.roll(eeg_buffer, -chunk_len, axis=0)
            eeg_buffer[-chunk_len:, :] = eeg_data

            fft_buffer = np.roll(fft_buffer, -chunk_len, axis=0)
            fft_buffer[-chunk_len:] = eeg_data[:, 0]  # single channel for FFT

            # -----------------------------
            # (C) Warm-up or real-time processing
            # -----------------------------
            if total_samples_collected > warm_up_samples:
                current_amplitudes = []

                # Real-time band analysis
                for band_name, (low, high) in FREQUENCY_BANDS.items():
                    """
                    we process each band
                    1. filter entire buffer to isolate that band
                    2. compute hilbert amplitude to find out how strong the band activity is
                    3. take an average across channels
                    4. take the latest amplitude
                    5. update rolling baseline for that band, compute baseline mean and stdev
                    6. normalize (zscore that amplitude)
                    7. store for plot and push to LSL
                    """
                    filtered = bandpass_filter(eeg_buffer, low, high, fs)
                    envelope = compute_amplitude_envelope(filtered)
                    avg_env = np.mean(envelope, axis=1)
                    latest_amp = avg_env[-1]

                    # Rolling baseline for Z-scoring
                    rolling_baseline[band_name].append(latest_amp)
                    baseline_mean = np.mean(rolling_baseline[band_name]) if len(rolling_baseline[band_name]) > 1 else 0
                    baseline_std  = np.std(rolling_baseline[band_name])  if len(rolling_baseline[band_name]) > 1 else 1

                    if baseline_std == 0:
                        normalized_val = latest_amp
                    else:
                        normalized_val = (latest_amp - baseline_mean) / baseline_std

                    amplitude_envelopes[band_name].append(normalized_val)
                    current_amplitudes.append(float(normalized_val))

                # Push amplitude data via LSL
                amplitude_outlet.push_sample(current_amplitudes)

                # Beta/Alpha ratio
                if 'Alpha' in amplitude_envelopes and 'Beta' in amplitude_envelopes:
                    if len(amplitude_envelopes['Alpha']) > 0 and len(amplitude_envelopes['Beta']) > 0:
                        alpha_val = amplitude_envelopes['Alpha'][-1]
                        beta_val  = amplitude_envelopes['Beta'][-1]
                        ratio = beta_val / alpha_val if alpha_val != 0 else 0.0
                        beta_alpha_ratio_list.append(ratio)

                        # -----------------------------
                        # (D) Send ratio to Unity
                        # -----------------------------
                        if unity_client is not None:
                            unity_client.send(f"{ratio}")

                        # If ratio > 1 => "AddLightningCharge"
                        current_time = time.time()
                        if ratio > BETA_ALPHA_RATIO_LIMIT and unity_client is not None:
                            if current_time - last_charge_time >= CHARGE_DELAY:
                                last_charge_time = current_time
                                try:
                                    unity_client.send("AddLightningCharge")
                                except Exception as e:
                                    print("Error sending AddLightningCharge to Unity:", e)

                # Store the processed data for this chunk
                # We can store just the last sample's data as a time snapshot
                last_ts = timestamps[-1] - script_start_time  # relative timestamp for the chunk's last sample
                # alpha_z, beta_z, gamma_z from "current_amplitudes" in that order
                # or you can do a dictionary approach; for clarity, let's do a fixed order:
                alpha_z = current_amplitudes[0] if len(current_amplitudes) > 0 else 0
                beta_z  = current_amplitudes[1] if len(current_amplitudes) > 1 else 0
                gamma_z = current_amplitudes[2] if len(current_amplitudes) > 2 else 0
                b_a_ratio = beta_alpha_ratio_list[-1] if len(beta_alpha_ratio_list) > 0 else 0
                session_data["processed_eeg"].append([
                    last_ts, alpha_z, beta_z, gamma_z, b_a_ratio
                ])

                # Compute FFT occasionally
                if np.count_nonzero(fft_buffer) >= fft_window_size * 0.8:
                    window = np.hanning(fft_window_size)
                    windowed_data = fft_buffer * window
                    freqs, fft_mag = compute_fft(windowed_data, fs)
                    frequency_outlet.push_sample(fft_mag.tolist())

            else:
                # Warm-up in progress
                print(f"Warm-up in progress... {total_samples_collected}/{warm_up_samples} samples")
                # Collect baseline amplitude
                for band_name, (low, high) in FREQUENCY_BANDS.items():
                    filtered = bandpass_filter(eeg_buffer, low, high, fs)
                    envelope = compute_amplitude_envelope(filtered)
                    avg_env = np.mean(envelope, axis=1)
                    latest_amp = avg_env[-1]
                    session_data["baseline_samples"].append((band_name, latest_amp))
                    rolling_baseline[band_name].append(latest_amp)

        else:
            print("No samples received. Retrying...")
            time.sleep(0.1)
            continue

        # Plot
        if is_window_minimized():
            plt.pause(0.3)
            continue

        if total_samples_collected > warm_up_samples:
            ax1.clear()
            ax2.clear()
            ax3.clear()

            # ax1: Plot band amplitudes (most recent ~100 points)
            for band_name, vals in amplitude_envelopes.items():
                ax1.plot(vals[-100:], label=band_name)
            ax1.set_title("Real-Time Band Amplitudes (Z-scored)")
            ax1.set_xlabel("Samples (last 100)")
            ax1.set_ylabel("Amplitude (z-score)")
            ax1.legend(loc="upper right")

            # ax2: Plot FFT if available
            if freqs is not None and fft_mag is not None:
                ax2.plot(freqs, fft_mag, label="FFT Mag")
                ax2.set_title("Real-Time FFT (Channel 0)")
                ax2.set_xlabel("Frequency (Hz)")
                ax2.set_ylabel("Magnitude")
                ax2.legend(loc="upper right")

            # ax3: Plot Beta/Alpha ratio
            ax3.plot(beta_alpha_ratio_list[-100:], label="Beta/Alpha Ratio", color='orange')
            ax3.set_title("Beta/Alpha Ratio")
            ax3.set_xlabel("Samples (last 100)")
            ax3.set_ylabel("Ratio")
            ax3.axhline(y=1.0, color='red', linestyle='--', label='Ratio=1')
            ax3.legend(loc="upper right")

            plt.tight_layout()
            plt.draw()
            plt.pause(0.01)

except KeyboardInterrupt:
    print("Real-time EEG processing stopped by user.")
finally:
    # --------------------------------------------------------------------------------
    # 4) Finalize baseline stats => store in session_data["baseline_stats"]
    # --------------------------------------------------------------------------------
    for band_name in FREQUENCY_BANDS.keys():
        arr = np.array(rolling_baseline[band_name], dtype=float)
        if len(arr) > 1:
            session_data["baseline_stats"][band_name] = {
                "mean": float(np.mean(arr)),
                "std":  float(np.std(arr))
            }
        else:
            session_data["baseline_stats"][band_name] = {"mean": 0.0, "std": 1.0}

    # --------------------------------------------------------------------------------
    # 5) SAVE session data to CSV
    # --------------------------------------------------------------------------------
    save_session_data(session_data)

    # Clean up
    plt.close(fig)
    if unity_client is not None:
        unity_client.close()
