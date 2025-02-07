# How Presets Work

---

## 1. Folder Structure

Each preset is a self-contained folder under the `presets/` directory. It includes:

- **`preset-config.yaml`**: Defines preset metadata, attributes, and modified files.
- **YAML configuration files** (e.g., `wfb.yaml`, `majestic.yaml`).
- **Optional `sensor/` folder**: Includes binary files like `milos-sensor.bin`.

**Example Structure**:
```
presets/
├── high_power_fpv/
│   ├── preset-config.yaml
│   ├── wfb.yaml
│   ├── majestic.yaml
│   ├── sensor/
│       └── milos-sensor.bin
```

---

## 2. Preset Definition (`preset-config.yaml`)

The `preset-config.yaml` file defines:

- **Metadata**: `name`, `author`, `description`, and `category`.
- **Optional Sensor**: Specifies a binary file (e.g., `milos-sensor.bin`) to be transferred to the remote device.
- **Files**: Specifies files and their key-value modifications.

**Example**:
```yaml
name: "High Power FPV"
author: "OpenIPC"
description: "Optimized settings for high-power FPV."
sensor: "milos-sensor.bin"
files:
  wfb.yaml:
    wireless.txpower: "30"
    wireless.channel: "161"
  majestic.yaml:
    fpv.enabled: "true"
    system.logLevel: "info"
```

---

## 3. Preset Loading

- The application scans the `presets/` directory.
- It parses each `preset-config.yaml` to create a `Preset` object.
- File modifications are transformed into a bindable `ObservableCollection<FileModification>` for the UI.

---

## 4. Applying Presets

When a preset is applied:

1. **Sensor File Transfer**:
    - If a sensor is specified, it is transferred using **SCP**:
      ```bash
      scp presets/high_power_fpv/sensor/milos-sensor.bin user@remote:/etc/sensors/milos-sensor.bin
      ```

2. **File Modifications**:
    - Each attribute in `files` is applied using **`yaml-cli`**:
      ```bash
      yaml-cli -i /etc/wfb.yaml "wireless.txpower" "30"
      yaml-cli -i /etc/wfb.yaml "wireless.channel" "161"
      yaml-cli -i /etc/majestic.yaml "fpv.enabled" "true"
      yaml-cli -i /etc/majestic.yaml "system.logLevel" "info"
      ```

3. **Logs**:
    - Logs success or failure of sensor transfer and YAML modifications.

---

## 5. UI Workflow

- **Preset List**:
    - Displays all available presets using a `ListBox`.
    - Users can select a preset by its name.

- **Details Panel**:
    - Displays metadata (`Name`, `Author`, `Description`).
    - Lists file modifications and sensor file.

- **"Apply Preset" Button**:
    - Applies the selected preset’s changes to the remote device.
    - Button is enabled only if a preset is selected.

---

## Key Features

- **Dynamic Preset Management**:
    - Add/remove presets by simply editing the `presets/` directory.
- **File Abstraction**:
    - Presets only define attributes; the app handles file locations.
- **Sensor File Handling**:
    - Automatically transfers sensor binaries if specified.
- **User-Friendly UI**:
    - Select a preset, view details, and apply it with a single click.

