# from scipy.signal import butter, lfilter
# from pylsl import StreamInlet, resolve_stream
# import numpy as np
# import matplotlib.pyplot as plt

# # Resolve EEG stream
# print("Looking for an LSL stream...")
# streams = resolve_stream()  # This resolves all available streams

# # Print available streams for identification
# for i, stream in enumerate(streams):
#     print(f"Stream {i}: Name={stream.name()}, Type={stream.type()}, Channels={stream.channel_count()}, Rate={stream.nominal_srate()}")

# # Select the EEG stream (stream index 1 based on your provided streams)
# print("\nConnecting to EEG stream...\n")
# eeg_inlet = StreamInlet(streams[3])  # Connect to the EEG stream

# def bandpass_filter(data, lowcut, highcut, fs, order=4):
#     nyquist = 0.5 * fs
#     low = lowcut / nyquist
#     high = highcut / nyquist
#     b, a = butter(order, [low, high], btype='band')
#     y = lfilter(b, a, data, axis=0)  # Ensure filtering along the correct axis
#     return y

# # Filter alpha band (8–13 Hz)
# fs = 256  # Sampling rate of Muse 2 EEG stream
# lowcut = 8  # Lower bound of the alpha band
# highcut = 13  # Upper bound of the alpha band

# # Pull data and apply bandpass filter
# print("Pulling and filtering data...")
# eeg_data = []
# timestamps = []

# # Collect a chunk of EEG data (e.g., 10 seconds worth)
# chunk_duration = 10  # seconds
# n_samples = int(fs * chunk_duration)

# for _ in range(n_samples):
#     sample, timestamp = eeg_inlet.pull_sample()  # Pull a single sample from the stream
#     eeg_data.append(sample)  # Append the sample (list of channel values)
#     timestamps.append(timestamp)

# eeg_data = np.array(eeg_data)  # Convert to numpy array
# timestamps = np.array(timestamps)

# # Apply bandpass filter to each channel
# filtered_data = bandpass_filter(eeg_data, lowcut, highcut, fs)

# # Plot one of the filtered channels (e.g., channel 0)
# plt.plot(timestamps, filtered_data[:, 0])  # Plot the first channel
# plt.xlabel("Time (s)")
# plt.ylabel("Amplitude")
# plt.title("Filtered EEG (Alpha Band, Channel 0)")
# plt.show()

################################################################################

# import numpy as np
# from scipy.signal import butter, lfilter, hilbert
# from pylsl import StreamInlet, resolve_stream, StreamInfo, StreamOutlet
# import matplotlib.pyplot as plt
# import time

# # Define frequency bands
# FREQUENCY_BANDS = {
#     'alpha': (8, 13),
#     'beta': (13, 30),
#     'gamma': (30, 100)
# }

# def bandpass_filter(data, lowcut, highcut, fs, order=4):
#     nyquist = 0.5 * fs
#     low = lowcut / nyquist
#     high = highcut / nyquist
#     b, a = butter(order, [low, high], btype='band')
#     y = lfilter(b, a, data, axis=0)
#     return y

# def compute_amplitude_envelope(filtered_data):
#     analytic_signal = hilbert(filtered_data, axis=0)
#     amplitude_envelope = np.abs(analytic_signal)
#     return amplitude_envelope

# def compute_fft(data, fs):
#     N = len(data)
#     fft_vals = np.fft.rfft(data)
#     fft_freq = np.fft.rfftfreq(N, 1/fs)
#     fft_magnitude = np.abs(fft_vals) / N
#     return fft_freq, fft_magnitude

# # Setup LSL outlets
# info_amplitude = StreamInfo('EEG_Amplitude', 'Amplitude', len(FREQUENCY_BANDS), 0, 'float32', 'uniqueid12345')
# amplitude_outlet = StreamOutlet(info_amplitude)

# info_frequency = StreamInfo('EEG_Frequency', 'Frequency', 257, 0, 'float32', 'uniqueid67890')  # 257 frequency bins for 512-point FFT
# frequency_outlet = StreamOutlet(info_frequency)

# # Resolve EEG stream
# print("Looking for an LSL EEG stream...")
# streams = resolve_stream('type', 'EEG')

# if not streams:
#     print("No EEG streams found.")
#     exit()

