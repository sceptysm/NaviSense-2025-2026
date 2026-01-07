## Setup
Clone the repository **outside of OneDrive** (e.g. `C:\Projects\`), as Unity and Git do not work reliably with synced folders.

This repository uses **Git Large File Storage (LFS)** to manage point clouds (`.ply`) and large 3D assets (e.g. `.fbx`), as defined in `.gitattributes`.

> **Note**: Active development currently happens on the `initial-upload` branch.
```bash
git clone https://github.com/sceptysm/NaviSense-2025-2026.git
cd NaviSense-2025-2026
git checkout initial-upload
```

If assets appear missing or corrupted, ensure Git LFS is installed and fetch the large files:
```bash
git lfs install
git lfs pull
```

### Unity Installation
1. Follow the official Unity installation guide: [https://unity.com/download](https://unity.com/download)
2. Install **Unity Hub**
3. In Unity Hub, go to **Installs** &#8594; **Install Editor**
4. Select **Unity Editor Version** `6000.2.14f1`

### Opening the Unity Project
1. Open **Unity Hub**
2. Go to the **Projects** tab
3. Click **Add** &#8594; "Add project from disk"
4. Select the repository folder `NaviSense-2025-2026`
5. Open the project

> The first launch may take several minutes while Unity imports assets and installs required packages.

### Opening the Navigation Scene
Once the project has opened:
1. In Unity, go to **File** &#8594; **Open Scene**
2. Open: `Assets/Scenes/Navigation/NavMesh_custom`
3. The point cloud and navigation mesh plane should now be visible in the Scene view.
