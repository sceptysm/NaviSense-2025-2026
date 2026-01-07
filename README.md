# NaviSense 2025-2026

## Project Overview
This project utilizes the **[Immersal SDK](https://developers.immersal.com/docs/immersal-sdk/)**, originally designed for large-scale AR experiences. By leveraging the underlying 6 DoF (Degrees of Freedom) localization provided by **Google ARCore** and **Apple ARKit**, this project repurposes the SDK to provide cross-platform indoor navigation via Computer Vision.

### Licensing & Constraints
The project currently operates under the **Immersal Free Tier**, which carries the following constraints:
* **Capacity:** Maximum of 100 images per map/scene.
* **Usage:** Suitable for development and small-scale testing.

Detailed information regarding plans and common questions can be found in the official documentation:
* **[Pricing & Plan Details](https://developers.immersal.com/docs/immersal-sdk/pricing/)**
* **[Immersal SDK FAQ](https://developers.immersal.com/docs/immersal-sdk/faq/)**

### Reference Documentation
The navigation logic in this project is based on the official Immersal "Navigation Sample NavMesh". This system functions by manually aligning a 3D mesh (floor map) to a pre-mapped point cloud of the environment; this mesh then serves as the navigation surface for pathfinding via the A* algorithm.

To successfully set up, map, and deploy the project for a new environment, **follow this official tutorial:**
> [Navigation Sample NavMesh Setup](https://developers.immersal.com/docs/unitysdk/samplescenes/thenavigationsamplenavmesh/)

*This guide is essential for understanding the baseline mapping, localization, and navigation workflow we are building upon for blind navigation.*

## Setup
Clone the repository **outside of OneDrive** (e.g. `C:\Projects\`), as Unity and Git do not work reliably with synced folders.

This repository uses **Git Large File Storage (LFS)** to manage point clouds (`.ply`) and large 3D assets (e.g. `.fbx`), as defined in `.gitattributes`.

```bash
git clone https://github.com/sceptysm/NaviSense-2025-2026.git
cd NaviSense-2025-2026
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