# print("\nAvailable EEG streams:")
# for i, stream in enumerate(streams):
#     print(f"Stream {i}: Name={stream.name()}, Type={stream.type()}, Channels={stream.channel_count()}, Rate={stream.nominal_srate()}")

# # Select the desired EEG stream
# stream_index = 0  # Adjust based on the available streams
# print(f"\nConnecting to EEG stream {stream_index}...\n")
# eeg_inlet = StreamInlet(streams[stream_index])

# # Filter settings
# fs = eeg_inlet.info().nominal_srate()
# if fs == 0:
#     fs = 256  # Default to 256 Hz if not available

# buffer_duration = 2  # seconds for amplitude envelope
# buffer_size = int(fs * buffer_duration)
# eeg_buffer = np.zeros((buffer_size, eeg_inlet.info().channel_count()))
# timestamp_buffer = np.zeros(buffer_size)

# # FFT settings
# fft_window_duration = 2  # seconds
# fft_window_size = int(fs * fft_window_duration)
# fft_buffer = np.zeros(fft_window_size)

# # Initialize buffers for amplitude envelopes
# amplitude_envelopes = {band: [] for band in FREQUENCY_BANDS.keys()}

# print("Starting real-time EEG processing...")
# try:
#     while True:
#         # Pull a chunk of samples
#         samples, timestamps = eeg_inlet.pull_chunk(timeout=1.0, max_samples=buffer_size)
#         if samples:
#             eeg_data = np.array(samples)
#             timestamps = np.array(timestamps)

#             # Update the buffer for amplitude envelopes
#             eeg_buffer = np.roll(eeg_buffer, -len(eeg_data), axis=0)
#             eeg_buffer[-len(eeg_data):, :] = eeg_data
#             timestamp_buffer = np.roll(timestamp_buffer, -len(timestamps))
#             timestamp_buffer[-len(timestamps):] = timestamps

#             # Update the FFT buffer
#             fft_buffer = np.roll(fft_buffer, -len(eeg_data), axis=0)
#             fft_buffer[-len(eeg_data):] = eeg_data[:, 0]  # Assuming channel 0 for FFT

#             # Process each frequency band for amplitude envelopes
#             current_amplitudes = []
#             for band_name, (low, high) in FREQUENCY_BANDS.items():
#                 # Apply bandpass filter
#                 filtered = bandpass_filter(eeg_buffer, low, high, fs)

#                 # Compute amplitude envelope using Hilbert Transform
#                 amplitude = compute_amplitude_envelope(filtered)

#                 # Compute average amplitude across channels
#                 avg_amplitude = np.mean(amplitude, axis=1)

#                 # Store the latest amplitude value
#                 latest_amplitude = avg_amplitude[-1]
#                 amplitude_envelopes[band_name].append(latest_amplitude)

#                 # Collect amplitudes for LSL outlet
#                 current_amplitudes.append(latest_amplitude)

#             # Send amplitude data via LSL
#             amplitude_outlet.push_sample(current_amplitudes)

#             # Compute FFT for frequency amplitude graph
#             if np.count_nonzero(fft_buffer) >= fft_window_size * 0.8:  # Ensure sufficient data
#                 window = np.hanning(fft_window_size)
#                 windowed_data = fft_buffer * window

#                 freqs, fft_mag = compute_fft(windowed_data, fs)

#                 fft_mag = fft_mag.tolist()

#                 # Send frequency data via LSL
#                 frequency_outlet.push_sample(fft_mag)

#                 print("Frequencies and Magnitudes sent.")

#             # Optional: Visualization for debugging
#             '''
#             plt.clf()
#             for band_name, amplitudes in amplitude_envelopes.items():
#                 plt.plot(amplitudes[-100:], label=band_name)
#             plt.legend()
#             plt.xlabel('Sample')
#             plt.ylabel('Amplitude')
#             plt.title('Real-Time Band Amplitudes')
#             plt.pause(0.01)
#             '''

#         else:
#             print("No samples received. Retrying...")
#             time.sleep(0.1)

# except KeyboardInterrupt:
#     print("Real-time EEG processing stopped.")

# # Close plots if using matplotlib
# # plt.close()


##########################################################

