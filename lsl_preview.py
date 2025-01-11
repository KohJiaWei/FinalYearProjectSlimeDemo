import time
from pylsl import StreamInlet, resolve_stream
import numpy as np
import matplotlib.pyplot as plt

# Resolve EEG stream (or any specific stream by its name/type)
print("Looking for an LSL stream...")
streams = resolve_stream()  # This resolves all available streams


# Print available streams for identification
for i, stream in enumerate(streams):
    print(f"Stream {i}: Name={stream.name()}, Type={stream.type()}, Channels={stream.channel_count()}, Rate={stream.nominal_srate()}")

# Select the third stream for EEG
print("\nConnecting to EEG stream...\n")
inlet = StreamInlet(streams[3])

# Data collection parameters
duration = 10  
data_buffer = []

print(f"Collecting data for {duration} seconds...")
start_time = time.time()
while time.time() - start_time < duration:
    sample, timestamp = inlet.pull_sample()  # Pull a sample from the stream
    data_buffer.append((timestamp, sample))

print("Data collection complete.\n")

# Extract timestamps and samples
timestamps, samples = zip(*data_buffer)
samples = np.array(samples)

# Normalize the samples
# Compute mean and standard deviation for each channel
means = np.mean(samples, axis=0)
stds = np.std(samples, axis=0)
normalized_samples = (samples - means) / stds

# Plot waveform
plt.figure(figsize=(10, 6))
for i in range(normalized_samples.shape[1]):  # Loop through each channel
    plt.plot(timestamps, normalized_samples[:, i], label=f"Channel {i+1}")
plt.xlabel("Time (s)")
plt.ylabel("Normalized Amplitude")
plt.title("Normalized Waveform Preview")
plt.legend()
plt.grid()
plt.show()