'''
beta_power = np.sum(fft_mag[(freqs >= 13) & (freqs <= 30)])
alpha_power = np.sum(fft_mag[(freqs >= 8) & (freqs <= 13)])
if alpha_power != 0:
    beta_alpha_ratio = beta_power / alpha_power
else:
    beta_alpha_ratio = 0

'''


import numpy as np
from scipy.signal import butter, lfilter, hilbert
from pylsl import StreamInlet, resolve_stream, StreamInfo, StreamOutlet
import matplotlib.pyplot as plt
import time

from collections import deque

# Define frequency bands
FREQUENCY_BANDS = {
    'alpha': (8, 13),
    'beta': (13, 30),
    'gamma': (30, 100)
}

def bandpass_filter(data, lowcut, highcut, fs, order=4):
    nyquist = 0.5 * fs
    low = lowcut / nyquist
    high = highcut / nyquist
    b, a = butter(order, [low, high], btype='band')
    y = lfilter(b, a, data, axis=0)
    return y

def compute_amplitude_envelope(filtered_data):
    analytic_signal = hilbert(filtered_data, axis=0)
    amplitude_envelope = np.abs(analytic_signal)
    return amplitude_envelope

def compute_fft(data, fs):
    N = len(data)
    fft_vals = np.fft.rfft(data)
    fft_freq = np.fft.rfftfreq(N, 1/fs)
    fft_magnitude = np.abs(fft_vals) / N
    return fft_freq, fft_magnitude

# Setup LSL outlets
info_amplitude = StreamInfo('EEG_Amplitude', 'Amplitude', len(FREQUENCY_BANDS), 0, 'float32', 'uniqueid12345')
amplitude_outlet = StreamOutlet(info_amplitude)

info_frequency = StreamInfo('EEG_Frequency', 'Frequency', 257, 0, 'float32', 'uniqueid67890')  # 257 frequency bins for a typical 512-point FFT
frequency_outlet = StreamOutlet(info_frequency)

# Resolve EEG stream
print("Looking for an LSL EEG stream...")
streams = resolve_stream('type', 'EEG')

if not streams:
    print("No EEG streams found.")
    exit()

print("\nAvailable EEG streams:")
for i, stream in enumerate(streams):
    print(f"Stream {i}: Name={stream.name()}, Type={stream.type()}, Channels={stream.channel_count()}, Rate={stream.nominal_srate()}")

# Select the desired EEG stream
stream_index = 0  # Adjust this if you have multiple streams
print(f"\nConnecting to EEG stream {stream_index}...\n")
eeg_inlet = StreamInlet(streams[stream_index])

# Filter settings
fs = eeg_inlet.info().nominal_srate()
if fs == 0:
    fs = 256  # default if not provided by stream

# For amplitude envelope
buffer_duration = 2  # seconds for amplitude envelope
buffer_size = int(fs * buffer_duration)
eeg_buffer = np.zeros((buffer_size, eeg_inlet.info().channel_count()))
timestamp_buffer = np.zeros(buffer_size)

# For FFT
fft_window_duration = 2  # seconds
fft_window_size = int(fs * fft_window_duration)
fft_buffer = np.zeros(fft_window_size)

# ---- Rolling Baseline Parameters ----
# e.g., 10 seconds for rolling baseline
baseline_window_size = int(10 * fs)

# We store a rolling deque for each band to compute baseline mean & std
rolling_baseline = {
    band: deque(maxlen=baseline_window_size) 
    for band in FREQUENCY_BANDS.keys()
}

# We'll store normalized amplitude envelopes here
amplitude_envelopes = {band: [] for band in FREQUENCY_BANDS.keys()}
freqs = None
fft_mag = None

# ---- SETUP MATPLOTLIB FOR REAL-TIME PLOTTING ----
plt.ion()  # Turn on interactive mode
fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(12, 5))

warm_up_seconds = 2
warm_up_samples = int(warm_up_seconds * fs)

total_samples_collected = 0  # track total number of samples

print("Starting real-time EEG processing...")
try:
    while True:
        # Pull a chunk of samples
        samples, timestamps = eeg_inlet.pull_chunk(timeout=1.0, max_samples=buffer_size)
        if samples:
            eeg_data = np.array(samples)
            timestamps = np.array(timestamps)

            # Update total samples for warm-up
            total_samples_collected += len(eeg_data)

            # Update the buffer for amplitude envelopes
            eeg_buffer = np.roll(eeg_buffer, -len(eeg_data), axis=0)
            eeg_buffer[-len(eeg_data):, :] = eeg_data
            timestamp_buffer = np.roll(timestamp_buffer, -len(timestamps))
            timestamp_buffer[-len(timestamps):] = timestamps

            # Update the FFT buffer (channel 0 for demonstration)
            fft_buffer = np.roll(fft_buffer, -len(eeg_data), axis=0)
            fft_buffer[-len(eeg_data):] = eeg_data[:, 0]

            # Process each frequency band for amplitude envelopes
            # But only do the z-score & LSL push if we've passed the warm-up.
            current_amplitudes = []
            if total_samples_collected > warm_up_samples:
                for band_name, (low, high) in FREQUENCY_BANDS.items():
                    # 1) Filter
                    filtered = bandpass_filter(eeg_buffer, low, high, fs)
                    # 2) Hilbert envelope
                    amplitude = compute_amplitude_envelope(filtered)
                    # 3) Average across channels
                    avg_amplitude = np.mean(amplitude, axis=1)
                    latest_amplitude = avg_amplitude[-1]

                    # Rolling baseline buffers + compute baseline stats
                    rolling_baseline[band_name].append(latest_amplitude)
                    if len(rolling_baseline[band_name]) > 1:
                        baseline_mean = np.mean(rolling_baseline[band_name])
                        baseline_std = np.std(rolling_baseline[band_name])
                    else:
                        baseline_mean = 0.0
                        baseline_std = 1.0

                    # Z-score
                    if baseline_std == 0:
                        normalized_val = latest_amplitude
                    else:
                        normalized_val = (latest_amplitude - baseline_mean) / baseline_std

                    amplitude_envelopes[band_name].append(normalized_val)
                    current_amplitudes.append(float(normalized_val))

                # Send amplitude data via LSL
                amplitude_outlet.push_sample(current_amplitudes)
            else:
                # Warm-up period: just fill rolling baseline, skip normalization/LSL
                for band_name, (low, high) in FREQUENCY_BANDS.items():
                    filtered = bandpass_filter(eeg_buffer, low, high, fs)
                    amplitude = compute_amplitude_envelope(filtered)
                    avg_amplitude = np.mean(amplitude, axis=1)
                    latest_amplitude = avg_amplitude[-1]
                    # Collect this data for baseline only
                    rolling_baseline[band_name].append(latest_amplitude)
                print(f"Warm-up in progress... {total_samples_collected} of {warm_up_samples} samples")

            # --- FFT Computation & LSL (optional) ---
            if total_samples_collected > warm_up_samples:
                if np.count_nonzero(fft_buffer) >= fft_window_size * 0.8:
                    window = np.hanning(fft_window_size)
                    windowed_data = fft_buffer * window
                    freqs, fft_mag = compute_fft(windowed_data, fs)
                    fft_out = fft_mag.tolist()
                    frequency_outlet.push_sample(fft_out)
                    print("Frequencies and Magnitudes sent.")

                # Plotting if beyond warm-up
                ax1.clear()
                ax2.clear()

                for band_name, norm_amplitudes in amplitude_envelopes.items():
                    ax1.plot(norm_amplitudes[-100:], label=band_name)
                ax1.set_title("Real-Time Band Amplitudes (Normalized)")
                ax1.set_xlabel("Sample (most recent 100)")
                ax1.set_ylabel("Z-scored Amplitude")
                ax1.legend(loc="upper right")

                if freqs is not None and fft_mag is not None:
                    ax2.plot(freqs, fft_mag)
                    ax2.set_title("Real-Time FFT (Channel 0)")
                    ax2.set_xlabel("Frequency (Hz)")
                    ax2.set_ylabel("Magnitude")
                
                    ax2.axvspan(8, 13, color='green', alpha=0.1, label='Alpha range')
                    ax2.axvspan(13, 30, color='blue', alpha=0.1, label='Beta range')
                    ax2.axvspan(30, 100, color='red', alpha=0.1, label='Gamma range')
                    
                    ax2.legend(loc="upper right")
                plt.tight_layout()
                plt.draw()
                plt.pause(0.01)

        else:
            print("No samples received. Retrying...")
            time.sleep(0.1)

except KeyboardInterrupt:
    print("Real-time EEG processing stopped.")

plt.close(fig)